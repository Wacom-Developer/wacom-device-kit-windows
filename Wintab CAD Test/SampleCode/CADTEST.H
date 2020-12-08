/*----------------------------------------------------------------------------

	NAME
		Cadtest.h

	PURPOSE
		Defines for cadtest demo.

	COPYRIGHT
		Copyright (C) 1998  LCS/Telegraphics
		Copyright (c) Wacom Company, Ltd. 2020 All Rights Reserved
		All rights reserved.

		The text and information contained in this file may be freely used,
		copied, or distributed without compensation or licensing restrictions.

---------------------------------------------------------------------------- */
#pragma once

#include "wintab.h"
#define PACKETDATA	(PK_X | PK_Y | PK_BUTTONS)
#define PACKETMODE	0
#include "pktdef.h"

//////////////////////////////////////////////////////////////////////////////

#define IDM_FILE	0
#define IDM_EDIT	1
#define IDM_HELP	2

#define IDM_ABOUT			100
#define IDM_CONFIG			104
#define IDM_PERSIST			105
#define IDM_RULER_DEMO		106

int __stdcall WinMain(HINSTANCE, HINSTANCE, LPSTR, int);
BOOL InitApplication(HINSTANCE);
BOOL InitInstance(HINSTANCE, int);
LRESULT FAR PASCAL MainWndProc(HWND, unsigned, WPARAM, LPARAM);
BOOL AboutProc(HWND, unsigned, WPARAM, LPARAM);
void Cleanup( void );
