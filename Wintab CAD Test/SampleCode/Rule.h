/*----------------------------------------------------------------------------

	NAME
		Rule.h

	PURPOSE
		Defines for pen button handling demo.

	COPYRIGHT
		This file is Copyright (c) Wacom Company, Ltd. 2020 All Rights Reserved
		with portions copyright 1998 by LCS/Telegraphics.

		The text and information contained in this file may be freely used,
		copied, or distributed without compensation or licensing restrictions.

---------------------------------------------------------------------------- */
#pragma once

namespace Ruler
{
	BOOL CALLBACK RuleDemoProc(HWND, UINT, WPARAM, LPARAM);
}

#define ID_CLICK			201
#define ID_PRESS			202
#define ID_RELASE			203
#define ID_HI				204
#define ID_VI				205
#define ID_DI				206
#define ID_HC				207
#define ID_VC				208
#define ID_DC				209
#define ID_RELEASE			210
