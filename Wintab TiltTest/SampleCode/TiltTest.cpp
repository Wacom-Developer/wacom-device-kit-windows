///////////////////////////////////////////////////////////////////////////////
//
//	DESCRIPTION
//		This demo shows how to use Wintab to detect/display pen tilt input.
//
//	COPYRIGHT
//		Copyright (C) 1998  LCS/Telegraphics
//		Copyright (c) 2014-2020 Wacom Co., Ltd.
//		All rights reserved.
//
///////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

#define GET_X_LPARAM(lp)                        ((int)(short)LOWORD(lp))
#define GET_Y_LPARAM(lp)                        ((int)(short)HIWORD(lp))

#include <msgpack.h>
#include <wintab.h>
#define PACKETDATA	(PK_X | PK_Y | PK_ORIENTATION)
#define PACKETMODE	0
#include <pktdef.h>
#include "Utils.h"

#include "TiltTest.h"

constexpr int MAX_LOADSTRING = 100;

double		aziFactor = 1.0;       /* Azimuth factor */
double		altFactor = 1.0;       /* Altitude factor */
double		altAdjust = 1.0;       /* Altitude zero adjust */
BOOL			tilt_support = TRUE;   /* Is tilt supported */
ORIENTATION	ortNew;                /* Tilt value storage */
RECT			rcClient;              /* Size of current Client */
RECT			rcInfoTilt;            /* Size of tilt info box */
RECT			rcDraw;                /* Size of draw area */

/* converts FIX32 to double */
#define FIX_DOUBLE(x)	((double)(INT(x)) + ((double)FRAC(x) / 65536))
#define pi 3.14159265359

#ifdef WIN32
#define MoveTo(h,x,y)	MoveToEx(h,x,y,NULL)
#endif

////////////////////////////////////////////////////////////////////////////////
// Global Variables:

HINSTANCE hInst;								// current instance
TCHAR szTitle[MAX_LOADSTRING];			// The title bar text
TCHAR szWindowClass[MAX_LOADSTRING];	// the main window class name

char* gpszProgramName = "TiltTest";
static LOGCONTEXT glogContext = { 0 };

//////////////////////////////////////////////////////////////////////////////
// Forward declarations of functions included in this code module:
ATOM					MyRegisterClass(HINSTANCE);
BOOL					InitInstance(HINSTANCE, int);
LRESULT CALLBACK	WndProc(HWND, UINT, WPARAM, LPARAM);
INT_PTR CALLBACK	About(HWND, UINT, WPARAM, LPARAM);

HCTX static NEAR TabletInit(HWND hWnd);
void Cleanup(void);

////////////////////////////////////////////////////////////////////////////////
int APIENTRY _tWinMain(
	_In_ HINSTANCE hInstance,
	_In_opt_ HINSTANCE hPrevInstance,
	_In_ LPTSTR    lpCmdLine,
	_In_ int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	// TODO: Place code here.
	MSG msg;
	HACCEL hAccelTable;

	// Initialize global strings
	LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
	LoadString(hInstance, IDC_TILTTEST, szWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);

	// Perform application initialization:
	if (!InitInstance(hInstance, nCmdShow))
	{
		return FALSE;
	}

	hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_TILTTEST));

	// Main message loop:
	while (GetMessage(&msg, NULL, 0, 0))
	{
		if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg))
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}

	// Return Wintab resources.
	Cleanup();

	return static_cast<int>(msg.wParam);
}

//////////////////////////////////////////////////////////////////////////////
//
//  FUNCTION: MyRegisterClass()
//
//  PURPOSE: Registers the window class.
//
//  COMMENTS:
//
//    This function and its usage are only necessary if you want this code
//    to be compatible with Win32 systems prior to the 'RegisterClassEx'
//    function that was added to Windows 95. It is important to call this function
//    so that the application will get 'well formed' small icons associated
//    with it.
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
	wcex.hIcon				= LoadIcon(hInstance, MAKEINTRESOURCE(IDI_TILTTEST));
	wcex.hCursor			= LoadCursor(NULL, IDC_ARROW);
	wcex.hbrBackground	= (HBRUSH)(COLOR_APPWORKSPACE + 1);
	wcex.lpszMenuName		= MAKEINTRESOURCE(IDC_TILTTEST);
	wcex.lpszClassName	= szWindowClass;
	wcex.hIconSm			= LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

	return RegisterClassEx(&wcex);
}

