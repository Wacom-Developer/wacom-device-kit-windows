///////////////////////////////////////////////////////////////////////////////
//
//	DESCRIPTION
//		Win32 boilerplate and main entry point.
//
//	COPYRIGHT
//		Copyright (c) 2014-2020 Wacom Co., Ltd.
//		All rights reserved.
//
///////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "resource.h"
#include <map>

#include "Drawing.h"
#include "Tablet.h"
#include "Utils.h"
#include "TabletControlsSample.h"

constexpr int MAX_LOADSTRING = 100;

extern DWORD gNumCursorsPerTablet;
extern std::map<int, bool> gAttachMap;

////////////////////////////////////////////////////////////////////////////////
// Global Variables:

HINSTANCE	ghInst;									// current instance
TCHAR			gszTitle[MAX_LOADSTRING];			// The title bar text
TCHAR			gszWindowClass[MAX_LOADSTRING];	// the main window class name

////////////////////////////////////////////////////////////////////////////////
// Forward declarations

ATOM					MyRegisterClass(HINSTANCE hInstance);
BOOL					InitInstance(HINSTANCE, int);
LRESULT CALLBACK	WndProc(HWND, UINT, WPARAM, LPARAM);

////////////////////////////////////////////////////////////////////////////////
// Main application entry point.

int APIENTRY _tWinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE, _In_ LPTSTR, _In_ int nCmdShow)
{
	MSG msg = {0};

	// Initialize global strings
	LoadString(hInstance, IDS_APP_TITLE, gszTitle, MAX_LOADSTRING);
	LoadString(hInstance, IDC_TABLETCONTROLSSAMPLE, gszWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);

	// Perform application initialization:
	if (!InitInstance(hInstance, nCmdShow))
	{
		return FALSE;
	}

	// Main message loop:
	while (GetMessage(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return static_cast<int>(msg.wParam);
}

////////////////////////////////////////////////////////////////////////////////
// Wrapper to register window class.

ATOM MyRegisterClass(HINSTANCE hInstance)
{
	WNDCLASSEX wcex = {0};

	wcex.cbSize = sizeof(WNDCLASSEX);

	wcex.style				= CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc		= WndProc;
	wcex.hInstance			= hInstance;
	wcex.hIcon				= LoadIcon(hInstance, MAKEINTRESOURCE(IDI_TABLETCONTROLSSAMPLE));
	wcex.hCursor			= LoadCursor(NULL, IDC_ARROW);
	wcex.hbrBackground	= (HBRUSH)(COLOR_WINDOW+1);
	wcex.lpszClassName	= gszWindowClass;
	wcex.hIconSm			= LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

	return RegisterClassEx(&wcex);
}

////////////////////////////////////////////////////////////////////////////////
// Initialize program state.

BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
	ghInst = hInstance; // Store instance handle in our global variable

	HWND hWnd = CreateWindow(gszWindowClass, gszTitle, WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT, 0, 500, 400, NULL, NULL, hInstance, NULL);
	if (!hWnd)
	{
		return FALSE;
	}

	ShowWindow(hWnd, nCmdShow);
	UpdateWindow(hWnd);

	return TRUE;
}

////////////////////////////////////////////////////////////////////////////////
// Message handler for main window.

LRESULT CALLBACK WndProc(HWND hWnd_I, UINT message_I, WPARAM wParam_I, LPARAM lParam_I)
{
	switch (message_I)
	{
		case WM_CREATE:
		{
			Tablet::Init(hWnd_I);
			break;
		}
		case WM_SIZE:
		{
			Drawing::Init(hWnd_I);
			break;
		}
		case WM_PAINT:
		{
			PAINTSTRUCT ps = {0};
			const HDC hdc = BeginPaint(hWnd_I, &ps);
			Drawing::PaintToHDC(hdc);
			EndPaint(hWnd_I, &ps);
			break;
		}
		case WT_PACKET:
		{
			// handle pen input here if desired
			break;
		}
		case WT_PACKETEXT:
		{
			PACKETEXT pkt = {0};
			if (gpWTPacket((HCTX)lParam_I, static_cast<UINT>(wParam_I), &pkt))
			{
				// Update display.
				Drawing::UpdateKeys(pkt.pkExpKeys.nTablet, pkt.pkExpKeys.nControl,
					pkt.pkExpKeys.nLocation, pkt.pkExpKeys.nState);
				Drawing::UpdateRing(pkt.pkTouchRing.nTablet, pkt.pkTouchRing.nControl,
					pkt.pkTouchRing.nMode, pkt.pkTouchRing.nPosition);
				Drawing::UpdateStrip(pkt.pkTouchStrip.nTablet, pkt.pkTouchStrip.nControl,
					pkt.pkTouchStrip.nMode, pkt.pkTouchStrip.nPosition);
				InvalidateRect(hWnd_I, NULL, TRUE);
			}
			break;
		}
		case WM_DESTROY:
		{
			Drawing::Cleanup();
			Tablet::Cleanup();
			PostQuitMessage(0);
			break;
		}
		default:
		{
			return DefWindowProc(hWnd_I, message_I, wParam_I, lParam_I);
		}
	}
	return 0;
}
