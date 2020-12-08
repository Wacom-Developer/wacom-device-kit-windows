/*----------------------------------------------------------------------------

	NAME
		Cadtest.cpp

	PURPOSE
		Example of how a cad program might use WinTab.

	COPYRIGHT
		This file is Copyright (c) Wacom Company, Ltd. 2020 All Rights Reserved
		with portions copyright 1998 by LCS/Telegraphics.

		The text and information contained in this file may be freely used,
		copied, or distributed without compensation or licensing restrictions.

	NOTES
		This app has been configured for DPI Awareness = Per Monitor High DPI Aware
		See: https://msdn.microsoft.com/en-us/library/windows/desktop/dn280512(v=vs.85).aspx

---------------------------------------------------------------------------- */

#include <map>
#include <string.h>
#include <windows.h>
#include "winuser.h"
#include <commdlg.h>
#include "msgpack.h"
#include "Utils.h"
#include "cadtest.h"
#include "rule.h"

HINSTANCE hInst = NULL;

#ifdef WIN32
	#define GetID()	GetCurrentProcessId()
#else
	#define GetID()	hInst
#endif

char*	gpszProgramName = "CadTest";

static int gnOpenContexts = 0;
static int gnAttachedDevices = 0;


void CloseContexts(void);

// --------------------------------------------------------------------------
// Hold tablet-specific properties used when responding to tablet data packets.
//
struct CadTabletInfo
{
	LONG		tabletXExt;
	LONG		tabletYExt;
	bool		displayTablet;
};

// --------------------------------------------------------------------------
// Cache all opened contexts for attached tablets.  
// This will allow us to close them when the window closes down.
//
typedef std::map<HCTX, CadTabletInfo> CtxMap;
static CtxMap g_contextMap;

// --------------------------------------------------------------------------
int __stdcall WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
	MSG msg;

	if (!hPrevInstance)
	{
		if (!InitApplication(hInstance))
		{
			return (FALSE);
		}
	}

	// Perform initializations that apply to a specific instance

	if (!InitInstance(hInstance, nCmdShow))
		return (FALSE);

	// Acquire and dispatch messages until a WM_QUIT message is received.

	while (GetMessage(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	// Return Wintab resources.
	Cleanup();

	return (msg.wParam);
}


// --------------------------------------------------------------------------
BOOL InitApplication(HINSTANCE hInstance)
{
	WNDCLASS  wc;

	// Fill in window class structure with parameters that describe the
	// main window.

	wc.style = 0;
	wc.lpfnWndProc = MainWndProc;

	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = hInstance;
	wc.hIcon = LoadIcon(NULL, IDI_APPLICATION);
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);
	wc.hbrBackground = (HBRUSH)(COLOR_APPWORKSPACE + 1);
	wc.lpszMenuName =  "CadTestMenu";
	wc.lpszClassName = "CadTestWClass";

	// Register the window class and return success/failure code.

	return (RegisterClass(&wc));

}


// --------------------------------------------------------------------------
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
	HWND            hWnd;
	char buf[50];

	// Save the instance handle in static variable, which will be used in
	// many subsequence calls from this application to Windows.

	hInst = hInstance;

	if ( !LoadWintab( ) )
	{
		ShowError( "Wintab not available" );
		return FALSE;
	}

	/* check if WinTab available. */
	if (!gpWTInfoA(0, 0, NULL)) {
		MessageBox(NULL, "WinTab Services Not Available.", gpszProgramName,
					MB_OK | MB_ICONHAND);
		UnloadWintab();
		return FALSE;
	}

	// Create a main window for this application instance. 
	wsprintf(buf, "CadTest:%x", GetID());
	hWnd = CreateWindow(
		"CadTestWClass",
		buf,
		WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		NULL,
		NULL,
		hInstance,
		NULL
	);

	// If window could not be created, return "failure"
	if (!hWnd)
	{
		UnloadWintab();
		return (FALSE);
	}

	// Make the window visible; update its client area; and return "success"
	ShowWindow(hWnd, nCmdShow);
	UpdateWindow(hWnd);
	return (TRUE);

}


