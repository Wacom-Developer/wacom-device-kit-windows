///////////////////////////////////////////////////////////////////////////////
//
//	DESCRIPTION
//		Some general-purpose functions for the WinTab demos.
//
//	COPYRIGHT
//		Copyright (c) 2014-2020 Wacom Co., Ltd.
//		All rights reserved.
//
///////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "Utils.h"

//////////////////////////////////////////////////////////////////////////////

HINSTANCE ghWintab = NULL;

WTINFOA gpWTInfoA = NULL;
WTOPENA gpWTOpenA = NULL;
WTGETA gpWTGetA = NULL;
WTSETA gpWTSetA = NULL;
WTCLOSE gpWTClose = NULL;
WTPACKET gpWTPacket = NULL;
WTENABLE gpWTEnable = NULL;
WTOVERLAP gpWTOverlap = NULL;
WTSAVE gpWTSave = NULL;
WTCONFIG gpWTConfig = NULL;
WTRESTORE gpWTRestore = NULL;
WTEXTSET gpWTExtSet = NULL;
WTEXTGET gpWTExtGet = NULL;
WTQUEUESIZESET gpWTQueueSizeSet = NULL;
WTDATAPEEK gpWTDataPeek = NULL;
WTPACKETSGET gpWTPacketsGet = NULL;
WTMGROPEN gpWTMgrOpen = NULL;
WTMGRCLOSE gpWTMgrClose = NULL;
WTMGRDEFCONTEXT gpWTMgrDefContext = NULL;
WTMGRDEFCONTEXTEX gpWTMgrDefContextEx = NULL;

// TODO - add more wintab32 function pointers as needed

//////////////////////////////////////////////////////////////////////////////
// Purpose
//		Find wintab32.dll and load it.  
//		Find the exported functions we need from it.
//
//	Returns
//		TRUE on success.
//		FALSE on failure.
//
BOOL LoadWintab(void)
{
	// load the wintab32 dll
	ghWintab = LoadLibraryA("Wintab32.dll");
	
	if ( !ghWintab )
	{
		const DWORD err = GetLastError();
		ShowError("Could not load Wintab32.dll: " + std::to_string(err));
		return FALSE;
	}

	// Explicitly find the exported Wintab functions in which we are interested.
	// We are using the ASCII, not unicode versions (where applicable).
	gpWTOpenA = (WTOPENA)GetProcAddress(ghWintab, "WTOpenA");
	gpWTInfoA = (WTINFOA)GetProcAddress(ghWintab, "WTInfoA");
	gpWTGetA = (WTGETA)GetProcAddress(ghWintab, "WTGetA");
	gpWTSetA = (WTSETA)GetProcAddress(ghWintab, "WTSetA");
	gpWTPacket = (WTPACKET)GetProcAddress(ghWintab, "WTPacket");
	gpWTClose = (WTCLOSE)GetProcAddress(ghWintab, "WTClose");
	gpWTEnable = (WTENABLE)GetProcAddress(ghWintab, "WTEnable");
	gpWTOverlap = (WTOVERLAP)GetProcAddress(ghWintab, "WTOverlap");
	gpWTSave = (WTSAVE)GetProcAddress(ghWintab, "WTSave");
	gpWTConfig = (WTCONFIG)GetProcAddress(ghWintab, "WTConfig");
	gpWTRestore = (WTRESTORE)GetProcAddress(ghWintab, "WTRestore");
	gpWTExtSet = (WTEXTSET)GetProcAddress(ghWintab, "WTExtSet");
	gpWTExtGet = (WTEXTGET)GetProcAddress(ghWintab, "WTExtGet");
	gpWTQueueSizeSet = (WTQUEUESIZESET)GetProcAddress(ghWintab, "WTQueueSizeSet");
	gpWTDataPeek = (WTDATAPEEK)GetProcAddress(ghWintab, "WTDataPeek");
	gpWTPacketsGet = (WTPACKETSGET)GetProcAddress(ghWintab, "WTPacketsGet");
	gpWTMgrOpen = (WTMGROPEN)GetProcAddress(ghWintab, "WTMgrOpen");
	gpWTMgrClose = (WTMGRCLOSE)GetProcAddress(ghWintab, "WTMgrClose");
	gpWTMgrDefContext = (WTMGRDEFCONTEXT)GetProcAddress(ghWintab, "WTMgrDefContext");
	gpWTMgrDefContextEx = (WTMGRDEFCONTEXTEX)GetProcAddress(ghWintab, "WTMgrDefContextEx");

	// TODO - don't forget to NULL out pointers in UnloadWintab().
	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
// Purpose
//		Uninitializes use of wintab32.dll
//
// Returns
//		Nothing.
//
void UnloadWintab(void)
{
	if (ghWintab)
	{
		FreeLibrary(ghWintab);
		ghWintab = NULL;
	}

	gpWTOpenA			= NULL;
	gpWTClose			= NULL;
	gpWTInfoA			= NULL;
	gpWTPacket			= NULL;
	gpWTEnable			= NULL;
	gpWTOverlap			= NULL;
	gpWTSave				= NULL;
	gpWTConfig			= NULL;
	gpWTGetA				= NULL;
	gpWTSetA				= NULL;
	gpWTRestore			= NULL;
	gpWTExtSet			= NULL;
	gpWTExtGet			= NULL;
	gpWTQueueSizeSet	= NULL;
	gpWTDataPeek		= NULL;
	gpWTPacketsGet		= NULL;
	gpWTMgrOpen			= NULL;
	gpWTMgrClose		= NULL;
	gpWTMgrDefContext = NULL;
	gpWTMgrDefContextEx = NULL;
}

//////////////////////////////////////////////////////////////////////////////
// Purpose
//		Display error to user.
//
void ShowError(const std::string &pszErrorMessage_I)
{
	MessageBoxA(NULL, pszErrorMessage_I.c_str(), "Error", MB_OK | MB_ICONERROR);
}
