/*----------------------------------------------------------------------------s
	NAME
		ScribbleDemo.h

	PURPOSE
		This demo shows how to use Wintab to detect/display pen pressure input
		for multiple tablets.

		Can be built for either Wintab system or digitizer contexts.

	COPYRIGHT
		This file is Copyright (c) Wacom Company, Ltd. 2024 All Rights Reserved
		with portions copyright 1991-1998 by LCS/Telegraphics.

		The text and information contained in this file may be freely used,
		copied, or distributed without compensation or licensing restrictions.S
---------------------------------------------------------------------------- */

#define IDM_ABOUT          100
#define IDM_CLEAR          101
#define IDM_LINES          201
#define IDM_PRESSURE       202
#define IDM_OFFSETMODE     203

#define IDD_ABOUTBOX							110

int PASCAL WinMain(_In_ HINSTANCE, _In_opt_ HINSTANCE, _In_ LPSTR, _In_ int);
bool InitApplication(HINSTANCE);
bool InitInstance(HINSTANCE, int);
LRESULT FAR PASCAL MainWndProc(HWND, unsigned, WPARAM, LPARAM);
INT_PTR CALLBACK	About(HWND, UINT, WPARAM, LPARAM);
void Cleanup( void );