//////////////////////////////////////////////////////////////////////////////
//
//  FUNCTION: InitInstance(HINSTANCE, int)
//
//  PURPOSE: Saves instance handle and creates main window
//
//  COMMENTS:
//
//      In this function, we save the instance handle in a global variable and
//      create and display the main program window.
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
	hInst = hInstance; // Store instance handle in our global variable

	if (!LoadWintab())
	{
		ShowError("Wintab not available");
		return FALSE;
	}

	/* check if WinTab available. */
	if (!gpWTInfoA(0, 0, NULL))
	{
		ShowError("WinTab Services Not Available.");
		return FALSE;
	}

	/* check if WACOM available. */
	char WName[50];		/* String to hold window name */
	gpWTInfoA(WTI_DEVICES, DVC_NAME, WName);
	if (strncmp(WName, "WACOM", 5))
	{
		MessageBox(NULL, "Wacom Tablet Not Installed.", gpszProgramName, MB_OK | MB_ICONERROR);
		return FALSE;
	}
	/* get info about tilt */
	struct tagAXIS TpOri[3]; /* The capabilities of tilt */
	double tpvar;            /* A temp for converting fix to double */

	tilt_support = gpWTInfoA(WTI_DEVICES, DVC_ORIENTATION, &TpOri);
	if (tilt_support)
	{
		/* does the tablet support azimuth and altitude */
		if (TpOri[0].axResolution && TpOri[1].axResolution)
		{
			/* convert azimuth resulution to double */
			tpvar = FIX_DOUBLE(TpOri[0].axResolution);
			/* convert from resolution to radians */
			aziFactor = tpvar / (2 * pi);

			/* convert altitude resolution to double */
			tpvar = FIX_DOUBLE(TpOri[1].axResolution);
			/* scale to arbitrary value to get decent line length */
			altFactor = tpvar / 1000;
			/* adjust for maximum value at vertical */
			altAdjust = (double)TpOri[1].axMax / altFactor;
		}
		else
		{
			/* no so dont do tilt stuff */
			tilt_support = FALSE;
		}
	}
	HWND hWnd = CreateWindow(
		szWindowClass, szTitle,
		WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT, CW_USEDEFAULT,
		CW_USEDEFAULT, CW_USEDEFAULT,
		NULL,
		NULL,
		hInstance,
		NULL);

	if (!hWnd)
	{
		return FALSE;
	}
	/* Get Device Context and setup a rects  to write packet info */
	HDC hDC = GetDC(hWnd);
	if (!hDC)
	{
		return FALSE;
	}
	TEXTMETRIC textmetric;                                             /* Structure for font info */
	GetTextMetrics(hDC, &textmetric);
	int nLineH = textmetric.tmExternalLeading + textmetric.tmHeight;   /* Holds the text height */

	int Xinch = GetDeviceCaps(hDC, LOGPIXELSX);                        /* Holds the number of pixels per inch */
	int Yinch = GetDeviceCaps(hDC, LOGPIXELSY);

	int Hres = GetDeviceCaps(hDC, HORZRES);                            /* Holds the screen resolution */
	int Vres = GetDeviceCaps(hDC, VERTRES);
	ReleaseDC(hWnd, hDC);

	GetClientRect(hWnd, &rcClient);
	rcInfoTilt = rcClient;
	rcInfoTilt.left = Xinch / 8;
	rcInfoTilt.top  = Yinch / 8;
	rcInfoTilt.bottom = rcInfoTilt.top + nLineH;

	rcDraw = rcInfoTilt;
	rcDraw.left = 0;
	rcDraw.top += nLineH;
	rcDraw.bottom = rcClient.bottom;

	ShowWindow(hWnd, nCmdShow);
	UpdateWindow(hWnd);

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//
//  FUNCTION: WndProc(HWND, UINT, WPARAM, LPARAM)
//
//  PURPOSE:  Processes messages for the main window.
//
//  WM_COMMAND	- process the application menu
//  WM_PAINT	- Paint the main window
//  WM_DESTROY	- post a quit message and return
//
LRESULT CALLBACK WndProc(
	HWND hWnd,
	UINT message,
	WPARAM wParam,
	LPARAM lParam)
{
	int wmId;
	int wmEvent;
	HDC hDC;               /* handle for Device Context */

	static HCTX hCtx = NULL;
	static POINT ptNew;
	static RECT rcClient;
	PAINTSTRUCT psPaint;
	PACKET pkt;

	const char* p = "Tilt: 90.0, Theta: 360.0 ";
	size_t c = strlen(p);

	switch (message)
	{
	case WM_CREATE:
		hCtx = TabletInit(hWnd);
		if (!hCtx)
		{
			MessageBox(NULL, "Could Not Open Tablet Context.", "WinTab", MB_OK | MB_ICONERROR);
			SendMessage(hWnd, WM_DESTROY, 0, 0L);
		}
		break;

	case WM_SIZE:
		GetClientRect(hWnd, &rcClient);

		if (hDC = BeginPaint(hWnd, &psPaint))
		{
			SIZE sizl;
			GetTextExtentPoint32(hDC, p, c, &sizl);

			rcInfoTilt.right  = rcInfoTilt.left + sizl.cx;
			rcInfoTilt.bottom = rcInfoTilt.top  + sizl.cy;
			EndPaint(hWnd, &psPaint);
		}
		rcDraw.right     = rcClient.right;
		rcDraw.bottom    = rcClient.bottom;
		/* redraw the entire window */
		InvalidateRect(hWnd, NULL, TRUE);
		break;

	case WM_COMMAND:
		wmId    = GET_WM_COMMAND_ID(wParam, lParam);
		wmEvent = HIWORD(wParam);
		// Parse the menu selections:
		switch (wmId)
		{
		case IDM_ABOUT:
			DialogBox(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
			break;
		case IDM_EXIT:
			DestroyWindow(hWnd);
			break;
		default:
			return DefWindowProc(hWnd, message, wParam, lParam);
		}
		break;

	case WM_PAINT:
		{
			int ZAngle;         /* Raw Altitude */
			UINT Theta;         /* Raw Azimuth */
			POINT Z1Angle;      /* Rect coords from polar coords */

			if (tilt_support)
			{
				double ZAngle2;     /* Adjusted Altitude */
				double Theta2;      /* Adjusted Azimuth */
				/*
					wintab.h defines .orAltitude
					as a UINT but documents .orAltitude
					as positive for upward angles
					and negative for downward angles.
					WACOM uses negative altitude values to
					show that the pen is inverted;
					therefore we cast .orAltitude as an
					(int) and then use the absolute value.
				*/
				ZAngle = /*(int)*/ortNew.orAltitude;
				ZAngle2 = altAdjust - abs((double)ZAngle) / altFactor;
				/* adjust azimuth */
				Theta = ortNew.orAzimuth;
				Theta2 = (double)Theta / aziFactor;
				/* get the length of the diagonal to draw */
				Z1Angle.x = (LONG)(ZAngle2 * sin(Theta2));
				Z1Angle.y = (LONG)(ZAngle2 * cos(Theta2));
			}
			else
			{
				Theta = 0;
				Z1Angle.x = 0;
				Z1Angle.y = 0;
			}

			char szOutput[128]; /* String for outputs */

			if (hDC = BeginPaint(hWnd, &psPaint))
			{
				POINT scrPoint = { ptNew.x, ptNew.y };
				ScreenToClient(hWnd, &scrPoint);

				/* write raw tilt info */
				if (tilt_support)
				{
					wsprintf((LPSTR)szOutput, "Tilt: %3d.%d, Theta: %3u.%u\0", ZAngle / 10, ZAngle % 10, Theta / 10, Theta % 10);
				}
				else
				{
					strcpy(szOutput, "Tilt not supported.");
				}
				DrawText(hDC, szOutput, strlen(szOutput), &rcInfoTilt, DT_LEFT);

				/* draw a line based on stylus tilt */
				MoveTo(hDC, scrPoint.x, scrPoint.y);
				LineTo(hDC, scrPoint.x + Z1Angle.x, scrPoint.y - Z1Angle.y);

				/* draw crosshairs based on tablet position */
				MoveTo(hDC, scrPoint.x - 20, scrPoint.y);
				LineTo(hDC, scrPoint.x + 20, scrPoint.y);
				MoveTo(hDC, scrPoint.x, scrPoint.y - 20);
				LineTo(hDC, scrPoint.x, scrPoint.y + 20);

				EndPaint(hWnd, &psPaint);
			}
		}
		break;

	case WM_DESTROY:
		if (hCtx)
		{
			gpWTClose(hCtx);
		}
		PostQuitMessage(0);
		break;

	case WT_PACKET:
		if (gpWTPacket((HCTX)lParam, wParam, &pkt))
		{
			/* old coordinates used for comparisons */
			POINT ptOld = ptNew;
			ORIENTATION ortOld = ortNew;
			
			/* save new coordinates */
			ptNew.x = (UINT)pkt.pkX;
			ptNew.y = (UINT)pkt.pkY;
			ortNew = pkt.pkOrientation;

			/* If the visual changes update the main graphic */
			if (  (ptNew.x != ptOld.x)
				|| (ptNew.y != ptOld.y)
				|| (ortNew.orAzimuth != ortOld.orAzimuth)
				|| (ortNew.orAltitude != ortOld.orAltitude)
				|| (ortNew.orTwist != ortOld.orTwist))
			{
				InvalidateRect(hWnd, &rcDraw, TRUE);
			}
			/* if the displayed data changes update the text */
			if (  (ortNew.orAzimuth != ortOld.orAzimuth)
				|| (ortNew.orAltitude != ortOld.orAltitude)
				|| (ortNew.orTwist != ortOld.orTwist))
			{
				InvalidateRect(hWnd, &rcInfoTilt, TRUE);
			}
		}
		break;

	case WM_ACTIVATE:
		if (GET_WM_ACTIVATE_STATE(wParam, lParam))
		{
			InvalidateRect(hWnd, NULL, TRUE);
		}
		/* if switching in the middle, disable the region */
		if (hCtx)
		{
			gpWTEnable(hCtx, GET_WM_ACTIVATE_STATE(wParam, lParam));
			if (hCtx && GET_WM_ACTIVATE_STATE(wParam, lParam))
			{
				gpWTOverlap(hCtx, TRUE);
			}
		}
		break;

	default:
		return DefWindowProc(hWnd, message, wParam, lParam);
	}
	return 0;
}

//////////////////////////////////////////////////////////////////////////////
// Message handler for about box.
INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	UNREFERENCED_PARAMETER(lParam);
	switch (message)
	{
	case WM_INITDIALOG:
		return (INT_PTR)TRUE;

	case WM_COMMAND:
		if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL)
		{
			EndDialog(hDlg, LOWORD(wParam));
			return (INT_PTR)TRUE;
		}
		break;
	}
	return (INT_PTR)FALSE;
}