// --------------------------------------------------------------------------
static bool TabletInit(HWND hWnd)
{
	int ctxIndex = 0;
	gnOpenContexts = 0;
	gnAttachedDevices = 0;
	g_contextMap.clear();
	
	gnAttachedDevices = 0;
	gpWTInfoA(WTI_INTERFACE, IFC_NDEVICES, &gnAttachedDevices);
	WacomTrace("Number of attached devices: %i\n", gnAttachedDevices);

	do
	{
		LOGCONTEXT lcMine = { 0 };
		int foundCtx = 0;

		foundCtx = gpWTInfoA(WTI_DDCTXS + ctxIndex, 0, &lcMine);

		if (foundCtx > 0)
		{
			UINT result = 0;
			UINT wWTInfoRetVal = gpWTInfoA(WTI_DEVICES + ctxIndex, DVC_HARDWARE, &result);
			bool displayTablet = result & HWC_INTEGRATED;

			/* modify the digitizing region */
			WacomTrace(lcMine.lcName, "CadTest Digitizing %x", GetID());
			lcMine.lcOptions |= CXO_MESSAGES;
			lcMine.lcMsgBase = WT_DEFBASE;
			lcMine.lcPktData = PACKETDATA;
			lcMine.lcPktMode = PACKETMODE;
			lcMine.lcMoveMask = PACKETDATA;
			lcMine.lcBtnUpMask = lcMine.lcBtnDnMask;

			lcMine.lcOutOrgX = lcMine.lcOutOrgY = 0;

			AXIS tabletX = { 0 };
			AXIS tabletY = { 0 };

			gpWTInfoA(WTI_DEVICES + ctxIndex, DVC_X, &tabletX);
			gpWTInfoA(WTI_DEVICES + ctxIndex, DVC_Y, &tabletY);

			// This prevents outputted display-tablet coordinates
			// range from being mapped to full desktop, which
			// causes problems in multi-screen set-ups. Ie, without this
			// then the tablet coord. range is mapped to full desktop, intead
			// of only the display tablet active area.
			{
				lcMine.lcOutExtX = tabletX.axMax - tabletX.axMin + 2;
				lcMine.lcOutExtY = (tabletY.axMax - tabletY.axMin + 1);
			}

			LONG xTbltExt = lcMine.lcOutExtX;
			LONG yTbltExt = lcMine.lcOutExtY;

			HCTX hCtx = gpWTOpenA(hWnd, &lcMine, true);

			if (hCtx)
			{
				CadTabletInfo info;
				info.tabletXExt = xTbltExt;
				info.tabletYExt = yTbltExt;
				info.displayTablet = displayTablet;
				g_contextMap[hCtx] = info;
				WacomTrace("Opened context: 0x%X for ctxIndex: %i\n", hCtx, ctxIndex);
				gnOpenContexts++;
			}
			else
			{
				WacomTrace("Did NOT open context for ctxIndex: %i\n", ctxIndex);
			}
		}
		else
		{
			break;
		}

		ctxIndex++;
	} while (true);

	return (gnAttachedDevices > 0);

}


