/*----------------------------------------------------------------------------s
	NAME
		ScribbleDemo.cpp

	PURPOSE
		This demo shows how to use Wintab to detect/display pen pressure input
		for multiple tablets.  
		
		Can be built for either Wintab system or digitizer contexts.

	COPYRIGHT
		This file is Copyright (c) Wacom Company, Ltd. 2024 All Rights Reserved
		with portions copyright 1991-1998 by LCS/Telegraphics.

		The text and information contained in this file may be freely used,
		copied, or distributed without compensation or licensing restrictions.

	NOTES
		This app has been configured for DPI Awareness = Per Monitor High DPI Aware
		See: https://msdn.microsoft.com/en-us/library/windows/desktop/dn280512(v=vs.85).aspx
---------------------------------------------------------------------------- */

#include <windows.h>
#include <windowsx.h>
#include "msgpack.h"
#include "wintab.h"
// PACKETDATA is a macro specifying what data the driver should return in pen data packets
#define PACKETDATA	(PK_X | PK_Y | PK_BUTTONS | PK_NORMAL_PRESSURE | PK_TANGENT_PRESSURE | PK_TIME)
#define PACKETMODE	PK_BUTTONS
#include "pktdef.h"
#include "Utils.h"
#include "ScribbleDemo.h"
#include <vector>
#include <map>
#include <sstream>
#include "ShellScalingAPI.h"

#if defined(_DEBUG)
// Uncomment the following line if you want to trace tablet packet data.
//#define TRACE_PACKETDATA

// Uncomment the following line if you want to trace context overlap messages.
//#define TRACE_CTXOVERLAP

// Uncomment the following line if you want to trace raw pen data.
//#define TRACE_RAWPENDATA

// Uncomment the following line if you want to trace converted draw pen data.
//#define TRACE_DRAWPENDATA
#endif

// Set g_openSystemContext as:
//		true for building a Wintab system context (WTI_DEFSYSCTX)
//		false for building a Wintab digitizer context(WTI_DDCTXS)
bool g_openSystemContext = true;

// If g_useMouseMessages is true, then the demo will respond to mouse events
// to get pen position, and poll for Wintab data to get pen pressure.  The
// mouse cursor position will be used to indicate the pen tip position.
bool g_useMouseMessages = false;

// If g_useMouseMessages is true, demo polls for Wintab data.  This simple demo
// keeps track of Wintab context of first tablet found, which will be used
// as the context of the tablet to be polled for demo purposes.
// (Note that when relying on Wintab events, the tablet context comes along with
// the WT_PACKET message, so this kind of thing is not needed.)
HCTX g_hCtxUsedForPolling = nullptr;

// This is the max number of Wintab data packets queried for at a time
// when using the polling method of getting Wintab data.
#define MAX_PACKETS	20

// Set g_penMovesSystemCursor true if the demo should move the system cursor.
bool g_penMovesSystemCursor = true;

// Uncomment this line to draw a graphic showing mouse click point.
//#define DRAW_CLICK_POINT

HINSTANCE g_hInst = nullptr;

bool g_drawLines = true;
bool g_pressure = true;
bool g_offsetMode = false;

// If true, uses actual tablet output.
bool g_useActualDigitizerOutput = false;

// If true, assumes app maximized on display tablet.
// Also assumes taskbar is hidden (but can fix later).
bool g_kioskDisplay = false;

HWND		g_mainWnd = nullptr;
HDC		g_hdc = nullptr;
HWND		g_hWndAbout = nullptr;

HBRUSH g_hDownBrush = CreateSolidBrush(RGB(255, 0, 0));
HBRUSH g_hUpBrush = CreateSolidBrush(RGB(0, 255, 0));

double g_sysWidth = 0;
double g_sysHeight = 0;
double g_sysOrigX = 0;
double g_sysOrigY = 0;

static HCTX g_hctx = nullptr;
static HCTX g_hctxLast = nullptr;
static double g_scaleWidth = 0.0;
static double g_scaleHeight = 0.0;
static RECT g_clientRect = { 0 };
static RECT g_windowRect = { 0 };
static MONITORINFO g_monInfo = { 0 };

///////////////////////////////////////////////////////////////////////////////
// Hold tablet-specific properties used when responding to tablet data packets.
//
typedef struct 
{
	int		maxPressure;
	COLORREF	penColor;
	char		name[32];
	LONG		tabletXExt;
	LONG		tabletYExt;
	bool		displayTablet;
} TabletInfo;


///////////////////////////////////////////////////////////////////////////////
// Cache all opened contexts for attached tablets.  
// This will allow us to close them when the window closes down.
//
std::map<HCTX, TabletInfo> g_contextMap;

///////////////////////////////////////////////////////////////////////////////
// Rotate through these colors for all tablets.
//
typedef enum
{
	PenColorRed,
	PenColorGreen,
	PenColorBlue,

	MAX_PEN_COLOR
} EPenColor;

EPenColor gNextPenColor = PenColorBlue;

///////////////////////////////////////////////////////////////////////////////
// Global variables
//
char* gpszProgramName = "ScribbleDemo";

static int gnOpenContexts = 0;
static int gnAttachedDevices = 0;

///////////////////////////////////////////////////////////////////////////////

