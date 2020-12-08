///////////////////////////////////////////////////////////////////////////////
//
//	PURPOSE
//		Sample code showing how to use the Wacom Feel(TM) Multi-Touch API and
//		the Wintab32 API.
//
//	COPYRIGHT
//		Copyright (c) 2012-2020 Wacom Co., Ltd.
//
//		The text and information contained in this file may be freely used,
//		copied, or distributed without compensation or licensing restrictions.
//
///////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "WacomMT_Scribble.h"

#include <iostream>
#include <vector>
#include <map>
#include <utility>
#include <algorithm>
#include <string>
#include <sstream>
#include <memory>
#include <crtdbg.h>

#include "WacomMultiTouch.h"
#include "WintabUtils.h"

///////////////////////////////////////////////////////////////////////////////
// Defines

// Colors for touch points
#define NO_CONFIDENCE_COLOR	RGB(255,128,0)		// orange
#define CONFIDENCE_COLOR		RGB(0, 0, 255)		// blue
#define POSITION_ONLY_COLOR	RGB(0, 255, 0)		// green

// Graphics HPEN objects
#define NUM_HPENS		10

///////////////////////////////////////////////////////////////////////////////
// Wintab support headers
#define PACKETDATA	(PK_X | PK_Y | PK_BUTTONS | PK_NORMAL_PRESSURE)
#define PACKETMODE	PK_BUTTONS
#include "pktdef.h"

// Small factor to render display tablet finger circles.
// to pixels assuming .27 pixel size. Sould use system api to get this value.
#define	DISPLAY_TAB_DRAW_SIZE_FACTOR		0.27f

///////////////////////////////////////////////////////////////////////////////
// Types

using WacomMTHitRectPtr = std::unique_ptr<WacomMTHitRect>;

enum class EDataType
{
	ENoData,
	EFingerData,
	EBlobData,
	ERawData
};

///////////////////////////////////////////////////////////////////////////////
// Global Variables

HINSTANCE								hInst = NULL;
std::wstring							szTitle = L"WacomMT_Scribble Pen, Consumer, Finger, HWND";
std::wstring							szWindowClass = L"WACOMMT_SCRIBBLE";
HWND										g_mainWnd = NULL;
HDC										g_hdc = NULL;
HWND										g_hWndAbout = NULL;
int										g_maxPressure = 1024;

// Cached client rect (system coordinates).
// Used for evaluating whether or not to render pen data by verifying whether
// the returned pen data (sys coords) falls within the client rect. Returned 
// touch contact locations use this rect to interpolate where they should be drawn.
// Similar interpolation done for raw and blob data rendering as well.
// This rect needs to be updated when the app is moved or resized.

RECT										g_clientRect = {0, 0, 0, 0};

bool										g_ShowTouchSize = true;
bool										g_ShowTouchID = false;

std::map<int, WacomMTCapability>	g_caps;
std::vector<int>						g_devices;
HCTX										g_tabCtx = NULL;

std::map<int, HPEN>					g_hPenMap;
std::map<int, HPEN>					g_fingerHPenMap;

HBRUSH									g_noConfidenceBrush = NULL;
HBRUSH									g_confidenceBrush = NULL;
HBRUSH									g_positionOnlyBrush = NULL;
HPEN										g_noConfidencePen = NULL;
HPEN										g_confidencePen = NULL;
std::map<int, WacomMTHitRectPtr>	g_lastWTHitRect;

bool										g_useConfidenceBits = true;
bool										g_ObserverMode = false;

EDataType								g_DataType = EDataType::EFingerData;
bool										g_UseHWND = true;
bool										g_UseWinHitRect = true;

CRITICAL_SECTION						g_graphicsCriticalSection;

///////////////////////////////////////////////////////////////////////////////
// Forward declarations of functions included in this code module

ATOM					MyRegisterClass(HINSTANCE hInstance);
BOOL					InitInstance(HINSTANCE, int);
LRESULT CALLBACK	WndProc(HWND, UINT, WPARAM, LPARAM);
INT_PTR CALLBACK	About(HWND, UINT, WPARAM, LPARAM);
void					ClearScreen();

///////////////////////////////////////////////////////////////////////////////
// Multi-touch API support functions

WacomMTHitRectPtr GetAppHitRect();
int FingerCallback(WacomMTFingerCollection *fingerData, void *userData);
int BlobCallback(WacomMTBlobAggregate *blobData, void *userData);
int RawCallback(WacomMTRawData *rawData, void *userData);
void AttachCallback(WacomMTCapability deviceInfo, void *userRef);
void DetachCallback(int deviceID, void *userRef);
void DrawFingerData(int count, WacomMTFinger *fingers, int device);
void DrawBlobData(int count, WacomMTBlob *blobs, int device);
void DrawRawData(int count, unsigned short* rawBuf, int device);
void DumpCaps(bool showMessageBox_I);
bool ClientHitRectChanged(const WacomMTHitRectPtr& wtHitRect_I, int deviceID);
WacomMTError RegisterForData(int deviceID_I, HWND hWnd_I);
WacomMTError UnregisterForData(int deviceID, HWND hWnd_I);
WacomMTError MoveCallback(int deviceID);
WacomMTError InitWacomMTAPI(HWND hwnd_I);

///////////////////////////////////////////////////////////////////////////////
// Wintab support functions.

HCTX InitWintabAPI(HWND hwnd_I);
void DrawPenData(POINT point_I, UINT pressure_I, bool bMoveToPoint_I);
void Cleanup(void);

///////////////////////////////////////////////////////////////////////////////

BOOL Square(HDC hDC, int x, int y, int width)
{
	int offset = width / 2;
	return Rectangle(hDC, x - offset, y - offset, x + offset, y + offset);
}

///////////////////////////////////////////////////////////////////////////////

BOOL Circle(HDC hDC, int x, int y, int r)
{
	return Ellipse(hDC, x - r, y - r, x + r, y + r);
}

///////////////////////////////////////////////////////////////////////////////

BOOL CenterEllipse(HDC hDC, int x, int y, int w, int h)
{
	return Ellipse(hDC, x - w, y - h, x + w, y + h);
}

///////////////////////////////////////////////////////////////////////////////

WacomMTProcessingMode CurrentMode(void)
{
	return g_ObserverMode
		? WMTProcessingModeObserver
		: WMTProcessingModeNone;
}

