///////////////////////////////////////////////////////////////////////////////
//
//	DESCRIPTION
//		Routines for drawing main display.
//
//	COPYRIGHT
//		Copyright (c) 2014-2020 Wacom Co., Ltd.
//		All rights reserved.
//
///////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "Drawing.h"
#include "Utils.h"
#include <map>

using namespace Gdiplus;
using std::vector;
using std::wstring;

////////////////////////////////////////////////////////////////////////////////
// Structures

struct SExpKey
{
	bool available;
	bool down;
	int  location;
};

struct SMode
{
	bool available;
	bool active;
};

struct SLinearControl
{
	vector<SMode>	modes;
	bool				down;
	int				position;
	int				min;
	int				max;
	int				location;
};

struct STablet
{
	vector<SExpKey>			expKeys;
	vector<SLinearControl>	touchRings;
	vector<SLinearControl>	touchStrips;
};

////////////////////////////////////////////////////////////////////////////////
// Module-global vars

ULONG_PTR							gGdiToken			= NULL;
std::map<int, STablet>			gTablets;
std::unique_ptr<Bitmap>			gBackbuffer;
std::unique_ptr<Pen>				gOutlinePen;
std::unique_ptr<SolidBrush>	gDisabledBrush;
std::unique_ptr<SolidBrush>	gActiveBrush;
std::unique_ptr<SolidBrush>	gInactiveBrush;
std::unique_ptr<SolidBrush>	gBlack;
bool									gDirty				= true;

////////////////////////////////////////////////////////////////////////////////
// Forward declarations for non-public functions

void DrawTablet(Graphics &g_I, STablet &tablet_I, REAL x_I, REAL y_I);

////////////////////////////////////////////////////////////////////////////////
// Public functions
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Clean up GDI+ objects

void Drawing::Cleanup(void)
{
	gOutlinePen.reset();
	gDisabledBrush.reset();
	gActiveBrush.reset();
	gInactiveBrush.reset();
	gBlack.reset();

	gBackbuffer.reset();

	GdiplusShutdown(gGdiToken);
}

////////////////////////////////////////////////////////////////////////////////
// Initialize GDI+. Call this again if the window is resized.

void Drawing::Init(HWND hWnd_I)
{
	if (!gGdiToken)
	{
		GdiplusStartupInput gdisi;
		GdiplusStartup(&gGdiToken, &gdisi, NULL);
		
		gOutlinePen = std::make_unique<Pen>(Color::Black);
		gDisabledBrush = std::make_unique<SolidBrush>(Color::LightGray);
		gActiveBrush = std::make_unique<SolidBrush>(Color::CornflowerBlue);
		gInactiveBrush = std::make_unique<SolidBrush>(Color::White);
		gBlack = std::make_unique<SolidBrush>(Color::Black);	
	}

	RECT r = {0};
	GetClientRect(hWnd_I, &r);
	gBackbuffer.reset(new Bitmap(r.right - r.left + 1, r.bottom - r.top + 1));
	gDirty = true;
}

////////////////////////////////////////////////////////////////////////////////
// Update data for an ExpressKey

