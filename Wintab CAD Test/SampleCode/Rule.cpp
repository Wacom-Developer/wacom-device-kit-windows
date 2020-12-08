/*----------------------------------------------------------------------------

	NAME
		Rule.cpp

	PURPOSE
		Ruler measuring widget.

	COPYRIGHT
		This file is Copyright (c) Wacom Company, Ltd. 2020 All Rights Reserved
		with portions copyright 1998 by LCS/Telegraphics.

		The text and information contained in this file may be freely used,
		copied, or distributed without compensation or licensing restrictions.

---------------------------------------------------------------------------- */
#include <windows.h>
#include <map>
#include <cmath>
#include <stdlib.h>
#include "Utils.h"
#include "MsgPack.h"
#include "CadTest.h"
#include "Rule.h"

#define Inch2Cm	CASTFIX32(2.54)
#define Cm2Inch	CASTFIX32(1.0/2.54)

namespace Ruler
{
	int gnOpenContexts = 0;
	int gnAttachedDevices = 0;

	///////////////////////////////////////////////////////////////////////////////
	// Hold tablet-specific properties used when responding to tablet data packets.
	//
	struct RulerTabletInfo
	{
		LONG		tabletXExt;
		LONG		tabletYExt;
		LONG		physSizeX;
		LONG		physSizeY;
		FIX32		scale[2];
		bool		displayTablet;
	};

	///////////////////////////////////////////////////////////////////////////////
	// Cache all opened contexts for attached tablets.  
	// This will allow us to close them when the window closes down.
	//
	typedef std::map<HCTX, RulerTabletInfo> CtxMap;
	CtxMap g_RulerContextMap = CtxMap();

	/* local functions */
	bool TabletRuleInit(HWND hWnd);
	void TabletRuleScaling(FIX32 scale[], int ctxIndex);
	void CloseTabletContexts(void);