///////////////////////////////////////////////////////////////////////////////

std::wstring GetTitle(void)
{
	std::wstring title = L"WacomMT_Scribble Pen";
	title.append(L", ");
	title.append(g_ObserverMode ? L"Observer" : L"Consumer");
	title.append(L", ");

	switch (g_DataType)
	{
		case EDataType::ENoData:
		{
			title.append(L"No Touch");
			break;
		}
		case EDataType::EFingerData:
		{
			title.append(L"Finger");
			break;
		}
		case EDataType::EBlobData:
		{
			title.append(L"Blob");
			break;
		}
		case EDataType::ERawData:
		{
			title.append(L"Raw");
			break;
		}
		default:
		{
			title.append(L"Unknown");
			break;
		}
	}

	title.append(L", ");
	title.append(g_UseHWND ? L"HWND" : g_UseWinHitRect ? L"Windowed" : L"Full Screen");
	return title;
}

///////////////////////////////////////////////////////////////////////////////

std::string GetStateString(WacomMTFingerState state_I)
{
	switch (state_I)
	{
		case WMTFingerStateDown:
		{
			return "D";
		}
		case WMTFingerStateHold:
		{
			return "H";
		}
		case WMTFingerStateUp:
		{
			return "U";
		}
		default:
		{
			return "N";
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Entrypoint (main) function for this application.
//
int APIENTRY _tWinMain(_In_ HINSTANCE hInstance,
							_In_opt_ HINSTANCE hPrevInstance,
							_In_ LPTSTR lpCmdLine,
							_In_ int nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	MSG msg;
	HACCEL hAccelTable;

	InitializeCriticalSection(&g_graphicsCriticalSection);

	// Initialize global strings
	MyRegisterClass(hInstance);

	// Perform application initialization:
	if (!InitInstance (hInstance, nCmdShow))
	{
		return FALSE;
	}

	hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_WACOMMT_SCRIBBLE));

	// Main message loop:
	while (GetMessage(&msg, NULL, 0, 0))
	{
		if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg))
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}

	// Cleanup global variables
	if (g_noConfidenceBrush)
	{
		DeleteObject(g_noConfidenceBrush);
		g_noConfidenceBrush = NULL;
	}

	if ( g_confidenceBrush)
	{
		DeleteObject(g_confidenceBrush);
		g_confidenceBrush = NULL;
	}

	if (g_positionOnlyBrush)
	{
		DeleteObject(g_positionOnlyBrush);
		g_positionOnlyBrush = NULL;
	}

	if (g_noConfidencePen)
	{
		DeleteObject(g_noConfidencePen);
		g_noConfidencePen = NULL;
	}

	if (g_confidencePen)
	{
		DeleteObject(g_confidencePen);
		g_confidencePen = NULL;
	}

	DeleteCriticalSection(&g_graphicsCriticalSection);

	return static_cast<int>(msg.wParam);
}

