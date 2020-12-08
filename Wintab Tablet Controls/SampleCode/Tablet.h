///////////////////////////////////////////////////////////////////////////////
//
//	DESCRIPTION
//		Declarations for tablet-related functions.
//
//	COPYRIGHT
//		Copyright (c) 2014-2020 Wacom Co., Ltd.
//		All rights reserved.
//
///////////////////////////////////////////////////////////////////////////////

#pragma once

// WINTAB headers
#include <wintab.h>

constexpr WTPKT PACKETMODE = 0;

// Tablet control extension defines
#define PACKETEXPKEYS		PKEXT_ABSOLUTE
#define PACKETTOUCHSTRIP	PKEXT_ABSOLUTE
#define PACKETTOUCHRING		PKEXT_ABSOLUTE
#include <pktdef.h>

///////////////////////////////////////////////////////////////////////////////

namespace Tablet
{
	bool Init(HWND hWnd_I);
	void Cleanup(void);
}