	/* -------------------------------------------------------------------------- */
	BOOL CALLBACK RuleDemoProc(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
	{
		static int inMode = ID_CLICK;
		static LONG x1 = 0, x2 = 0, y1 = 0, y2 = 0;
		static HCTX hctx = NULL;

		PAINTSTRUCT psPaint;
		HDC hDC;

		// WacomTrace("Incoming Message: 0x%X\n", message);

		switch (message)
		{
		case WM_INITDIALOG:
		{
			inMode = ID_CLICK;
			x1 = x2 = y1 = y2 = 0;
			TabletRuleInit(hDlg);
			return TRUE;
		}

		case WM_CLOSE:
		{
			CloseTabletContexts();
			EndDialog(hDlg, TRUE);
			return TRUE;
		}

		case WM_LBUTTONDOWN:
		{
			if (hctx)
			{
				PACKET pkt;
				BOOL bSawFirstButtonPkt = FALSE;

				inMode = ID_PRESS;
				x1 = x2 = y1 = y2 = 0;
				InvalidateRect(hDlg, NULL, TRUE);
				UpdateWindow(hDlg);

				while (inMode != ID_CLICK)
				{
					// poll
					if (!gpWTPacketsGet(hctx, 1, &pkt))
					{
						continue;
					}

					// Sometimes, the first packet comes through with no button data.
					// If so, just ignore it and wait for the next packet.
					if (!bSawFirstButtonPkt)
					{
						if (!pkt.pkButtons)
						{
							//WACOM_TRACE("Disregarding packet w/no button data...\n");
							continue;	// disregard this packet
						}

						bSawFirstButtonPkt = TRUE;
					}

					//WACOM_TRACE("pkt: [%i,%i], pkt.pkButtons: %i\n", pkt.pkX, pkt.pkY, pkt.pkButtons);

					// handle it
					if (inMode == ID_PRESS && pkt.pkButtons)
					{
						//WACOM_TRACE("PRESSING: pkt: [%i,%i]\n", pkt.pkX, pkt.pkY);
						x1 = pkt.pkX;
						y1 = pkt.pkY;

						// Scale values here from tablet coordinates to physical coordinates
						x1 = (LONG)((double)((double)x1 / g_RulerContextMap[hctx].tabletXExt) * g_RulerContextMap[hctx].physSizeX);
						y1 = (LONG)((double)((double)y1 / g_RulerContextMap[hctx].tabletYExt) * g_RulerContextMap[hctx].physSizeY);

						inMode = ID_RELEASE;
						InvalidateRect(hDlg, NULL, TRUE);
						UpdateWindow(hDlg);
					}
					if (inMode == ID_RELEASE && !pkt.pkButtons)
					{
						//WACOM_TRACE("RELEASED: pkt: [%i,%i]\n", pkt.pkX, pkt.pkY);
						x2 = pkt.pkX;
						y2 = pkt.pkY;

						x2 = (LONG)((double)((double)x2 / g_RulerContextMap[hctx].tabletXExt) * g_RulerContextMap[hctx].physSizeX);
						y2 = (LONG)((double)((double)y2 / g_RulerContextMap[hctx].tabletYExt) * g_RulerContextMap[hctx].physSizeY);

						inMode = ID_CLICK;
						InvalidateRect(hDlg, NULL, TRUE);
						UpdateWindow(hDlg);
					}
				}
			}
			break;
		}

		case WM_PAINT:
		{
			hDC = BeginPaint(hDlg, &psPaint);
			ShowWindow(GetDlgItem(hDlg, ID_CLICK), inMode == ID_CLICK);
			ShowWindow(GetDlgItem(hDlg, ID_PRESS), inMode == ID_PRESS);
			ShowWindow(GetDlgItem(hDlg, ID_RELEASE), inMode == ID_RELEASE);
			if (inMode == ID_CLICK || inMode == ID_PRESS)
			{
				// [0] - x-axis start/end distance
				// [1] - y-axis start/end distance
				// [2] - straight line start/end distance
				LONG delta[3];

				delta[0] = std::abs(x2 - x1);
				delta[1] = std::abs(y2 - y1);
				delta[2] = static_cast<LONG>(std::sqrt(delta[0] * delta[0] + delta[1] * delta[1]));

				for (int i = 0; i < 3; i++) // direction 
				{
					char buf[30];

					// print result in cm
					wsprintf(buf, "%d.%3.3d", (UINT)delta[i] / 1000, (UINT)delta[i] % 1000);

					SetWindowText(GetDlgItem(hDlg, ID_HC + i), buf);

					// convert to inches
					delta[i] = INT(delta[i] * Cm2Inch);

					// print result in inches
					wsprintf(buf, "%d.%3.3d", (UINT)delta[i] / 1000, (UINT)delta[i] % 1000);

					SetWindowText(GetDlgItem(hDlg, ID_HI + i), buf);
				}
			}

			EndPaint(hDlg, &psPaint);
			break;
		}


		case WT_PACKET:
		{
			hctx = (HCTX)lParam;
			break;
		}
		}

		return FALSE;
	}


	/* -------------------------------------------------------------------------- */
	bool TabletRuleInit(HWND hWnd)
	{
		int ctxIndex = 0;
		gnOpenContexts = 0;
		gnAttachedDevices = 0;
		g_RulerContextMap.clear();

		gnAttachedDevices = 0;
		gpWTInfoA(WTI_INTERFACE, IFC_NDEVICES, &gnAttachedDevices);
		WacomTrace("Number of attached devices: %i\n", gnAttachedDevices);
		do
		{
			LOGCONTEXT lcMine;
			int foundCtx = gpWTInfoA(WTI_DDCTXS + ctxIndex, 0, &lcMine);

			if (foundCtx > 0)
			{
				UINT result = 0;
				UINT wWTInfoRetVal = gpWTInfoA(WTI_DEVICES + ctxIndex, DVC_HARDWARE, &result);
				bool displayTablet = result & HWC_INTEGRATED;

				// modify the digitizing region
				strcpy(lcMine.lcName, "Rule Digitizing");
				lcMine.lcOptions |= CXO_MESSAGES;
				lcMine.lcMsgBase = WT_DEFBASE;
				lcMine.lcPktData = PACKETDATA;
				lcMine.lcPktMode = PACKETMODE;
				lcMine.lcMoveMask = PACKETDATA;
				lcMine.lcBtnUpMask = lcMine.lcBtnDnMask;

				lcMine.lcOutOrgX = lcMine.lcOutOrgY = 0;

				// Set the entire tablet as active
				AXIS tabletX = { 0 };
				AXIS tabletY = { 0 };

				gpWTInfoA(WTI_DEVICES + ctxIndex, DVC_X, &tabletX);
				gpWTInfoA(WTI_DEVICES + ctxIndex, DVC_Y, &tabletY);

				// This prevents outputted display-tablet coordinates
				// range from being mapped to full desktop, which
				// causes problems in multi-screen set-ups. Ie, without this
				// then the tablet coord. range is mapped to full desktop, intead
				// of only the display tablet active area.
				{
					lcMine.lcOutExtX = tabletX.axMax - tabletX.axMin + 2;
					lcMine.lcOutExtY = tabletY.axMax - tabletY.axMin + 1;
				}

				LONG xTbltExt = lcMine.lcOutExtX;
				LONG yTbltExt = lcMine.lcOutExtY;


				FIX32 scale[2] = { 0, 0 };
				TabletRuleScaling(scale, ctxIndex);
				// Physical size of active tablet area where 1,000 units = 1cm
				LONG physSizeX = INT(scale[0] * lcMine.lcInExtX);
				LONG physSizeY = INT(scale[1] * lcMine.lcInExtY);

				// open the region
				HCTX hCtx = gpWTOpenA(hWnd, &lcMine, TRUE);
				if (hCtx)
				{
					RulerTabletInfo info = { };
					memcpy(info.scale, scale, sizeof(info.scale));
					info.tabletXExt = xTbltExt;
					info.tabletYExt = yTbltExt;
					info.physSizeX = physSizeX;
					info.physSizeY = physSizeY;
					info.displayTablet = displayTablet;
					g_RulerContextMap[hCtx] = info;
					WacomTrace("Opened context: 0x%X for ctxIndex: %i\n", hCtx, ctxIndex);
					gnOpenContexts++;
				}
				else
				{
					MessageBox(NULL, " Could Not Open Tablet Context.", gpszProgramName,
						MB_OK | MB_ICONHAND);

					SendMessage(hWnd, WM_DESTROY, 0, 0L);
				}
			}
			else
			{
				break;
			}
			ctxIndex++;
		} while (true);

		return (gnAttachedDevices > 0);
	}


	// --------------------------------------------------------------------------
	// return scaling factors in thousandths of cm per axis unit
	void TabletRuleScaling(FIX32 scale[], int ctxIndex)
	{
		AXIS aXY[2];
		int i;

		gpWTInfoA(WTI_DEVICES + ctxIndex, DVC_X, &aXY[0]);
		gpWTInfoA(WTI_DEVICES + ctxIndex, DVC_Y, &aXY[1]);

		/* calculate the scaling factors */
		for (i = 0; i < 2; i++) {
			FIX_DIV(scale[i], CASTFIX32(1000), aXY[i].axResolution);
			if (aXY[i].axUnits == TU_INCHES) {
				FIX_MUL(scale[i], scale[i], Inch2Cm);
			}
		}
	}

	// --------------------------------------------------------------------------
	// Close all opened tablet contexts
	void CloseTabletContexts(void)
	{
		// Close all contexts we opened so we don't have them lying around in prefs.
		for (CtxMap::iterator it = g_RulerContextMap.begin(); it != g_RulerContextMap.end(); ++it)
		{
			HCTX hCtx = it->first;
			WacomTrace("Closing context: 0x%X\n", hCtx);
			if (!gpWTClose(hCtx))
			{
				WacomTrace("Could not close context: 0x%X\n", hCtx);
			}
			else
			{
				hCtx = NULL;
			}
		}
		g_RulerContextMap.clear();
	}
}