///////////////////////////////////////////////////////////////////////////////
//	Purpose
//		Registers the window class.
//
ATOM MyRegisterClass(HINSTANCE hInstance)
{
	WNDCLASSEX wcex;

	wcex.cbSize = sizeof(WNDCLASSEX);

	wcex.style				= CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc		= WndProc;
	wcex.cbClsExtra		= 0;
	wcex.cbWndExtra		= 0;
	wcex.hInstance			= hInstance;
	wcex.hIcon				= LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WACOMMT_SCRIBBLE));
	wcex.hCursor			= LoadCursor(NULL, IDC_ARROW);
	wcex.hbrBackground	= (HBRUSH)(COLOR_WINDOW+1);
	wcex.lpszMenuName		= MAKEINTRESOURCE(IDC_WACOMMT_SCRIBBLE);
	wcex.lpszClassName	= szWindowClass.c_str();
	wcex.hIconSm			= LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

	return RegisterClassEx(&wcex);
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Saves instance handle and creates main window
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
	hInst = hInstance; // Store instance handle in our global variable

	g_mainWnd = CreateWindow(szWindowClass.c_str(),
		szTitle.c_str(),
		WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT,
		0,
		CW_USEDEFAULT,
		0,
		NULL,
		NULL,
		hInstance,
		NULL );

	if (!g_mainWnd)
	{
		return FALSE;
	}

	g_hdc = GetDC(g_mainWnd);

	// Create a brush and pens
	g_noConfidenceBrush = CreateSolidBrush(NO_CONFIDENCE_COLOR);
	g_confidenceBrush   = CreateSolidBrush(CONFIDENCE_COLOR);
	g_positionOnlyBrush = CreateSolidBrush(POSITION_ONLY_COLOR);
	g_noConfidencePen   = CreatePen(PS_SOLID, 3, NO_CONFIDENCE_COLOR);
	g_confidencePen     = CreatePen(PS_SOLID, 3, CONFIDENCE_COLOR);
	
	nCmdShow = SW_MAXIMIZE;

	ShowWindow(g_mainWnd, nCmdShow);
	UpdateWindow(g_mainWnd);

	return TRUE;
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Processes messages for the main window.
//
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
		case WM_CREATE:
		{
			WINDOWINFO appWindowInfo = { 0 };
			appWindowInfo.cbSize = sizeof(appWindowInfo);
			GetWindowInfo(hWnd, &appWindowInfo);
			g_clientRect = appWindowInfo.rcClient;

			// Create pens with random colors, which will be assigned to fingerIDs.
			for (int idx = 0; idx < NUM_HPENS; idx++)
			{
				g_hPenMap[idx] = CreatePen(PS_SOLID, 2, RGB(rand() % 255, rand() % 255, rand() % 255));
			}

			// Initialize the Wintab API
			g_tabCtx = InitWintabAPI(hWnd);
			if (!g_tabCtx)
			{
				ShowError("Could Not Open a Wintab Tablet Context.");
			}

			// Initialize the Multi-Touch API
			if (WMTErrorSuccess != InitWacomMTAPI(hWnd))
			{
				ShowError("Could not initialize Wacom Multi-Touch API.");
			}
			break;
		}

		case WM_TIMER:
		{
			return DefWindowProc(hWnd, message, wParam, lParam);
		}

		case WM_CLOSE:
		{
			// Cleanup pens on close
			for (int idx = 0; idx < NUM_HPENS; idx++)
			{
				DeleteObject(g_hPenMap[idx]);
			}
			return DefWindowProc(hWnd, message, wParam, lParam);
		}

		// Handle keyboard input
		case WM_KEYDOWN:
		{
			switch(wParam)
			{
				// Escape key clears the screen
				case VK_ESCAPE:
				{
					ClearScreen();
					break;
				}

				// TODO - Handle other keys here

				default:
				{
					break;
				}
			}
			break;
		}

		case WM_COMMAND:
		{
			// Handle menu selections & changes
			HMENU menu = GetMenu(g_mainWnd);
			WORD wmId = LOWORD(wParam);
			WORD wmEvent = HIWORD(wParam);

			switch (wmId)
			{
				case IDM_ABOUT:
				{
					if (!IsWindow(g_hWndAbout))
					{
						CreateDialog(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
						ShowWindow(g_hWndAbout, SW_SHOW);
					}
					break;
				}

				// Non-confident fingers are usually hidden, if this option is disabled they will be shown
				case IDM_OPTIONS_USECONFIDENCEBITS:
				{
					g_useConfidenceBits = !g_useConfidenceBits;
					CheckMenuItem(menu, IDM_OPTIONS_USECONFIDENCEBITS, (g_useConfidenceBits ? MF_CHECKED : MF_UNCHECKED));
					ClearScreen();
					break;
				}

				// Shows the capabilities of attached devices
				case IDM_OPTIONS_SHOW_CAPS:
				{
					DumpCaps(true);
					break;
				}

				// Change between Consumer (default) and Observer mode.
				// See the Wacom MTAPI Developer docs for more on the difference.
				case IDM_OBSERVER:
				case IDM_CONSUMER:
				{
					bool newMode = wmId == IDM_OBSERVER;
					if (g_ObserverMode != newMode)
					{
						for (size_t idx = 0; idx < g_devices.size(); idx++)
						{
							UnregisterForData(g_devices[idx], hWnd);
						}
						g_ObserverMode = newMode;
						CheckMenuItem(menu, IDM_OBSERVER, (g_ObserverMode ? MF_CHECKED : MF_UNCHECKED));
						CheckMenuItem(menu, IDM_CONSUMER, (g_ObserverMode ? MF_UNCHECKED : MF_CHECKED));
						for (size_t idx = 0; idx < g_devices.size(); idx++)
						{
							RegisterForData(g_devices[idx], hWnd);
						}
					}
					SetWindowTextW(hWnd, GetTitle().c_str());
					ClearScreen();
					break;
				}

				// Toggle between showing the touch size and the touch id next to fingers
				case IDM_SHOW_TOUCH_SIZE:
				case IDM_SHOW_TOUCH_ID:
				{
					g_ShowTouchSize = (wmId == IDM_SHOW_TOUCH_SIZE);
					g_ShowTouchID = (wmId == IDM_SHOW_TOUCH_ID);

					CheckMenuItem(menu, IDM_SHOW_TOUCH_SIZE, (g_ShowTouchSize ? MF_CHECKED : MF_UNCHECKED));
					CheckMenuItem(menu, IDM_SHOW_TOUCH_ID, (g_ShowTouchID ? MF_CHECKED : MF_UNCHECKED));

					ClearScreen();
					break;
				}

				// Toggle the data being shown.
				// Finger (default) is supported by all Wacom Touch tablets
				// See the Wacom MTAPI Developer docs for more on the difference.
				case IDM_FINGER:
				case IDM_BLOB:
				case IDM_RAW:
				{
					EDataType typeHit = (wmId == IDM_FINGER) ? EDataType::EFingerData : (wmId == IDM_BLOB) ? EDataType::EBlobData : EDataType::ERawData;
					for (size_t idx = 0; idx < g_devices.size(); idx++)
					{
						UnregisterForData(g_devices[idx], hWnd);
					}
					if (g_DataType == typeHit)
					{
						g_DataType = EDataType::ENoData;
					}
					else
					{
						g_DataType = typeHit;
					}

					CheckMenuItem(menu, IDM_FINGER, (g_DataType == EDataType::EFingerData ? MF_CHECKED : MF_UNCHECKED));
					CheckMenuItem(menu, IDM_BLOB, (g_DataType == EDataType::EBlobData ? MF_CHECKED : MF_UNCHECKED));
					CheckMenuItem(menu, IDM_RAW, (g_DataType == EDataType::ERawData ? MF_CHECKED : MF_UNCHECKED));

					for (size_t idx = 0; idx < g_devices.size(); idx++)
					{
						RegisterForData(g_devices[idx], hWnd);
					}

					SetWindowTextW(hWnd, GetTitle().c_str());
					ClearScreen();
					break;
				}

				// Wacom MTAPI has different mechanisms for tracking touch. The API can handle it itself
				// or the developer can handle it manually.
				// See the Wacom MTAPI Developer docs for more on the difference.
				case IDM_WINDOW_HANDLES:
				{
					for (size_t idx = 0; idx < g_devices.size(); idx++)
					{
						UnregisterForData(g_devices[idx], hWnd);
					}
					g_UseHWND = !g_UseHWND;

					CheckMenuItem(menu, IDM_WINDOW_HANDLES, (g_UseHWND ? MF_CHECKED : MF_UNCHECKED));
					CheckMenuItem(menu, IDM_WINDOW_RECT, (g_UseHWND || g_UseWinHitRect ? MF_CHECKED : MF_UNCHECKED));
					EnableMenuItem(menu, IDM_WINDOW_RECT, (g_UseHWND ? MF_GRAYED : MF_ENABLED));

					for (size_t idx = 0; idx < g_devices.size(); idx++)
					{
						RegisterForData(g_devices[idx], hWnd);
					}

					SetWindowTextW(hWnd, GetTitle().c_str());
					ClearScreen();
					break;
				}

				case IDM_WINDOW_RECT:
				{
					for (size_t idx = 0; idx < g_devices.size(); idx++)
					{
						UnregisterForData(g_devices[idx], hWnd);
					}

					g_UseWinHitRect = !g_UseWinHitRect;
					CheckMenuItem(menu, IDM_WINDOW_RECT, (g_UseWinHitRect ? MF_CHECKED : MF_UNCHECKED));

					for (size_t idx = 0; idx < g_devices.size(); idx++)
					{
						RegisterForData(g_devices[idx], hWnd);
					}

					SetWindowTextW(hWnd, GetTitle().c_str());
					ClearScreen();
					break;
				}

				case IDM_ERASE:
				{
					ClearScreen();
					break;
				}

				case IDM_EXIT:
				{
					DestroyWindow(hWnd);
					break;
				}

				default:
				{
					return DefWindowProc(hWnd, message, wParam, lParam);
				}
			}
			break;
		}

		case WM_PAINT:
		{
			PAINTSTRUCT ps = {0};
			HDC hdc = BeginPaint(hWnd, &ps);

			// Forcing a "no-op" LineTo allows display to refresh pen data, upon getting
			// this WM_PAINT message due to the InvalidateRect() call in DrawPenData().
			// If also drawing due to finger ellipses, then this hack wouldn't be needed, but
			// we want to use the pen by itself (no touch data).
			// Hack seems to need to do a drawing operation; MoveToEx by itself doesn't work.
			LineTo(hdc, 0, 0);
			EndPaint(hWnd, &ps);
			break;
		}

		case WM_SETTINGCHANGE:
		{
			if (lParam)
			{
				DebugTrace("WM_SETTINGCHANGE %i, %S\n", wParam, lParam);
			}
			else
			{
				DebugTrace("WM_SETTINGCHANGE %i, NULL\n", wParam);
			}
			break;
		}

		case WM_DESTROY:
		{
			ReleaseDC(hWnd, g_hdc);
			if (g_tabCtx)
			{
				gpWTClose(g_tabCtx);
				g_tabCtx = NULL;
			}

			// Return Wintab and MTAPI resources.
			Cleanup();

			PostQuitMessage(0);
			break;
		}

		case WM_SIZE:
		case WM_MOVE:
		{
			WINDOWINFO appWindowInfo = { 0 };
			appWindowInfo.cbSize = sizeof(appWindowInfo);
			GetWindowInfo(hWnd, &appWindowInfo);
			g_clientRect = appWindowInfo.rcClient;

			if (!g_UseHWND)
			{
				// Make sure there's an attached touch tablet.
				for (size_t idx = 0; idx < g_devices.size(); idx++)
				{
					int deviceID = g_devices[idx];
					if (g_caps.count(deviceID) && (g_caps[deviceID].Type == WMTDeviceTypeIntegrated))
					{
						// Resend hitrect size and position to MTAPI
						MoveCallback(deviceID);
					}
				}
			}
			else
			{
				return DefWindowProc(hWnd, message, wParam, lParam);
			}
			break;
		}

		// Handle MTAPI Finger data
		case WM_FINGERDATA:
		{
			DrawFingerData(((WacomMTFingerCollection*)lParam)->FingerCount,
				((WacomMTFingerCollection*)lParam)->Fingers,
				((WacomMTFingerCollection*)lParam)->DeviceID);
			break;
		}

		// Handle MTAPI Blob data
		case WM_BLOBDATA:
		{
			DrawBlobData(((WacomMTBlobAggregate*)lParam)->BlobCount,
				((WacomMTBlobAggregate*)lParam)->BlobArray,
				((WacomMTBlobAggregate*)lParam)->DeviceID);
			break;
		}

		// Capture pen data.
		// Note that the data is being sent in system coordinates.
		case WT_PACKET:
		{
			PACKET wintabPkt;
			static POINT ptOld = {0};
			static POINT ptNew = {0};
			static UINT prsOld = 0;
			static UINT prsNew = 0;

			if (gpWTPacket((HCTX)lParam, wParam, &wintabPkt))
			{
				ptNew.x = wintabPkt.pkX;
				ptNew.y = wintabPkt.pkY;
				prsNew = wintabPkt.pkNormalPressure;

				if ((ptNew.x != ptOld.x) || (ptNew.y != ptOld.y))
				{
					bool bMoveToPoint = ((prsOld == 0) && (prsNew > 0));
					DebugTrace("prsOld: %i, prsNew: %i, ptNew: [%i,%i], ptOld: [%i,%i], moveToPoint: %s\n",
						prsOld, prsNew,
						ptNew.x, ptNew.y,
						ptOld.x, ptOld.y,
						(bMoveToPoint ? "Move" : "Draw"));
					DrawPenData(ptNew, prsNew, bMoveToPoint);

					// Keep track of last time we did move or draw.
					ptOld = ptNew;
					prsOld = prsNew;
				}
			}
			break;
		}

		default:
		{
			return DefWindowProc(hWnd, message, wParam, lParam);
		}
	}
	return 0;
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Message handler for about box.
//
INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	UNREFERENCED_PARAMETER(lParam);
	switch (message)
	{
		case WM_INITDIALOG:
		{
			WacomMTError res = WMTErrorInvalidParam;

			g_hWndAbout = hDlg;
			for (size_t idx = 0; idx < g_devices.size(); idx++)
			{
				int deviceID = g_devices[idx];
				if (g_caps.count(deviceID))
				{
					res = WacomMTRegisterFingerReadHWND(deviceID, WMTProcessingModePassThrough, g_hWndAbout, 5);
					if (res != WMTErrorSuccess)
					{
						break;
					}
				}
			}
			return 1;
		}

		case WM_COMMAND:
		{
			if ((LOWORD(wParam) == IDOK) || (LOWORD(wParam) == IDCANCEL))
			{
				for (size_t idx = 0; idx < g_devices.size(); idx++)
				{
					int deviceID = g_devices[idx];
					if (g_caps.count(deviceID))
					{
						if (WacomMTUnRegisterFingerReadHWND(g_hWndAbout) != WMTErrorSuccess)
						{
							break;
						}
					}
				}
				DestroyWindow(hDlg);
				g_hWndAbout = NULL;

				return 1;
			}
			break;
		}
	}

	return 0;
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Register for finger, blob, and raw data from the MTAPI.
//
WacomMTError RegisterForData(int deviceID_I, HWND hWnd_I)
{
	WacomMTError res = WMTErrorInvalidParam;

	WacomMTHitRectPtr wtHitRect;
	if (g_caps[deviceID_I].Type == WMTDeviceTypeIntegrated)
	{
		wtHitRect = GetAppHitRect();
	}

	switch (g_DataType)
	{
		case EDataType::EFingerData:
		{
			if (g_UseHWND)
			{
				res = WacomMTRegisterFingerReadHWND(deviceID_I, CurrentMode(), hWnd_I, 5);
			}
			else
			{
				res = WacomMTRegisterFingerReadCallback(deviceID_I, wtHitRect.get(), CurrentMode(), FingerCallback, NULL);
			}
			break;
		}

		case EDataType::EBlobData:
		{
			if (g_UseHWND)
			{
				res = WacomMTRegisterBlobReadHWND(deviceID_I, CurrentMode(), hWnd_I, 5);
			}
			else
			{
				res = WacomMTRegisterBlobReadCallback(deviceID_I, wtHitRect.get(), CurrentMode(), BlobCallback, NULL);
			}
			break;
		}

		case EDataType::ERawData:
		{
			res = WacomMTRegisterRawReadCallback(deviceID_I, CurrentMode(), RawCallback, NULL);
			break;
		}

		default:
		{
			break;
		}
	}

	g_lastWTHitRect[deviceID_I] = std::move(wtHitRect);

	return res;
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Callback triggered to deliver movement data from the MTAPI.
//
WacomMTError MoveCallback(int deviceID)
{
	WacomMTError res = WMTErrorInvalidParam;

	WacomMTHitRectPtr wtHitRect = GetAppHitRect();
	if (ClientHitRectChanged(wtHitRect, deviceID))
	{
		switch (g_DataType)
		{
			case EDataType::EFingerData:
			{
				// move registered callback from prev hit rect to new hit rect
				res = WacomMTMoveRegisteredFingerReadCallback(deviceID, g_lastWTHitRect[deviceID].get(), CurrentMode(), wtHitRect.get(), NULL);
				break;
			}

			case EDataType::EBlobData:
			{
				// move registered callback from prev hit rect to new hit rect
				res = WacomMTMoveRegisteredBlobReadCallback(deviceID, g_lastWTHitRect[deviceID].get(), CurrentMode(), wtHitRect.get(), NULL);
				break;
			}

			case EDataType::ERawData:
			{
				res = WacomMTRegisterRawReadCallback(deviceID, CurrentMode(), RawCallback, NULL);
				break;
			}

			default:
			{
				break;
			}
		}

		g_lastWTHitRect[deviceID] = std::move(wtHitRect);
	}
	return res;
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Unregisters a device from the MTAPI.
//
WacomMTError UnregisterForData(int deviceID, HWND hWnd_I)
{
	WacomMTError res = WMTErrorInvalidParam;

	switch (g_DataType)
	{
		case EDataType::EFingerData:
		{
			if (g_UseHWND)
			{
				res = WacomMTUnRegisterFingerReadHWND(hWnd_I);
			}
			else
			{
				res = WacomMTUnRegisterFingerReadCallback(deviceID, g_lastWTHitRect[deviceID].get(), CurrentMode(), NULL);
			}
			break;
		}

		case EDataType::EBlobData:
		{
			if (g_UseHWND)
			{
				res = WacomMTUnRegisterBlobReadHWND(g_mainWnd);
			}
			else
			{
				res = WacomMTUnRegisterBlobReadCallback(deviceID, g_lastWTHitRect[deviceID].get(), CurrentMode(), NULL);
			}
			break;
		}

		case EDataType::ERawData:
		{
			res = WacomMTUnRegisterRawReadCallback(deviceID, CurrentMode(), NULL);
			break;
		}

		default:
		{
			break;
		}
	}

	g_lastWTHitRect[deviceID].reset();

	return res;
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Callback triggered to deliver finger touch data from the MTAPI.
//
int FingerCallback(WacomMTFingerCollection *fingerData, void *userData)
{
	if (fingerData)
	{
		DrawFingerData(fingerData->FingerCount, fingerData->Fingers, fingerData->DeviceID);
	}
	return 0;
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Callback triggered to deliver blob touch data from the MTAPI.
//
int BlobCallback(WacomMTBlobAggregate *blobData, void *userData)
{
	if (blobData)
	{
		DrawBlobData(blobData->BlobCount, blobData->BlobArray, blobData->DeviceID);
	}
	return 0;
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Callback triggered to deliver raw touch data from the MTAPI.
//
int RawCallback(WacomMTRawData *rawData, void *userData)
{
	// rawData->ElementCount should equal caps.ScanX times caps.ScanY
	if (rawData)
	{
		DrawRawData(rawData->ElementCount, rawData->Sensitivity, rawData->DeviceID);
	}
	return 0;
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Callback triggered on device attach.
//
void AttachCallback(WacomMTCapability deviceInfo, void *userRef)
{
	if (!g_caps.count(deviceInfo.DeviceID))
	{
		g_devices.push_back(deviceInfo.DeviceID);
		g_caps[deviceInfo.DeviceID] = deviceInfo;

		RegisterForData(deviceInfo.DeviceID, g_mainWnd); // Ignore result
	}
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Callback triggered on device detach.
//
void DetachCallback(int deviceID, void *userRef)
{
	if (g_caps.count(deviceID))
	{
		UnregisterForData(deviceID, g_mainWnd);
		std::vector<int>::const_iterator iter = std::find(g_devices.begin(), g_devices.end(), deviceID);
		if (iter != g_devices.end())
		{
			g_devices.erase(iter);
		}
		g_caps.erase(deviceID);
	}
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Calculates the rotated x & y of a given set of points around a centeral
//		pivot.
//
void Rotate(double degrees, const POINT& centerPnt, std::vector<POINT>& points)
{
	double rad = (90 - degrees) * 3.14159 / 180.0;

	for (UINT idx = 0; idx < points.size(); idx++)
	{
		points[idx].x = static_cast<LONG>(cos(rad)
			* (static_cast<double>(points[idx].x) - centerPnt.x) - (sin(rad)
				* (static_cast<double>(points[idx].y) - centerPnt.y)) + centerPnt.x);
		points[idx].y = static_cast<LONG>(sin(rad)
			* (static_cast<double>(points[idx].x) - centerPnt.x) + (cos(rad)
				* (static_cast<double>(points[idx].y) - centerPnt.y)) + centerPnt.y);
	}
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Draws Finger data from the MTAPI.
//
void DrawFingerData(int count, WacomMTFinger *fingers, int device)
{
	if (g_devices.size() && count && fingers)
	{
		EnterCriticalSection(&g_graphicsCriticalSection);

		for (int index = 0; index < count; index++)
		{
			DebugTrace("TC[%i], confidence: %i\n", fingers[index].FingerID, fingers[index].Confidence);

			if (!g_fingerHPenMap.count(fingers[index].FingerID))
			{
				g_fingerHPenMap[fingers[index].FingerID] = g_hPenMap[index];
			}

			if (fingers[index].TouchState != WMTFingerStateNone)
			{
				// Skip this finger if using confidence bits and it's NOT confident.
				if (g_useConfidenceBits && !fingers[index].Confidence)
				{
					continue;
				}

				HPEN pen = g_fingerHPenMap[fingers[index].FingerID];
				HPEN oldPen = (HPEN)SelectObject(g_hdc, pen);

				// Display tablets report position in pixels.
				double x = fingers[index].X;
				double y = fingers[index].Y;
				if (g_caps[device].Type == WMTDeviceTypeOpaque)
				{
					// If we're using an opaque tablet, then the X and Y values are not in
					// pixels; they are a percentage of the tablet width (eg: 0.123, 0.37, etc.).
					// We need to convert to client pixels.
					x *= static_cast<double>(g_clientRect.right) - g_clientRect.left;
					x += g_clientRect.left;
					y *= static_cast<double>(g_clientRect.bottom) - g_clientRect.top;
					y += g_clientRect.top ;
				}

				//map to our window
				POINT pt = {static_cast<LONG>(x), static_cast<LONG>(y)};
				::ScreenToClient(g_mainWnd, &pt);

				// If width and height are not supported; we will fake it.
				double widthMM = 0.;
				double widthMaxMM = 0.;

				if (fingers[index].Width > 0)
				{
					// Make larger for visibility, if necessary.
					if (fingers[index].Width <= 1.0)
					{
						// Convert logical value to millimeters by multiplying by physical width.
						widthMM = static_cast<double>(fingers[index].Width) * g_caps[device].PhysicalSizeX;
					}
					else //must be Cintiq so width already in pixels
					{
						widthMM = static_cast<double>(fingers[index].Width) * DISPLAY_TAB_DRAW_SIZE_FACTOR; //pixel pitch of Cintiq 24 HD
					}
				}

				int contactWidthOffset = static_cast<int>(widthMM / DISPLAY_TAB_DRAW_SIZE_FACTOR / 2);

				double heightMM = 0.;
				if (fingers[index].Height > 0)
				{
					// Make larger for visibility, if necessary.
					if (fingers[index].Height <= 1.0)
					{
						// Convert logical value to millimeters by multiplying by physical height.
						heightMM = static_cast<double>(fingers[index].Height) * g_caps[device].PhysicalSizeY;
					}
					else //must be Cintiq so height already in pixels
					{
						heightMM = static_cast<double>(fingers[index].Height) * DISPLAY_TAB_DRAW_SIZE_FACTOR; //pixel pitch of Cintiq 24 HD
					}
				}
				int contactHeightOffset = static_cast<int>(heightMM / DISPLAY_TAB_DRAW_SIZE_FACTOR / 2);

				DebugTrace("width, height (mm): %3.3f,%3.3f\n", widthMM, heightMM);

				// Draw a larger ellipse around essentially a dot.
				// Fill in any con-confident contacts.
				HBRUSH oBrush = 0;

				if (!fingers[index].Confidence)
				{
					oBrush = static_cast<HBRUSH>(SelectObject(g_hdc, g_noConfidenceBrush));
				}

				CenterEllipse(g_hdc, pt.x, pt.y, contactWidthOffset, contactHeightOffset);

				wchar_t displayText[8] = L"";
				if (g_ShowTouchSize)
				{
					_stprintf_s(displayText, L"%.1f", widthMM);
				}
				else if (g_ShowTouchID)
				{
					_stprintf_s(displayText, L"%i", fingers[index].FingerID);
				}
				// else bubble will be blank
				TextOut(g_hdc, pt.x, pt.y, displayText, _tcslen(displayText));

				if (!fingers[index].Confidence)
				{
					SelectObject(g_hdc, oBrush);
				}

				// Display finger stats in upper left corner.
				wchar_t fingerStr[128] = L"";
				_stprintf_s( fingerStr, L"Finger:%d ID:%d Xtab:%.2f Ytab:%.2f W:%.2f [%.2f mm]  H:%.2f [%.2f mm]  Angle:%.0f      \n",
					index, fingers[index].FingerID, fingers[index].X, fingers[index].Y,
					fingers[index].Width, widthMM, fingers[index].Height, heightMM, fingers[index].Orientation );
				TextOut(g_hdc, 50, 20, fingerStr, _tcslen(fingerStr));

				SelectObject(g_hdc, oldPen);
			}
		}

		LeaveCriticalSection(&g_graphicsCriticalSection);
	}
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Clears the canvas of finger and pen data.
//
void ClearScreen()
{
	RECT cRect = g_clientRect;
	cRect.right -= cRect.left;
	cRect.left = 0;
	cRect.bottom -= cRect.top;
	cRect.top = 0;

	FillRect(g_hdc, &cRect, static_cast<HBRUSH>(GetStockObject((g_ObserverMode ? COLOR_WINDOW : COLOR_APPWORKSPACE) + 1)));
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Determines the applications hit rect.
//
WacomMTHitRectPtr GetAppHitRect()
{
	if (g_UseWinHitRect)
	{
		WINDOWINFO appWindowInfo = {0};
		appWindowInfo.cbSize = sizeof(appWindowInfo);
		GetWindowInfo(g_mainWnd, &appWindowInfo);

		WacomMTHitRect hitRect = {
			static_cast<float>(appWindowInfo.rcClient.left),
			static_cast<float>(appWindowInfo.rcClient.top),
			static_cast<float>(appWindowInfo.rcClient.right - appWindowInfo.rcClient.left),
			static_cast<float>(appWindowInfo.rcClient.bottom - appWindowInfo.rcClient.top)
		};

		return WacomMTHitRectPtr(new WacomMTHitRect(hitRect));
	}
	return WacomMTHitRectPtr();
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Draw Raw data from the MTAPI.
//
void DrawRawData(int count, unsigned short* rawBuf, int device)
{
	SIZE rawSize = {g_caps[device].ScanSizeX, g_caps[device].ScanSizeY};
	if (count && rawBuf)
	{
		ClearScreen();

		HPEN pen = CreatePen(PS_SOLID, 2, RGB(255,0,0));
		HPEN oldPen = static_cast<HPEN>(SelectObject(g_hdc, pen));

		for (int sy = 0; sy < rawSize.cy; sy++)
		{
			for (int sx = 0; sx < rawSize.cx; sx++)
			{
				unsigned short value = rawBuf[sy * rawSize.cx + sx];
				if (value > 4)
				{
					int X = sx * static_cast<int>(g_caps[device].LogicalWidth)  / rawSize.cx + static_cast<int>(g_caps[device].LogicalOriginX);
					int Y = sy * static_cast<int>(g_caps[device].LogicalHeight) / rawSize.cy + static_cast<int>(g_caps[device].LogicalOriginY);
					int offset = std::max(value * 6 / 255 + 5, 7);

					if ((X > g_clientRect.left) && (X < g_clientRect.right) &&
						 (Y > g_clientRect.top) &&  (Y < g_clientRect.bottom))
					{
						POINT pt = {X, Y};
						::ScreenToClient(g_mainWnd, &pt);

						Circle(g_hdc, pt.x, pt.y, offset);
					}
				}
			}
		}

		SelectObject(g_hdc, oldPen);
		DeleteObject(pen);
	}
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Calculates the center point given a set of points.
//
POINT FindCenterPoint(int count, WacomMTBlobPoint *points)
{
	UINT32 msig = 0;
	UINT32 wmx = 0;
	UINT32 wmy = 0;

	for (int i = 0; i < count; i++)
	{
		msig += points[i].Sensitivity;
		wmx += static_cast<UINT32>(points[i].X * points[i].Sensitivity);
		wmy += static_cast<UINT32>(points[i].Y * points[i].Sensitivity);
	}

	POINT center = {0,0};
	if (msig)
	{
		center.x = wmx / msig;
		center.y = wmy / msig;
	}
	return center;
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Draw a single blob from the MTAPI.
//
void DrawBlob(int count, WacomMTBlobPoint *points)
{
	if (count && points)
	{
		WacomMTBlobPoint apiPoint = points[0];
		POINT curPt = {static_cast<LONG>(apiPoint.X), static_cast<LONG>(apiPoint.Y)};
		::ScreenToClient(g_mainWnd, &curPt);

		for (int pointIndex = 1; pointIndex <= count; pointIndex++)
		{
			bool bDrawLine = apiPoint.Sensitivity > 0;
			apiPoint = points[pointIndex == count ? 0 : pointIndex];
			POINT prevPt = curPt;
			curPt.x = static_cast<LONG>(apiPoint.X);
			curPt.y = static_cast<LONG>(apiPoint.Y);
			::ScreenToClient(g_mainWnd, &curPt);

			if (bDrawLine)
			{
				MoveToEx(g_hdc, prevPt.x, prevPt.y, NULL);
				LineTo(g_hdc, curPt.x, curPt.y);
			}
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Draw blob data from the MTAPI.
//
void DrawBlobData(int count, WacomMTBlob *blobs, int device)
{
	if (count && blobs)
	{
		blobs->BlobID;
		blobs->BlobType;
		blobs->ParentID;

		ClearScreen();

		{
			POINT pt = { static_cast<LONG>(blobs->X), static_cast<LONG>(blobs->Y)};

			if ((pt.x > g_clientRect.left) && (pt.x < g_clientRect.right) &&
				 (pt.y > g_clientRect.top) &&  (pt.y < g_clientRect.bottom))
			{
				//map to our window
				::ScreenToClient(g_mainWnd, &pt);
				Circle(g_hdc, pt.x, pt.y, 2);
			}

			for (int blobIndex = 0; blobIndex < count; blobIndex++)
			{
				bool confident = blobs[blobIndex].Confidence;

				if ( g_useConfidenceBits && !confident )
				{
					continue;	// skip drawing this blob
				}

				HPEN oldPen = static_cast<HPEN>(SelectObject(g_hdc, 
					confident ? g_confidencePen : g_noConfidencePen));

				DrawBlob(blobs[blobIndex].PointCount, blobs[blobIndex].BlobPoints);

				SelectObject(g_hdc, oldPen);
			}
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
// Wintab support functions

///////////////////////////////////////////////////////////////////////////////
//  Purpose
//		Loads the Wintab32 DLL and sets up the API function pointers.
//
HCTX InitWintabAPI(HWND hwnd_I)
{
	AXIS Pressure = { 0 };

	if (!LoadWintab())
	{
		ShowError("Wintab not available");
		return 0;
	}

	char TabletName[50] = "";
	gpWTInfoA(WTI_DEVICES, DVC_NAME, TabletName);
	gpWTInfoA(WTI_INTERFACE, IFC_WINTABID, TabletName);

	// check if WinTab available.
	if (!gpWTInfoA(0, 0, NULL))
	{
		ShowError("WinTab Services Not Available.");
		return FALSE;
	}

	// get default region
	LOGCONTEXTA logContext = {0};
	gpWTInfoA(WTI_DEFSYSCTX, 0, &logContext);

	// No need to specify lcInOrg* and lcInExt* as they are the entire
	// physical tablet area by default.

	// Get messages
	logContext.lcOptions |= CXO_MESSAGES;

	// Move the system cursor.
	logContext.lcOptions |= CXO_SYSTEM;

	logContext.lcPktData = PACKETDATA;
	logContext.lcPktMode = PACKETMODE;
	logContext.lcMoveMask = PACKETDATA;
	logContext.lcBtnUpMask = logContext.lcBtnDnMask;

	// In Wintab, the tablet origin is lower left.  Move origin to upper left
	// so that it coincides with screen origin.
	logContext.lcOutExtY = -GetSystemMetrics(SM_CYVIRTUALSCREEN);
	
	gpWTInfoA(WTI_DEVICES + 0, DVC_NPRESSURE, &Pressure);
	g_maxPressure = Pressure.axMax;

	// open the region
	return gpWTOpenA(hwnd_I, (LPLOGCONTEXT)&logContext, TRUE);
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Initialize the Wacom Multi-Touch API
//
WacomMTError InitWacomMTAPI(HWND hWnd_I)
{
	WacomMTError res = WacomMTInitialize(WACOM_MULTI_TOUCH_API_VERSION);
	if (res != WMTErrorSuccess)
	{
		return res;
	}

	int deviceCount = WacomMTGetAttachedDeviceIDs(NULL, 0);
	if (deviceCount)
	{
		int newCount = 0;
		while (newCount != deviceCount)
		{
			g_devices.resize(deviceCount, 0);
			newCount = WacomMTGetAttachedDeviceIDs(&g_devices[0], deviceCount * sizeof(int));
		}

		DumpCaps(false);
	}

	res = WacomMTRegisterAttachCallback(AttachCallback, NULL); 
	if (res != WMTErrorSuccess)
	{
		return res;
	}

	res = WacomMTRegisterDetachCallback(DetachCallback, NULL); 
	if (res != WMTErrorSuccess)
	{
		return res;
	}

	int loopCount = static_cast<int>(g_devices.size());
	for (int idx = 0; idx < loopCount; idx++)
	{
		res = RegisterForData(g_devices[idx], hWnd_I); 
		if (res != WMTErrorSuccess)
		{
			return res;
		}
	}

	return WMTErrorSuccess;
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Drawing routine for pen data
//
void DrawPenData(POINT point_I, UINT pressure_I, bool bMoveToPoint_I)
{
	// Prevent hover drawing.
	if (!pressure_I)
	{
		return;
	}

	EnterCriticalSection(&g_graphicsCriticalSection);

	int penWidth = static_cast<int>(1 + std::floor(10 * (double) pressure_I / (double) g_maxPressure));
	HPEN pen = CreatePen(PS_SOLID, penWidth, RGB(0, 0, 255));
	HPEN oldPen = static_cast<HPEN>(SelectObject(g_hdc, pen));

	POINT ptNew = point_I;

	// Compare the new point with cached client rectangle in screen coordinates.
	if ((ptNew.x >= g_clientRect.left) &&
		 (ptNew.y >= g_clientRect.top) &&
		 (ptNew.x <= g_clientRect.right) &&
		 (ptNew.y <= g_clientRect.bottom))
	{
		// Convert from screen to client coordinates to render.
		// This will let us put the app window anywhere on the desktop.
		::ScreenToClient(g_mainWnd, &ptNew);
		DebugTrace("\tX:%ld  Y:%ld\n", ptNew.x, ptNew.y);

		// Move to a starting point if so directed.
		// Prevents streaks from last draw point or edge of client.
		if (bMoveToPoint_I)
		{
			DebugTrace("MoveTo: %i, %i\n", ptNew.x, ptNew.y);
			MoveToEx(g_hdc, ptNew.x, ptNew.y, NULL);
		}
		else
		{
			DebugTrace("LineTo: %i, %i\n", ptNew.x, ptNew.y);
			LineTo(g_hdc, ptNew.x, ptNew.y);
		}
	}

	SelectObject(g_hdc, oldPen);
	DeleteObject(pen);

	InvalidateRect(g_mainWnd, NULL, FALSE);

	LeaveCriticalSection(&g_graphicsCriticalSection);
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Release resources we used in this example.
//
void Cleanup(void)
{
	// Release MTAPI resources.
	WacomMTQuit();

	// Release Wintab resources.
	UnloadWintab();
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Dump the MT capabilities of the attached devices.
//
void DumpCaps(bool showMessageBox_I)
{
	std::stringstream msg;
	WacomMTError res = WMTErrorInvalidParam;

	for (int idx = 0; idx < static_cast<int>(g_devices.size()); idx++)
	{
		WacomMTCapability cap = { 0 };
		res = WacomMTGetDeviceCapabilities(g_devices[idx], &cap);
		if (res != WMTErrorSuccess)
		{
			break;
		}

		g_caps[g_devices[idx]] = cap;

		msg << "MT Capabilities for idx: " << idx << std::endl;
		msg << "\tVersion: " << g_caps[idx].Version << std::endl;
		msg << "\tdeviceID_I: " << g_caps[idx].DeviceID << std::endl;
		msg << "\tType: " << (int)g_caps[idx].Type << std::endl;
		msg << "\tLogicalOriginX: " << g_caps[idx].LogicalOriginX << std::endl;
		msg << "\tLogicalOriginY: " << g_caps[idx].LogicalOriginY << std::endl;
		msg << "\tLogicalWidth: " << g_caps[idx].LogicalWidth << std::endl;
		msg << "\tLogicalHeight: " << g_caps[idx].LogicalHeight << std::endl;
		msg << "\tPhysicalSizeX: " << g_caps[idx].PhysicalSizeX << "\n";
		msg << "\tPhysicalSizeY: " << g_caps[idx].PhysicalSizeY << "\n";
		msg << "\tReportedSizeX: " << g_caps[idx].ReportedSizeX << "\n";
		msg << "\tReportedSizeY: " << g_caps[idx].ReportedSizeY << "\n";
		msg << "\tScanSizeX: " << g_caps[idx].ScanSizeX << "\n";
		msg << "\tScanSizeY: " << g_caps[idx].ScanSizeY << "\n";
		msg << "\tFingerMax: " << g_caps[idx].FingerMax << "\n";
		msg << "\tBlobMax: " << g_caps[idx].BlobMax << "\n";
		msg << "\tBlobPointsMax: " << g_caps[idx].BlobPointsMax << "\n";
		msg << "\tCapabilityFlags: " << std::hex << static_cast<int>(g_caps[idx].CapabilityFlags) << "\n\n";
	}

	DebugTrace("%s\n", msg.str().c_str());

	if (showMessageBox_I)
	{
		MessageBoxA(NULL, msg.str().c_str(), "Capabilities", MB_OK);
	}
}

///////////////////////////////////////////////////////////////////////////////
// Purpose
//		Returns true if client hit rect changed from last time.
//
bool ClientHitRectChanged(const WacomMTHitRectPtr& wtHitRect_I, int deviceID)
{
	if (!wtHitRect_I && !g_lastWTHitRect[deviceID]) return false;
	if (!wtHitRect_I || !g_lastWTHitRect[deviceID]) return true;
	return ( (wtHitRect_I->originX != g_lastWTHitRect[deviceID]->originX)
			|| (wtHitRect_I->originY != g_lastWTHitRect[deviceID]->originY)
			|| (wtHitRect_I->width   != g_lastWTHitRect[deviceID]->width  )
			|| (wtHitRect_I->height  != g_lastWTHitRect[deviceID]->height ));
}

///////////////////////////////////////////////////////////////////////////////