/// Asks Wintab for a data packet.  Normally would use this in response to 
/// a non-Wintab event, such as a mouse event.  If new data received, the
/// drawing area is invalidated so that the data can be drawn.
///
void PollForPenData(HCTX hCtx_I, HWND hWnd_I, POINT& ptOld_I, UINT& prsOld_I, POINT& ptNew_O, UINT& prsNew_O)
{
	PACKET pkts[MAX_PACKETS] = {0};

	// Get up to MAX_PACKETS from Wintab data packet cache per request.
	int numPackets = gpWTPacketsGet(hCtx_I, MAX_PACKETS, (LPVOID)pkts);

	for (int idx = 0; idx < numPackets; idx++)
	{
		PACKET* pkt = &pkts[idx];

		//WacomTrace("pkt: x,y,p: %i,%i,%i\n", pkt->pkX, pkt->pkY, pkt->pkNormalPressure);

		ptOld_I = ptNew_O;
		prsOld_I = prsNew_O;

		ptNew_O.x = pkt->pkX;
		ptNew_O.y = pkt->pkY;

		prsNew_O = pkt->pkNormalPressure;

		if (ptNew_O.x != ptOld_I.x ||
				ptNew_O.y != ptOld_I.y ||
				prsNew_O != prsOld_I) 
		{
			InvalidateRect(hWnd_I, nullptr, false);
		}
	}
}

///////////////////////////////////////////////////////////////////////////////

void DumpWintabContext(const LOGCONTEXT &ctx_I)
{
	WacomTrace("***********************************************\n");
	WacomTrace("Context:\n");
	WacomTrace("  lcName:      %s\n", ctx_I.lcName);
	WacomTrace("  lcOptions:   %i\n", ctx_I.lcOptions);
	WacomTrace("  lcStatus:    %i\n", ctx_I.lcStatus);
	WacomTrace("  lcLocks:     %i\n", ctx_I.lcLocks);
	WacomTrace("  lcMsgBase:   %i\n", ctx_I.lcMsgBase);
	WacomTrace("  lcDevice:    %i\n", ctx_I.lcDevice);
	WacomTrace("  lcPktRate:   %i\n", ctx_I.lcPktRate);
	WacomTrace("  lcPktData:   %i\n", ctx_I.lcPktData);
	WacomTrace("  lcPktMode:   %i\n", ctx_I.lcPktMode);
	WacomTrace("  lcMoveMask:  0x%X\n", ctx_I.lcMoveMask);
	WacomTrace("  lcBtnDnMask: 0x%X\n", ctx_I.lcBtnDnMask);
	WacomTrace("  lcBtnUpMask: 0x%X\n", ctx_I.lcBtnUpMask);
	WacomTrace("  lcInOrgX:    %i\n", ctx_I.lcInOrgX);
	WacomTrace("  lcInOrgY:    %i\n", ctx_I.lcInOrgY);
	WacomTrace("  lcInOrgZ:    %i\n", ctx_I.lcInOrgZ);
	WacomTrace("  lcInExtX:    %i\n", ctx_I.lcInExtX);
	WacomTrace("  lcInExtY:    %i\n", ctx_I.lcInExtY);
	WacomTrace("  lcInExtZ:    %i\n", ctx_I.lcInExtZ);
	WacomTrace("  lcOutOrgX:   %i\n", ctx_I.lcOutOrgX);
	WacomTrace("  lcOutOrgY:   %i\n", ctx_I.lcOutOrgY);
	WacomTrace("  lcOutOrgZ:   %i\n", ctx_I.lcOutOrgZ);
	WacomTrace("  lcOutExtX:   %i\n", ctx_I.lcOutExtX);
	WacomTrace("  lcOutExtY:   %i\n", ctx_I.lcOutExtY);
	WacomTrace("  lcOutExtZ:   %i\n", ctx_I.lcOutExtZ);
	WacomTrace("  lcSensX:     %i\n", ctx_I.lcSensX);
	WacomTrace("  lcSensY:     %i\n", ctx_I.lcSensY);
	WacomTrace("  lcSensZ:     %i\n", ctx_I.lcSensZ);
	WacomTrace("  lcSysMode:   %i\n", ctx_I.lcSysMode);
	WacomTrace("  lcSysOrgX:   %i\n", ctx_I.lcSysOrgX);
	WacomTrace("  lcSysOrgY:   %i\n", ctx_I.lcSysOrgY);
	WacomTrace("  lcSysExtX:   %i\n", ctx_I.lcSysExtX);
	WacomTrace("  lcSysExtY:   %i\n", ctx_I.lcSysExtY);
	WacomTrace("  lcSysSensX:  %i\n", ctx_I.lcSysSensX);
	WacomTrace("  lcSysSensY:  %i\n", ctx_I.lcSysSensY);
	WacomTrace("***********************************************\n");
}
 
///////////////////////////////////////////////////////////////////////////////


void SetTitleBarText(HWND hwnd_I, const std::string &text_I)
{
	std::string text = text_I + (g_openSystemContext ? " [SYSTEM CONTEXT]" : " [DIGITIZER CONTEXT]");
	SetWindowText(hwnd_I, text.c_str());
}

///////////////////////////////////////////////////////////////////////////////