///////////////////////////////////////////////////////////////////////////////

HCTX static NEAR TabletInit(HWND hWnd)
{
	LOGCONTEXT      lcMine;           /* The context of the tablet */
	AXIS            TabletX, TabletY; /* The maximum tablet size */

	/* get default region */
	gpWTInfoA(WTI_DEFCONTEXT, 0, &lcMine);

	/* modify the digitizing region */
	wsprintf(lcMine.lcName, "TiltTest Digitizing %p", hInst);
	lcMine.lcOptions |= CXO_MESSAGES;
	lcMine.lcPktData = PACKETDATA;
	lcMine.lcPktMode = PACKETMODE;
	lcMine.lcMoveMask = PACKETDATA;
	lcMine.lcBtnUpMask = lcMine.lcBtnDnMask;

	/* Set the entire tablet as active */
	gpWTInfoA(WTI_DEVICES, DVC_X, &TabletX);
	gpWTInfoA(WTI_DEVICES, DVC_Y, &TabletY);

	lcMine.lcInOrgX = 0;
	lcMine.lcInOrgY = 0;
	lcMine.lcInExtX = TabletX.axMax;
	lcMine.lcInExtY = TabletY.axMax;

	/* output the data in screen coords */
	lcMine.lcOutOrgX = 0;
	lcMine.lcOutOrgY = 0;
	lcMine.lcOutExtX = GetSystemMetrics(SM_CXSCREEN);
	/* move origin to upper left */
	lcMine.lcOutExtY = -GetSystemMetrics(SM_CYSCREEN);

	/* open the region */
	return gpWTOpenA(hWnd, &lcMine, FALSE);
}

///////////////////////////////////////////////////////////////////////////////

void Cleanup(void)
{
	UnloadWintab();
}

///////////////////////////////////////////////////////////////////////////////
