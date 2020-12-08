///////////////////////////////////////////////////////////////////////////////
//
//	DESCRIPTION
//		Defines for the general-purpose functions for the WinTab demos.
//
//	COPYRIGHT
//		Copyright (c) 2014-2020 Wacom Co., Ltd.
//		All rights reserved.
//
///////////////////////////////////////////////////////////////////////////////

#pragma once

#include <windows.h>
#include <stdio.h>
#include <assert.h>
#include <stdarg.h>

#include <wintab.h>		// NOTE: get from wactab header package

//////////////////////////////////////////////////////////////////////////////
// Ignore warnings about using unsafe string functions.
#pragma warning( disable : 4996 )

//////////////////////////////////////////////////////////////////////////////
// Function pointers to Wintab functions exported from wintab32.dll.
using WTINFOA           = UINT (API*)(UINT, UINT, LPVOID);
using WTOPENA           = HCTX (API*)(HWND, LPLOGCONTEXTA, BOOL);
using WTGETA            = BOOL (API*)(HCTX, LPLOGCONTEXT);
using WTSETA            = BOOL (API*)(HCTX, LPLOGCONTEXT);
using WTCLOSE           = BOOL (API*)(HCTX);
using WTENABLE          = BOOL (API*)(HCTX, BOOL);
using WTPACKET          = BOOL (API*)(HCTX, UINT, LPVOID);
using WTOVERLAP         = BOOL (API*)(HCTX, BOOL);
using WTSAVE            = BOOL (API*)(HCTX, LPVOID);
using WTCONFIG          = BOOL (API*)(HCTX, HWND);
using WTRESTORE         = HCTX (API*)(HWND, LPVOID, BOOL);
using WTEXTSET          = BOOL (API*)(HCTX, UINT, LPVOID);
using WTEXTGET          = BOOL (API*)(HCTX, UINT, LPVOID);
using WTQUEUESIZESET    = BOOL (API*)(HCTX, int);
using WTDATAPEEK        = int  (API*)(HCTX, UINT, UINT, int, LPVOID, LPINT);
using WTPACKETSGET      = int  (API*)(HCTX, int, LPVOID);
//using WTMGROPEN         = HMGR (API*)(HWND, UINT);
//using WTMGRCLOSE        = BOOL (API*)(HMGR);
//using WTMGRDEFCONTEXT   = HCTX (API*)(HMGR, BOOL);
//using WTMGRDEFCONTEXTEX = HCTX (API*)(HMGR, UINT, BOOL);

// TODO - add more wintab32 function defs as needed

//////////////////////////////////////////////////////////////////////////////

// Loaded Wintab32 API functions.
extern HINSTANCE         ghWintab;

extern WTINFOA           gpWTInfoA;
extern WTOPENA           gpWTOpenA;
extern WTGETA            gpWTGetA;
extern WTSETA            gpWTSetA;
extern WTCLOSE           gpWTClose;
extern WTPACKET          gpWTPacket;
extern WTENABLE          gpWTEnable;
extern WTOVERLAP         gpWTOverlap;
extern WTSAVE            gpWTSave;
extern WTCONFIG          gpWTConfig;
extern WTRESTORE         gpWTRestore;
extern WTEXTSET          gpWTExtSet;
extern WTEXTGET          gpWTExtGet;
extern WTQUEUESIZESET    gpWTQueueSizeSet;
extern WTDATAPEEK        gpWTDataPeek;
extern WTPACKETSGET      gpWTPacketsGet;
//extern WTMGROPEN         gpWTMgrOpen;
//extern WTMGRCLOSE        gpWTMgrClose;
//extern WTMGRDEFCONTEXT   gpWTMgrDefContext;
//extern WTMGRDEFCONTEXTEX gpWTMgrDefContextEx;

// TODO - add more wintab32 function pointers as needed

//////////////////////////////////////////////////////////////////////////////

BOOL LoadWintab(void);
void UnloadWintab(void);

void ShowError(const std::string &pszErrorMessage_I);

//////////////////////////////////////////////////////////////////////////////

