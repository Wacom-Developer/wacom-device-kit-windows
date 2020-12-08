///////////////////////////////////////////////////////////////////////////////
//
//	PURPOSE
//		Some general-purpose functions for accessing Wintab.
//
//	COPYRIGHT
//		Copyright (c) 2012-2020 Wacom Co., Ltd.
//
//		The text and information contained in this file may be freely used,
//		copied, or distributed without compensation or licensing restrictions.
//
///////////////////////////////////////////////////////////////////////////////

#include "WintabUtils.h"

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

// TODO - Add more wintab32 function pointers as needed

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
	ghWintab = LoadLibraryA( "Wintab32.dll" );
	if (!ghWintab)
	{
		DWORD err = GetLastError();
		ShowError("Could not load Wintab32.dll");
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

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
// Purpose
//		Uninitializes use of wintab32.dll
//
void UnloadWintab(void)
{
	if (ghWintab)
	{
		FreeLibrary(ghWintab);
		ghWintab = NULL;
	}

	// NULL out pointers
	gpWTInfoA				= NULL;
	gpWTClose				= NULL;
	gpWTOpenA				= NULL;
	gpWTPacket				= NULL;
	gpWTEnable				= NULL;
	gpWTOverlap				= NULL;
	gpWTSave					= NULL;
	gpWTConfig				= NULL;
	gpWTGetA					= NULL;
	gpWTSetA					= NULL;
	gpWTRestore				= NULL;
	gpWTExtSet				= NULL;
	gpWTExtGet				= NULL;
	gpWTQueueSizeSet		= NULL;
	gpWTDataPeek			= NULL;
	gpWTPacketsGet			= NULL;
	gpWTMgrOpen				= NULL;
	gpWTMgrClose			= NULL;
	gpWTMgrDefContext		= NULL;
	gpWTMgrDefContextEx	= NULL;
}

//////////////////////////////////////////////////////////////////////////////
// Purpose
//		Display error to user.
//
void ShowError(const char *pszErrorMessage)
{
	MessageBoxA(NULL, pszErrorMessage, "Scribble", MB_OK | MB_ICONHAND);
}

//////////////////////////////////////////////////////////////////////////////
// Purpose
//		Display information in the debugger.
//
void DebugTrace(const char* lpszFormat, ...)
{
	char szTraceMessage[1024] = { 0 };

	va_list args = NULL;
	va_start(args, lpszFormat);

	int nBytesWritten = _vsnprintf_s(szTraceMessage, sizeof(szTraceMessage) - 1, lpszFormat, args);
	if (nBytesWritten > 0)
	{
		OutputDebugStringA(szTraceMessage);
	}

	va_end(args);
}