// --------------------------------------------------------------------------
LRESULT FAR PASCAL MainWndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	DLGPROC lpProcAbout = NULL;
	DLGPROC lpProcRuler = NULL;
	// static HCTX hTab = NULL;
	static POINT ptOld, ptNew;
	static RECT rcClient;
	static HCTX hctx = NULL;
	PAINTSTRUCT psPaint;
	HDC hDC;
	BOOL fHandled = TRUE;
	LRESULT lResult = 0L;
	static int count;
	static BOOL fPersist;
	static MONITORINFO monInfo = { 0 };
	static const MONITORINFO EmptyMonInfo = { 0 };
	static LONG monWidth = 0;
	static LONG monHeight = 0;

	switch (message) {

		case WM_CREATE:
			if (!TabletInit(hWnd)) {
				MessageBox(NULL, " Could Not Open Tablet Context.", gpszProgramName,
							MB_OK | MB_ICONHAND);

				SendMessage(hWnd, WM_DESTROY, 0, 0L);
			}
			break;

		case WM_MOVE:
		case WM_SIZE:
		{
			GetClientRect(hWnd, &rcClient);

			{
				HMONITOR hMon = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
				monInfo = EmptyMonInfo;
				monInfo.cbSize = sizeof(MONITORINFO);
				GetMonitorInfo(hMon, &monInfo);
				monWidth = monInfo.rcMonitor.right - monInfo.rcMonitor.left;
				monHeight = monInfo.rcMonitor.bottom - monInfo.rcMonitor.top;
			}

			InvalidateRect(hWnd, NULL, TRUE);
			break;
		}
			

		case WM_COMMAND:
			switch (GET_WM_COMMAND_ID(wParam, lParam)) 
			{
			case IDM_ABOUT:
				lpProcAbout = MakeProcInstance(reinterpret_cast<BOOL(__stdcall*)(HWND, unsigned int, WPARAM, LPARAM)>(AboutProc), hInst);
				DialogBox(hInst, "AboutBox", hWnd, (DLGPROC)lpProcAbout);
				FreeProcInstance(lpProcAbout);
				break;

			case IDM_CONFIG:
				gpWTConfig(hctx, hWnd);
				break;

			case IDM_PERSIST:
				fPersist = !fPersist;
				CheckMenuItem(GetSubMenu(GetMenu(hWnd), IDM_EDIT),
					IDM_PERSIST, (fPersist ? MF_CHECKED : MF_UNCHECKED));
				break;

			case IDM_RULER_DEMO:
			{
				lpProcRuler = MakeProcInstance(reinterpret_cast<BOOL(__stdcall*)(HWND, unsigned int, WPARAM, LPARAM)>(Ruler::RuleDemoProc), hInst);
				DialogBox(hInst, "RuleDemoDlg", hWnd, (DLGPROC)lpProcRuler);
				FreeProcInstance(lpProcRuler);
				break;
			}

			default:
				fHandled = FALSE;
				break;
			}
			break;

		case WT_PACKET:
		{
			hctx = (HCTX)lParam;
			PACKET pkt;

			if (gpWTPacket((HCTX)lParam, wParam, &pkt))
			{
				if (HIWORD(pkt.pkButtons) == TBN_DOWN)
				{
					MessageBeep(0);
				}

				ptOld = ptNew;

				if (g_contextMap[hctx].displayTablet)
				{
					double scaleWidth = monWidth / (double)g_contextMap[hctx].tabletXExt;
					double scaleHeight = monHeight / (double)g_contextMap[hctx].tabletYExt;

					ptNew.x = (LONG)((double)pkt.pkX * scaleWidth);
					ptNew.y = (LONG)((double)pkt.pkY * scaleHeight);

					ptNew.x += monInfo.rcMonitor.left;
					ptNew.y += monInfo.rcMonitor.top;

					ScreenToClient(hWnd, &ptNew);
				}
				else
				{
					ptNew.x = (LONG)(rcClient.right * ((double)pkt.pkX / (double)g_contextMap[hctx].tabletXExt));
					ptNew.y = (LONG)(rcClient.bottom * ((double)pkt.pkY / (double)g_contextMap[hctx].tabletYExt));
				}

				if (ptNew.x != ptOld.x || ptNew.y != ptOld.y)
				{
					InvalidateRect(hWnd, NULL, TRUE);
					if (count++ == 4)
					{
						count = 0;
						UpdateWindow(hWnd);
					}
				}
			}
			break;
		}

		case WM_ACTIVATE:
			if (GET_WM_ACTIVATE_STATE(wParam, lParam))
				InvalidateRect(hWnd, NULL, TRUE);

			// if switching in the middle, disable the region
			if (hctx) {
				if (!fPersist)
					gpWTEnable(hctx, GET_WM_ACTIVATE_STATE(wParam, lParam));
				if (hctx && GET_WM_ACTIVATE_STATE(wParam, lParam))
					gpWTOverlap(hctx, TRUE);
			}
			break;

		case WM_DESTROY:
			CloseContexts();
			PostQuitMessage(0);
			break;

		case WM_PAINT:
			count = 0;
			hDC = BeginPaint(hWnd, &psPaint);

			/* redo horz */
			PatBlt(hDC, rcClient.left, rcClient.bottom - ptNew.y,
					rcClient.right, 1, BLACKNESS);
			/* redo vert */
			PatBlt(hDC, ptNew.x, rcClient.top,
					1, rcClient.bottom, BLACKNESS);

			EndPaint(hWnd, &psPaint);
			break;

		default:
			fHandled = FALSE;
			break;
	}
	if (fHandled)
		return (lResult);
	else
		return (DefWindowProc(hWnd, message, wParam, lParam));
}


// --------------------------------------------------------------------------
BOOL AboutProc(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message) {
	case WM_INITDIALOG:
		return (TRUE);

	case WM_COMMAND:
		if (GET_WM_COMMAND_ID(wParam, lParam) == IDOK
				|| GET_WM_COMMAND_ID(wParam, lParam) == IDCANCEL) {
		EndDialog(hDlg, TRUE);
		return (TRUE);
		}
		break;
	}
	return (FALSE);
}


// --------------------------------------------------------------------------
void Cleanup( void )
{
	WACOM_TRACE( "Cleanup()\n" );

	UnloadWintab( );
}


// --------------------------------------------------------------------------
// Close all opened tablet contexts
void CloseContexts(void)
{
	// Close all contexts we opened so we don't have them lying around in prefs.
	for (CtxMap::iterator it = g_contextMap.begin(); it != g_contextMap.end(); ++it)
	{
		HCTX hCtx = it->first;
		WacomTrace("Closing context: 0x%X\n", hCtx);
		if (!gpWTClose(hCtx))
		{
			WacomTrace("Could not close context: 0x%X\n", hCtx);
		}
		else
		{
			hCtx = NULL;
		}
	}

	g_contextMap.clear();
}