int PASCAL WinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE hPrevInstance, _In_ LPSTR lpCmdLine, _In_ int nCmdShow)
{
	MSG msg;

	std::string cmdline = GetCommandLine();

	if (cmdline.find("/ctx system") != -1)
	{
		g_openSystemContext = true;		// build a Wintab system context
	}
	else if (cmdline.find("/ctx digitizer") != -1)
	{
		g_openSystemContext = false;		// build a Wintab digitizer context
	}

	// When set, uses a digitizer context fix.
	if (cmdline.find("/useActualDigitizerOutput") != -1)
	{
		g_useActualDigitizerOutput = true;
		g_openSystemContext = false;		// must be using digitizer context
	}

	// When set, assumes app is full display size.
	// Useful for display tablet input only.
	if (cmdline.find("/kioskDisplay") != -1)
	{
		g_kioskDisplay = true;
		g_useActualDigitizerOutput = true;	// actual digitizer data needed
		g_openSystemContext = false;			// must be using digitizer context
	}

	if (!hPrevInstance)
	{
		if (!InitApplication(hInstance))
		{
			return false;
		}
	}

	/* Perform initializations that apply to a specific instance */

	if (!InitInstance(hInstance, nCmdShow))
	{
		return false;
	}

	/* Acquire and dispatch messages until a WM_QUIT message is received. */

	while (GetMessage(&msg, nullptr, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	// Return Wintab resources.
	Cleanup();

	return static_cast<int>(msg.wParam);
}

///////////////////////////////////////////////////////////////////////////////

// Normal Win32 initialization defining/registering window class
bool InitApplication(HINSTANCE hInstance)
{
	WNDCLASS  wc;

	/* Fill in window class structure with parameters that describe the       */
	/* main window.                                                           */

	wc.style = 0;
	wc.lpfnWndProc = MainWndProc;

	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = hInstance;
	wc.hIcon = LoadIcon(nullptr, IDI_APPLICATION);
	wc.hCursor = LoadCursor(nullptr, IDC_ARROW);
	wc.hbrBackground = (HBRUSH)(COLOR_APPWORKSPACE + 1);
	wc.lpszMenuName =  "ScribbleDemoMenu";
	wc.lpszClassName = "ScribbleDemoWClass";

	/* Register the window class and return success/failure code. */

	return (RegisterClass(&wc));
}

///////////////////////////////////////////////////////////////////////////////

// Initializes the app:
//		Loads Wintab32
//		Checks for running tablet driver
//		Shows the window, which, during creation, will create Wintab contexts
bool InitInstance(HINSTANCE hInstance, int nCmdShow)
{
	char	buf[50];

	/* Save the instance handle in static variable, which will be used in  */
	/* many subsequence calls from this application to Windows.            */

	g_hInst = hInstance;

	if ( !LoadWintab( ) )
	{
		ShowError( "Wintab not available" );
		return false;
	}

	/* check if WinTab available. */
	if (!gpWTInfoA(0, 0, nullptr))
	{
		ShowError( "WinTab Services Not Available." );
		return false;
	}

	// Make sure that we are using system context if using mouse messages
	// to indicate the pen position.
	if (g_useMouseMessages)
	{
		g_openSystemContext = true;
	}

	/* Create a main window for this application instance.  */

	wsprintf(buf, "%s", gpszProgramName);
	g_mainWnd = CreateWindow(
		"ScribbleDemoWClass",
		buf,
		WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		nullptr,
		nullptr,
		hInstance,
		nullptr
	);

	/* If window could not be created, return "failure" */

	if (!g_mainWnd)
	{
		ShowError( "Could not create window!");
		return false;
	}

	g_hdc = GetDC(g_mainWnd);

	/* Make the window visible; update its client area; and return "success" */

	ShowWindow(g_mainWnd, nCmdShow);
	UpdateWindow(g_mainWnd);
	return true;
}

///////////////////////////////////////////////////////////////////////////////
// Open contexts for all attached tablets.
// Returns true if any tablet(s) configured.
//
bool static NEAR OpenTabletContexts(HWND hWnd)
{
	int ctxIndex = 0;
	gnOpenContexts = 0;
	gnAttachedDevices = 0;
	std::stringstream szTabletName;

	g_contextMap.clear();

	gpWTInfoA(WTI_INTERFACE, IFC_NDEVICES, &gnAttachedDevices);
	WacomTrace("Number of attached devices: %i\n", gnAttachedDevices);

	// Open/save contexts until first failure to open a context.
	// Note that gpWTInfoA(WTI_STATUS, STA_CONTEXTS, &nOpenContexts);
	// will not always let you enumerate through all contexts.
	do
	{
		int foundCtx = 0;
		LOGCONTEXT lcMine = {0};
		UINT wWTInfoRetVal = 0;
		AXIS tabletX = {0};
		AXIS tabletY = {0};
		AXIS Pressure = {0};

		WacomTrace("Getting info on contextIndex: %i ...\n", ctxIndex);

		if ( g_openSystemContext )
		{
			// Opens a system context; XY returned as pixels for all 
			// attached tablets.
			WacomTrace("Opening WTI_DEFSYSCTX (system context)...\n");
			foundCtx = gpWTInfoA(WTI_DEFSYSCTX, 0, &lcMine);
		}
		else
		{
			// Opens a digitizer context; XY returned as tablet coordinates for 
			// each attached tablet.
			WacomTrace("Opening WTI_DDCTXS (digitizer context)...\n");
			foundCtx = gpWTInfoA(WTI_DDCTXS + ctxIndex, 0, &lcMine);

			// Use this flavor of digitizing context if not enumerating tablets.
			// Opens a "virtual" context used for all tablets.
			//foundCtx = gpWTInfoA(WTI_DEFCONTEXT, 0, &lcMine);
		}

		if ( foundCtx > 0 )
		{
			UINT result = 0;
			wWTInfoRetVal = gpWTInfoA(WTI_DEVICES + ctxIndex, DVC_HARDWARE, &result);
			bool displayTablet = result & HWC_INTEGRATED;

			gpWTInfoA(WTI_DEVICES + ctxIndex, DVC_PKTRATE, &result);
			WacomTrace("pktrate: %i\n", result);

			char name[1024];
			gpWTInfoA(WTI_DEVICES + -1, DVC_NAME, name);
			WacomTrace("name: %s\n", name);

			WacomTrace("Current context tablet type is: %s\n", displayTablet ? "display (integrated)" : "opaque");

			lcMine.lcPktData = PACKETDATA;
			lcMine.lcOptions |= CXO_MESSAGES;

			if (g_penMovesSystemCursor)
			{
				lcMine.lcOptions |= CXO_SYSTEM;	// move system cursor
			}
			else
			{
				lcMine.lcOptions &= ~CXO_SYSTEM;	// don't move system cursor
			}

			lcMine.lcPktMode = PACKETMODE;
			lcMine.lcMoveMask = PACKETDATA;
			lcMine.lcBtnUpMask = lcMine.lcBtnDnMask;

			// Set the entire tablet as active
			wWTInfoRetVal = gpWTInfoA( WTI_DEVICES + ctxIndex, DVC_X, &tabletX );
			if (  wWTInfoRetVal != sizeof( AXIS ) )
			{
				WacomTrace("This context should not be opened.\n");
				continue;
			}

			wWTInfoRetVal = gpWTInfoA( WTI_DEVICES + ctxIndex, DVC_Y, &tabletY );

			gpWTInfoA( WTI_DEVICES + ctxIndex, DVC_NPRESSURE, &Pressure );
			WacomTrace("Pressure: %i, %i\n", Pressure.axMin, Pressure.axMax);

			if ( g_openSystemContext )
			{
				// leave lcIn* and lcOut* as-is except for reversing lcOutExtY.
			}
			else // digitizer context
			{
				// This is essential code that picks up orientation changes.
				// The reason for the calculations is to convert the tablet
				// Max/Min values to extents (counts).
				lcMine.lcOutExtX = tabletX.axMax - tabletX.axMin + 1;
				lcMine.lcOutExtY = tabletY.axMax - tabletY.axMin + 1;

				if (g_useActualDigitizerOutput)
				{
					// This is bumped to communicate to the driver that we
					// want to use the fixed behavior to get actual tablet output.
					lcMine.lcOutExtX++;
				}
			}

			// In Wintab, the tablet origin is lower left.  Move origin to upper left
			// so that it coincides with screen origin.
			lcMine.lcOutExtY = -lcMine.lcOutExtY;
			
			// Leave the system origin and extents as received:
			// lcSysOrgX, lcSysOrgY, lcSysExtX, lcSysExtY

			DumpWintabContext(lcMine);

			// Open the context enabled.
			HCTX hCtx = gpWTOpenA(hWnd, &lcMine, true);

			// Save the first context, to be used to poll first tablet found when
			// mouse messages are received.
			if ( g_useMouseMessages && !g_hCtxUsedForPolling && hCtx)
			{
				g_hCtxUsedForPolling = hCtx;
			}

			if ( hCtx )
			{
				// Save context
				COLORREF penColor = MAX_PEN_COLOR;

				switch (gNextPenColor)
				{
					case PenColorRed:		penColor = RGB(255,0,0);	gNextPenColor = PenColorGreen; break;
					case PenColorGreen:	penColor = RGB(0,255,0);	gNextPenColor = PenColorBlue; break;
					case PenColorBlue:	penColor = RGB(0,0,255);	gNextPenColor = PenColorRed; break;
					default:
						WACOM_ASSERT( !"Bad ben color" );
				}

				TabletInfo info = { Pressure.axMax, penColor };
				sprintf(info.name, "Tablet: %i\n", ctxIndex);
				info.tabletXExt = tabletX.axMax;
				info.tabletYExt = tabletY.axMax;
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
			WacomTrace("No context info for ctxIndex: %i, bailing out...\n", ctxIndex);
			break;
		}

		if ( g_openSystemContext )
		{
			break;	// we're done; only the one context; bail out...
		}

		ctxIndex++;
	} while (true);

	if ( gnOpenContexts < gnAttachedDevices && !g_openSystemContext)
	{
		ShowError("Oops - did not open a context for each attached device");
	}

	{
		std::stringstream szTitle; szTitle.flush();
		szTitle << gpszProgramName << ": #tablet(s) attached: " << gnAttachedDevices;
		SetTitleBarText(hWnd, szTitle.str().c_str());
	}

	return gnAttachedDevices > 0;
}

///////////////////////////////////////////////////////////////////////////////
// Close all opened tablet contexts.
//
void CloseTabletContexts(void)
{
	// Close all contexts we opened so we don't have them lying around in prefs.
	for (std::map<HCTX, TabletInfo>::iterator it = g_contextMap.begin();
			it != g_contextMap.end(); ++it)
	{
		HCTX hCtx = it->first;
		WacomTrace("Closing context: 0x%X\n", hCtx);

		if (hCtx != nullptr)
		{
			gpWTClose(hCtx);
		}
	}

	g_contextMap.clear();

	gnOpenContexts = 0;
	gnAttachedDevices = 0;

	g_hctx = nullptr;
	g_hctxLast = nullptr;
	g_hCtxUsedForPolling = nullptr;
	g_scaleWidth = 0.0;
	g_scaleHeight = 0.0;
}

///////////////////////////////////////////////////////////////////////////////

void UpdateWindowExtents(HWND hWnd)
{
	// Compute scaling factor from tablet to display.
	if (g_contextMap.count(g_hctx) != 0)
	{
		TabletInfo info = g_contextMap[g_hctx];
		if (g_contextMap[g_hctx].displayTablet)
		{
			if (g_kioskDisplay)
			{
				// Scale tablet to app window rect
				::GetWindowRect(hWnd, &g_windowRect);
				double winWidth = static_cast<double>(g_windowRect.right) - static_cast<double>(g_windowRect.left);
				double winHeight = static_cast<double>(g_windowRect.bottom) - static_cast<double>(g_windowRect.top);
				g_scaleWidth = winWidth / (double)info.tabletXExt;
				g_scaleHeight = winHeight / (double)info.tabletYExt;
			}
			else
			{
				// Scale tablet to monitor
				HMONITOR hMon = ::MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
				g_monInfo = { 0 };
				g_monInfo.cbSize = sizeof(MONITORINFO);
				::GetMonitorInfo(hMon, &g_monInfo);
				double monWidth = static_cast<double>(g_monInfo.rcMonitor.right) - static_cast<double>(g_monInfo.rcMonitor.left);
				double monHeight = static_cast<double>(g_monInfo.rcMonitor.bottom) - static_cast<double>(g_monInfo.rcMonitor.top);
				g_scaleWidth = monWidth / (double)info.tabletXExt;
				g_scaleHeight = monHeight / (double)info.tabletYExt;

				//WacomTrace("UpdateWindowExtents: mon: %f,%f, tabletExt: %f,%f\n", monWidth, monHeight, (double)info.tabletXExt, (double)info.tabletYExt);
			}
		}
		else
		{
			// Scales tablet to entire desktop.
			g_scaleWidth = (double)g_sysWidth / (double)info.tabletXExt;
			g_scaleHeight = (double)g_sysHeight / (double)info.tabletYExt;
		}

		InvalidateRect(hWnd, nullptr, true);
	}
}

///////////////////////////////////////////////////////////////////////////////

void UpdateSystemExtents()
{
	g_sysWidth = GetSystemMetrics(SM_CXVIRTUALSCREEN);
	g_sysHeight = GetSystemMetrics(SM_CYVIRTUALSCREEN);
	g_sysOrigX = GetSystemMetrics(SM_XVIRTUALSCREEN);
	g_sysOrigY = GetSystemMetrics(SM_YVIRTUALSCREEN);
}

///////////////////////////////////////////////////////////////////////////////

bool HasAttachedDisplayTablet()
{
	for (std::map<HCTX, TabletInfo>::iterator it = g_contextMap.begin(); it != g_contextMap.end(); ++it)
	{
		if (it->second.displayTablet)
		{
			return true;
		}
	}

	return false;
}

///////////////////////////////////////////////////////////////////////////////

// Windows message handlers, which include handlers for specific Wintab messages.
LRESULT FAR PASCAL MainWndProc(HWND hWnd, unsigned message ,WPARAM wParam, LPARAM lParam)
{
	static POINT ptOld = {0};
	static POINT ptNew = {0};
	static POINT ptMouseDown, ptMouseUp = {-1};
	static bool  bMouseDown, bMouseUp = false;
	static UINT prsOld = 0;
	static UINT prsNew = 0;
	static RECT g_clientRect = { 0 };
	static MONITORINFO g_monInfo = { 0 };

	PAINTSTRUCT psPaint = {0};
	HDC hDC = nullptr;
	bool fHandled = true;
	LRESULT lResult = 0L;

	//WacomTrace("msg: 0x%X, wparam: 0x%X, lparam: 0x%X\n",message, wParam, lParam);

	switch (message) 
	{
		case WM_CREATE:
		{
			UpdateSystemExtents();

			// Initialize a Wintab context for each connected tablet.
			if (!OpenTabletContexts(hWnd))
			{
				ShowError("No tablets found.");
				//SendMessage(hWnd, WM_DESTROY, 0, 0L);
			}

			break;
		}

		case WM_MOVE:
		case WM_SIZE:
		{
			UpdateWindowExtents(hWnd);
			break;
		}

		case WM_KEYDOWN:
		if (wParam == VK_ESCAPE)
		{
			InvalidateRect(hWnd, nullptr, true);
			break;
		}

		case WM_COMMAND:
		{
			switch (GET_WM_COMMAND_ID(wParam, lParam))
			{
				case IDM_ABOUT:
				{
					if (!IsWindow(g_hWndAbout))
					{
						CreateDialog(g_hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
						ShowWindow(g_hWndAbout, SW_SHOW);
					}
					break;
				}

				case IDM_LINES:
				{
					HMENU menu = GetMenu(hWnd);
					UINT wmId = LOWORD(wParam);
					UINT state = GetMenuState(menu, wmId, MF_BYCOMMAND);
					if (state != (UINT)-1)
					{
						// reverse of the current state
						bool bOn = (state & MF_CHECKED) == 0;
						g_drawLines = bOn;
						CheckMenuItem(menu, wmId, bOn ? MF_CHECKED : MF_UNCHECKED);
					}
					break;
				}

				case IDM_PRESSURE:
				{
					HMENU menu = GetMenu(hWnd);
					UINT wmId = LOWORD(wParam);
					UINT state = GetMenuState(menu, wmId, MF_BYCOMMAND);
					if (state != (UINT)-1)
					{
						// reverse of the current state
						bool bOn = (state & MF_CHECKED) == 0;
						g_pressure = bOn;
						CheckMenuItem(menu, wmId, bOn ? MF_CHECKED : MF_UNCHECKED);
					}
					break;
				}

				case IDM_OFFSETMODE:
				{
					HMENU menu = GetMenu(hWnd);
					UINT wmId = LOWORD(wParam);
					UINT state = GetMenuState(menu, wmId, MF_BYCOMMAND);
					if (state != (UINT)-1)
					{
						// reverse of the current state
						bool bOn = (state & MF_CHECKED) == 0;
						g_offsetMode = bOn;
						CheckMenuItem(menu, wmId, bOn ? MF_CHECKED : MF_UNCHECKED);
					}
					break;
				}

				case IDM_CLEAR:
				{
					InvalidateRect(hWnd, nullptr, true);
					break;
				}

				default:
				{
					fHandled = false;
					break;
				}
			}

			break;
		}

		case WM_LBUTTONDOWN:
		{
			//WacomTrace("WM_LBUTTONDOWN\n");
			bMouseDown = true;

			if (g_useMouseMessages)
			{
				PollForPenData(g_hCtxUsedForPolling, hWnd, ptOld, prsOld, ptNew, prsNew);
			}
			else
			{
				InvalidateRect(hWnd, nullptr, false);
			}

			break;
		}

		case WM_LBUTTONUP:
		{
			//WacomTrace("WM_LBUTTONUP\n");
			bMouseUp = true;

			if (g_useMouseMessages)
			{
				PollForPenData(g_hCtxUsedForPolling, hWnd, ptOld, prsOld, ptNew, prsNew);
			}
			else
			{
				InvalidateRect(hWnd, nullptr, false);
			}

			break;
		}

		case WM_MOUSEMOVE:
		//WacomTrace("WM_MOUSEMOVE\n");
		if (g_useMouseMessages)
		{
			PollForPenData(g_hCtxUsedForPolling, hWnd, ptOld, prsOld, ptNew, prsNew);
			break;
		}

		// Wintab message indicating new pen data available.
		case WT_PACKET:
		{
			if (g_contextMap.count((HCTX)lParam) == 0)
			{
				//WacomTrace("WT_PACKET: (HCTX)lParam: 0x%X not found in map\n", (HCTX)lParam);
				break;
			}

			g_hctx = (HCTX)lParam;
			PACKET pkt = {0};

			// Query for the new pen data.
			// Wintab X/Y data is in screen or tablet coordinates, depending on how
			// the Wintab context was opened. These coordinates will have to
			// be converted to client coordinates in the WM_PAINT handler.
			if (gpWTPacket(g_hctx, static_cast<int>(wParam), &pkt))
			{
				if (g_useMouseMessages)
				{
					// Use cursor position for pen position.
					// Note - we must query for pen data so that we can use pressure below.
					POINT curPoint;
					::GetCursorPos(&curPoint);

					pkt.pkX = curPoint.x;
					pkt.pkY = curPoint.y;
				}
				else
				{
					// Use pen data for pen position
					pkt.pkX = (long)((double)pkt.pkX);
					pkt.pkY = (long)((double)pkt.pkY);
				}

#if defined(TRACE_PACKETDATA)
				WacomTrace("WT_PACKET: g_hctx[0x%X], pkt: x,y,p,tp: %i,%i,%i,%i - timestamp: %i\n", 
					g_hctx, pkt.pkX, pkt.pkY, pkt.pkNormalPressure, pkt.pkTangentPressure, pkt.pkTime);
#endif

				ptNew.x = pkt.pkX;
				ptNew.y = pkt.pkY;
				prsNew = pkt.pkNormalPressure;

				// WM_PAINT will use ptNew and prsNew to draw lines.
				InvalidateRect(hWnd, nullptr, false);
			}

			break;
		}

		// Wintab message indicating tablet attach or detach.
		case WT_INFOCHANGE:
		{
			int nAttachedDevices = 0;
			gpWTInfoA(WTI_INTERFACE, IFC_NDEVICES, &nAttachedDevices);

			WacomTrace("WT_INFOCHANGE detected; number of connected tablets is: %i\n", nAttachedDevices);

			// close all current tablet contexts
			CloseTabletContexts();

			if (nAttachedDevices > 0)
			{
				// re-enumerate attached tablets
				OpenTabletContexts(hWnd);
			}

			break;
		}

		case WM_DISPLAYCHANGE:
		{
			UpdateSystemExtents();
			CloseTabletContexts();

			// re-enumerate attached tablets
			// Possibly redundant with WT_INFOCHANGE re-enumerate.
			OpenTabletContexts(hWnd);

			break;
		}

		// WIntab message indicating pen came into or went out of proximity to tablet surface.
		case WT_PROXIMITY:
		{
			if (g_contextMap.count((HCTX)lParam) == 0)
			{
				//WacomTrace("WT_PACKET: (HCTX)lParam: 0x%X not found in map\n", (HCTX)lParam);
				break;
			}

			g_hctx = (HCTX)lParam;

			bool entering = (HIWORD(lParam) != 0);
			TabletInfo info = {0};
			std::stringstream szTitle;	szTitle.flush();

			if ( g_contextMap.count(g_hctx) > 0 )
			{
				info = g_contextMap[g_hctx];

				if ( g_openSystemContext )
				{
					szTitle << (entering ? "ENTER: " : "LEAVE: ") << gpszProgramName << "; #tablet(s) attached: " << gnAttachedDevices << "; drawing on: virtual system context";
				}
				else
				{
					szTitle << (entering ? "ENTER: " : "LEAVE: ") << gpszProgramName << "; #tablet(s) attached: " << gnAttachedDevices << "; drawing on: " << info.name;
				}
				WacomTrace("Tablet name: %s\n", szTitle.str().c_str());
			}
			else
			{
				WacomTrace("Oops - couldn't find context: 0x%X\n", g_hctx);
				szTitle << "ERROR: couldn't find tablet context: " << g_hctx;
			}

			SetTitleBarText(hWnd, szTitle.str().c_str());

			break;
		}

		// Wintab message indicating app Wintab context ordering changed.
		// Define DO_CTXOVERLAP handling for WT_CTXOVERLAP if you want the app to 
		// receive data when it is not in focus.  This is normally a BAD IDEA, as it
		// may produce unintended side effects with other apps.
#if defined(DO_CTXOVERLAP)
		case WT_CTXOVERLAP:
		{
			HCTX myCtx = (HCTX) wParam;
		
#if defined(TRACE_CTXOVERLAP)
			// lParam is a status value.
			// See Wintab v1.4, table 7.12 Context Status Values
			WacomTrace("TRACE_CTXOVERLAP received for context: 0x%X, lParam: 0x%X\n", 
				myCtx, lParam);
#endif
			bool activate = GET_WM_ACTIVATE_STATE(wParam, lParam);
			WacomTrace("wParam: 0x%X, lParam: 0x%X, activate: %i\n", wParam, lParam, activate);
			gpWTEnable(myCtx, activate);
			if (myCtx)
			{
				//Force context to always be on top so that it will
				//get the data, even if out of focus.
				gpWTOverlap(g_hctx, true);
			}

			break;
		}
#endif // DO_CTXOVERLAP

		case WM_DESTROY:
		{
			CloseTabletContexts();
			PostQuitMessage(0);
			break;
		}

		// Windows Paint message used to draw captured pen data.
		case WM_PAINT:
		{
			// This code draws line from Wintab packet data.
			if (hDC = BeginPaint(hWnd, &psPaint))
			{
				if (prsNew > 0)
				{
					HGDIOBJ original = SelectObject(hDC, GetStockObject(DC_PEN));

					int penWidth = (int) (1 + std::floor(10 * (double) prsNew / (double) g_contextMap[g_hctx].maxPressure));
					COLORREF penColor = g_contextMap[g_hctx].penColor;

					if (!g_pressure)
					{
						penWidth = 4;
					}

					HPEN hPen = CreatePen(PS_SOLID, penWidth, penColor);
					SelectObject(hDC, hPen);

					POINT oldPoint = { ptOld.x, ptOld.y };
					POINT newPoint = { ptNew.x, ptNew.y };

#if defined(TRACE_RAWPENDATA)
					WacomTrace("RAWPENDATA: old: [%i,%i], new: [%i,%i], prsNew: %i, penWidth: %i\n", oldPoint.x, oldPoint.y, newPoint.x, newPoint.y, prsNew, penWidth);
#endif

					//WacomTrace("old: [%i,%i], new: [%i,%i], penWidth: %i %s\n", 
					//	oldPoint.x, oldPoint.y, newPoint.x, newPoint.y, penWidth,
					//	oldPoint.x == newPoint.x && oldPoint.y == newPoint.y ? "[DATA HOLE]" : "");

					if (g_openSystemContext)
					{
						// Convert system pixel coordinates to client rectangle (pixels).
						// Wintab has done all the heavy lifting to produce screen coordinates - just convert to client window coordinates.
						ScreenToClient(hWnd, &oldPoint);
						ScreenToClient(hWnd, &newPoint);
					}
					else
					{
						// Interpolate tablet coordinates to client rectangle (pixels).
						// Note that this will be affected by tablet to display mapping.

						// If scale factors not computed yet, force that to happen in WM_SIZE handler.
						if (g_scaleWidth == 0.0)
						{
							SendMessage(hWnd, WM_SIZE, 0, 0);
							break;
						}

						// Convert to pixels
						if (g_useActualDigitizerOutput && g_contextMap[g_hctx].displayTablet)
						{
							if (g_kioskDisplay)
							{
								// Map tablet point to app window rect
								oldPoint.x = (LONG) ((double) oldPoint.x * g_scaleWidth); oldPoint.y = (LONG) ((double) oldPoint.y * g_scaleHeight);
								newPoint.x = (LONG) ((double) newPoint.x * g_scaleWidth); newPoint.y = (LONG) ((double) newPoint.y * g_scaleHeight);
								oldPoint.x += g_windowRect.left; oldPoint.y += g_windowRect.top;
								newPoint.x += g_windowRect.left; newPoint.y += g_windowRect.top;
							}
							else
							{
								// Map tablet point to monitor
								oldPoint.x = (LONG) ((double) oldPoint.x * g_scaleWidth); oldPoint.y = (LONG) ((double) oldPoint.y * g_scaleHeight);
								newPoint.x = (LONG) ((double) newPoint.x * g_scaleWidth); newPoint.y = (LONG) ((double) newPoint.y * g_scaleHeight);
								oldPoint.x += g_monInfo.rcMonitor.left; oldPoint.y += g_monInfo.rcMonitor.top;
								newPoint.x += g_monInfo.rcMonitor.left; newPoint.y += g_monInfo.rcMonitor.top;
							}
						}
						else
						{
							// Map tablet point to screen space
							TabletInfo info = g_contextMap[g_hctx];

							oldPoint.x = (LONG)g_sysOrigX + (LONG)(g_sysWidth * ((double) oldPoint.x / (double) info.tabletXExt));
							oldPoint.y = (LONG)g_sysOrigY + (LONG) (g_sysHeight * ((double) oldPoint.y / (double) info.tabletYExt));

							newPoint.x = (LONG)g_sysOrigX + (LONG) (g_sysWidth * ((double) newPoint.x / (double) info.tabletXExt));
							newPoint.y = (LONG)g_sysOrigY + (LONG) (g_sysHeight * ((double) newPoint.y / (double) info.tabletYExt));
						}

						// map to client window coordinates
						ScreenToClient(hWnd, &oldPoint);
						ScreenToClient(hWnd, &newPoint);
					}

#if defined(TRACE_DRAWPENDATA)
					WacomTrace("WM_PAINT: old: [%i,%i], new: [%i,%i], prsOld: %i, prsNew: %i, penWidth: %i %s\n",
						oldPoint.x, oldPoint.y, newPoint.x, newPoint.y, penWidth, prsOld, prsNew,
						oldPoint.x == newPoint.x && oldPoint.y == newPoint.y ? "[DATA HOLE]" : "");
#endif

					if (g_drawLines)
					{
						if (g_offsetMode)
						{
							MoveToEx(hDC, newPoint.x, newPoint.y + 50, nullptr);
							LineTo(hDC, newPoint.x, newPoint.y + 50);
						}

						MoveToEx(hDC, oldPoint.x, oldPoint.y, nullptr);
					}
					else
					{
						if (g_offsetMode)
						{
							MoveToEx(hDC, oldPoint.x, oldPoint.y + 50, nullptr);
							LineTo(hDC, newPoint.x, newPoint.y + 50);
						}

						MoveToEx(hDC, newPoint.x, newPoint.y, nullptr);
					}

					LineTo(hDC, newPoint.x, newPoint.y);
					DeleteObject(hPen);

#if defined(DRAW_CLICK_POINT)
					{
						// Draw the clickpoint.
						int offs = 10;
						if (bMouseDown)
						{
							HBRUSH bDownBrush = CreateSolidBrush(RGB(255, 0, 0));
							RECT rect{ newPoint.x - offs, newPoint.y - 2, newPoint.x + offs, newPoint.y + 2 };
							::FillRect(hDC, &rect, g_hDownBrush);
							bMouseDown = false;
						}
						if (bMouseUp)
						{
							HBRUSH bUpBrush = CreateSolidBrush(RGB(0, 255, 0));
							RECT rect{ newPoint.x - 2, newPoint.y - offs, newPoint.x + 2, newPoint.y + offs };
							::FillRect(hDC, &rect, g_hUpBrush);
							bMouseUp = false;
						}
					}
#endif

					SelectObject(hDC, original);
					EndPaint(hWnd, &psPaint);
				}

				// Keep track of last time we did move or draw.
				ptOld = ptNew;
				prsOld = prsNew;
			}

			break;
		}

		default:
		{
			fHandled = false;
			break;
		}
	}

	if (fHandled)
	{
		return (lResult);
	}
	else
	{
		return (DefWindowProc(hWnd, message, wParam, lParam));
	}
}

///////////////////////////////////////////////////////////////////////////////

INT_PTR CALLBACK About(HWND hDlg, unsigned message, WPARAM wParam, LPARAM lParam)
{
	UNREFERENCED_PARAMETER(lParam);

	switch (message) 
	{
		case WM_INITDIALOG:
		{
			g_hWndAbout = hDlg;
			return (INT_PTR)TRUE;
		}

		case WM_COMMAND:
		{
			if (GET_WM_COMMAND_ID(wParam, lParam) == IDOK
				|| GET_WM_COMMAND_ID(wParam, lParam) == IDCANCEL) 
			{
				DestroyWindow(hDlg);
				g_hWndAbout = nullptr;
				return (INT_PTR)TRUE;
			}
			break;
		}

		default:
			break;
	}
	return (INT_PTR)FALSE;
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Release resources we used in this example.
//
void Cleanup( void )
{
	WACOM_TRACE( "Cleanup()\n" );
	UnloadWintab( );
}

///////////////////////////////////////////////////////////////////////////////