void Drawing::UpdateKeys(int tablet_I, int control_I, int, int state_I)
{
	if (gTablets.count(tablet_I))
	{
		if (control_I < static_cast<int>(gTablets[tablet_I].expKeys.size()))
		{
			gTablets[tablet_I].expKeys[control_I].down = (state_I != 0);
			gDirty = true;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////
// Update data a TouchRing

void Drawing::UpdateRing(int tablet_I, int control_I, int function_I, int position_I)
{
	if (gTablets.count(tablet_I))
	{
		if (control_I < static_cast<int>(gTablets[tablet_I].touchRings.size()))
		{
			const int ringModes = static_cast<int>(gTablets[tablet_I].touchRings[control_I].modes.size());
			gTablets[tablet_I].touchRings[control_I].down = (position_I != 0);
			gTablets[tablet_I].touchRings[control_I].position = position_I - 1;

			for (int i = 0; i < ringModes; ++i)
			{
				gTablets[tablet_I].touchRings[control_I].modes[i].active = ((i == function_I) || (ringModes == 1));
			}

			gDirty = true;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////
// Update data for a TouchStrip

void Drawing::UpdateStrip(int tablet_I, int control_I, int function_I, int position_I)
{
	if (gTablets.count(tablet_I))
	{
		if (control_I < static_cast<int>(gTablets[tablet_I].touchStrips.size()))
		{
			const int stripModes = static_cast<int>(gTablets[tablet_I].touchStrips[control_I].modes.size());
			gTablets[tablet_I].touchStrips[control_I].down = (position_I != 0);
			gTablets[tablet_I].touchStrips[control_I].position = position_I;

			for (int i = 0; i < stripModes; ++i)
			{
				gTablets[tablet_I].touchStrips[control_I].modes[i].active = ((i == function_I) || (stripModes == 1));
			}

			gDirty = true;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////
// Perform redraw if necessary, blit to the given DC

void Drawing::PaintToHDC(HDC hdc)
{
	if (gDirty)
	{
		REAL x = 20;
		REAL y = 20;
		Graphics g(gBackbuffer.get());
		g.SetSmoothingMode(SmoothingModeHighQuality);
		g.Clear(Color::White);

		for (const auto& val : gTablets)
		{
			STablet tablet = val.second;
			DrawTablet(g, tablet, x, y);
			y += 100;
		}
		gDirty = false;
	}

	Graphics g(hdc);
	g.DrawImage(gBackbuffer.get(), 0, 0);
}

////////////////////////////////////////////////////////////////////////////////
// Setup for an ExpressKey

void Drawing::SetupKey(int tablet_I,
							  int control_I,
							  int,
							  BOOL availible_I,
							  int location_I,
							  int,
							  int)
{
	if (control_I >= static_cast<int>(gTablets[tablet_I].expKeys.size()))
	{
		gTablets[tablet_I].expKeys.resize(control_I + 1);
	}

	gTablets[tablet_I].expKeys[control_I].available = (availible_I != FALSE);
	gTablets[tablet_I].expKeys[control_I].down = false;
	gTablets[tablet_I].expKeys[control_I].location = location_I;
}

////////////////////////////////////////////////////////////////////////////////
// Setup for a TouchRing

void Drawing::SetupRing(int tablet_I,
								int control_I,
								int function_I,
								BOOL availible_I,
								int location_I,
								int min_I,
								int max_I)
{
	if (control_I >= static_cast<int>(gTablets[tablet_I].touchRings.size()))
	{
		gTablets[tablet_I].touchRings.resize(control_I + 1);
	}

	if (function_I >= static_cast<int>(gTablets[tablet_I].touchRings[control_I].modes.size()))
	{
		gTablets[tablet_I].touchRings[control_I].modes.resize(function_I + 1);
	}

	gTablets[tablet_I].touchRings[control_I].min = min_I;
	gTablets[tablet_I].touchRings[control_I].max = max_I - 1;
	gTablets[tablet_I].touchRings[control_I].location = location_I;
	gTablets[tablet_I].touchRings[control_I].down = false;
	gTablets[tablet_I].touchRings[control_I].position = 0;
	gTablets[tablet_I].touchRings[control_I].modes[function_I].available = (availible_I != FALSE);
	gTablets[tablet_I].touchRings[control_I].modes[function_I].active = false;
}

////////////////////////////////////////////////////////////////////////////////
// Setup for a TouchStrip

void Drawing::SetupStrip(int tablet_I,
								 int control_I,
								 int function_I,
								 BOOL availible_I,
								 int location_I,
								 int min_I,
								 int max_I)
{
	if (control_I >= static_cast<int>(gTablets[tablet_I].touchStrips.size()))
	{
		gTablets[tablet_I].touchStrips.resize(control_I + 1);
	}

	if (function_I >= static_cast<int>(gTablets[tablet_I].touchStrips[control_I].modes.size()))
	{
		gTablets[tablet_I].touchStrips[control_I].modes.resize(function_I + 1);
	}

	gTablets[tablet_I].touchStrips[control_I].min = min_I;
	gTablets[tablet_I].touchStrips[control_I].max = max_I;
	gTablets[tablet_I].touchStrips[control_I].location = location_I;
	gTablets[tablet_I].touchStrips[control_I].down = false;
	gTablets[tablet_I].touchStrips[control_I].position = 0;
	gTablets[tablet_I].touchStrips[control_I].modes[function_I].available = (availible_I != FALSE);
	gTablets[tablet_I].touchStrips[control_I].modes[function_I].active = false;
}

////////////////////////////////////////////////////////////////////////////////
// Module-private functions
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Brush selection logic

Brush *MyBrush(bool available_I, bool active_I)
{
	if (!available_I)
	{
		return gDisabledBrush.get();
	}
	if (active_I)
	{
		return gActiveBrush.get();
	}
	return gInactiveBrush.get();
}

////////////////////////////////////////////////////////////////////////////////
// Translate Wintab location code into a string

wstring LocationLabel(int location_I)
{
	switch (location_I)
	{
		case 0:
		{
			return L"L";		// label for Left buttons
		}
		case 1:
		{
			return L"R";		// label for right buttons
		}
		case 2:
		{
			return L"T";		// label for top buttons
		}
		case 3:
		{
			return L"B";		// label for bottom buttons
		}
		default:
		{
			return L"";
		}
	}
}

////////////////////////////////////////////////////////////////////////////////
// Draw rectangles to represent ExpressKeys

void DrawExpKeys(Graphics &g_I, STablet &tablet_I, REAL x_I, REAL y_I)
{
	const int numButtonSets = 4;
	REAL XOffsets[4] = {0,  0,  0,  0};
	REAL YOffsets[4] = {5, 20, 35, 50};

	// Draw labels
	Font f(FontFamily::GenericSansSerif(), 9);
	StringFormat sf;
	SolidBrush brush(Color::Black);

	for (int i=0; i < numButtonSets; ++i)
	{
		g_I.DrawString(LocationLabel(i).c_str(), -1, &f, PointF(x_I, y_I + YOffsets[i]-3), &brush);
	}
	x_I += 15;

	// Draw button rectangles
	for (const auto &ek : tablet_I.expKeys)
	{
		REAL thisY = y_I + YOffsets[ek.location];
		REAL thisX = x_I + XOffsets[ek.location];
		XOffsets[ek.location] += 15;

		Brush *br = MyBrush(ek.available, ek.down);
		g_I.FillRectangle(br, thisX, thisY, 12.0, 10.0);
		g_I.DrawRectangle(gOutlinePen.get(), thisX, thisY, 12.0, 10.0);
	}
}

////////////////////////////////////////////////////////////////////////////////
// Draw a series of circles to represent modes

void DrawModeBubbles(Graphics &g_I, vector<SMode> &modes_I, REAL x_I, REAL y_I)
{
	REAL modeOffset = 0;

	for (const auto &mode : modes_I)
	{
		Brush *br = MyBrush(mode.available, mode.active);
		g_I.FillEllipse(br, x_I + modeOffset, y_I, 10.0, 10.0);
		g_I.DrawEllipse(gOutlinePen.get(), x_I + modeOffset, y_I, 10.0, 10.0);
		modeOffset += 15;
	}
}

////////////////////////////////////////////////////////////////////////////////
// Draw a circle with needle to represent a TouchRing

void DrawRing(Graphics &g_I, SLinearControl &ring_I, REAL x_I, REAL y_I)
{
	DrawModeBubbles(g_I, ring_I.modes, x_I, y_I);

	g_I.DrawEllipse(gOutlinePen.get(), x_I, y_I + 15, 45.0, 45.0);

	// Draw location indicator
	REAL centerX = static_cast<REAL>(x_I + 22.5);
	REAL centerY = static_cast<REAL>(y_I + 37.5);
	Font f(FontFamily::GenericSansSerif(), 6);
	StringFormat sf;
	sf.SetAlignment(StringAlignmentCenter);
	sf.SetLineAlignment(StringAlignmentCenter);
	g_I.DrawString(LocationLabel(ring_I.location).c_str(), -1, &f, PointF(centerX, centerY), &sf, gBlack.get());

	if (ring_I.down)
	{
		float pos = static_cast<float>(ring_I.position - ring_I.min);
		int range = ring_I.max - ring_I.min;
		REAL angle = static_cast<REAL>(pos/range * M_PI * 2 - (M_PI / 2));

		REAL x1 = static_cast<REAL>(centerX + ( 5.0 * cos(angle)));
		REAL x2 = static_cast<REAL>(centerX + (27.0 * cos(angle)));
		REAL y1 = static_cast<REAL>(centerY + ( 5.0 * sin(angle)));
		REAL y2 = static_cast<REAL>(centerY + (27.0 * sin(angle)));

		Pen p(Color::Black, 3);
		g_I.DrawLine(&p, x1, y1, x2, y2);
	}
}

////////////////////////////////////////////////////////////////////////////////
// Draw a rectangle with needle to represent a TouchStrip

void DrawStrip(Graphics &g_I, SLinearControl &strip_I, REAL x_I, REAL y_I)
{
	DrawModeBubbles(g_I, strip_I.modes, x_I, y_I);

	// Draw location indicator
	Font f(FontFamily::GenericSansSerif(), 6);
	StringFormat sf;
	sf.SetAlignment(StringAlignmentCenter);
	sf.SetLineAlignment(StringAlignmentCenter);
	g_I.DrawString(LocationLabel(strip_I.location).c_str(), -1, &f, PointF(x_I + 45, y_I + 40), &sf, gBlack.get());
	g_I.DrawRectangle(gOutlinePen.get(), x_I, y_I + 30, 90.0, 20.0);

	if (strip_I.down)
	{
		Pen p(Color::Black, 3);
		float pos = static_cast<float>(strip_I.position - strip_I.min);
		int range = strip_I.max - strip_I.min;
		REAL offset = static_cast<REAL>(pos/range * 90);
		g_I.DrawLine(&p, x_I + offset, y_I + 25, x_I + offset, y_I + 55);
	}
}

////////////////////////////////////////////////////////////////////////////////
// Draw all controls for a single tablet

void DrawTablet(Graphics &g_I, STablet &tablet_I, REAL x_I, REAL y_I)
{
	DrawExpKeys(g_I, tablet_I, x_I, y_I);
	x_I += 140;

	for (auto &tr : tablet_I.touchRings)
	{
		DrawRing(g_I, tr, x_I, y_I);
		x_I += 100;
	}

	for (auto &ts : tablet_I.touchStrips)
	{
		DrawStrip(g_I, ts, x_I, y_I);
		x_I += 100;
	}
}
