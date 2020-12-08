///////////////////////////////////////////////////////////////////////////////
//
//	DESCRIPTION
//		Declarations of routines for drawing main display.
//
//	COPYRIGHT
//		Copyright (c) 2014-2020 Wacom Co., Ltd.
//		All rights reserved.
//
///////////////////////////////////////////////////////////////////////////////

#pragma once

namespace Drawing
{
	using SetupControlPtr = void (*) (int, int, int, BOOL, int, int, int);

	void Init(HWND hWnd_I);
	void Cleanup();

	void SetupKey(int tablet_I, int control_I, int function_I, BOOL availible_I,
		int location_I, int min_I, int max_I);

	void SetupRing(int tablet_I, int control_I, int function_I, BOOL availible_I,
		int location_I, int min_I, int max_I);

	void SetupStrip(int tablet_I, int control_I, int function_I, BOOL availible_I,
		int location_I, int min_I, int max_I);

	void UpdateKeys(int tablet_I, int control_I, int location_I, int state_I);
	void UpdateRing(int tablet_I, int control_I, int function_I, int position_I);
	void UpdateStrip(int tablet_I, int control_I, int function_I, int position_I);

	void PaintToHDC(HDC hdc_I);
}
