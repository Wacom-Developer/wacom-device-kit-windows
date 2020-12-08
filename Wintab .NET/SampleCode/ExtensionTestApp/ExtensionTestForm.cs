///////////////////////////////////////////////////////////////////////////////
//
//	PURPOSE
//		Extensions test dialog for WintabDN
//
//	COPYRIGHT
//		Copyright (c) 2019-2020 Wacom Co., Ltd.
//
//		The text and information contained in this file may be freely used,
//		copied, or distributed without compensation or licensing restrictions.
//
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WintabDN;

using WTPKT = System.UInt32;

namespace FormExtTestApp
{
	/// <summary>
	/// Exercise the CWintabExtensions API.
	/// NOTE: This demo only supports handling a context for one tablet.
	/// </summary>
	public partial class ExtensionTestForm : Form
	{
		private UInt32 mExpKeysMask = 0;
		private UInt32 mTouchRingMask = 0;
		private UInt32 mTouchStripMask = 0;

		private ExtensionControlState mExtensionControlState = null;

		private CWintabContext mLogContext = null;

		private CWintabData m_wtData = null;

		// Constants used for touchring operation.
		private const float mPI = (float)3.14159265358979323846;
		private const float mPIDIV2 = (float)1.57079632679489661923;

		private String mTestImageIconPath = "";

		private const String mTestImageIDEIconPath = "..\\..\\sample.png";
		private const String mTestImageDefaultIconPath = ".\\sample.png";

		/// <summary>
		/// Tablet control rendering graphics objects.
		/// </summary>
		private Graphics grfxEKP = null; // for ExpressKeys panel
		private Graphics grfxTCP = null; // for touch control panel
		private Font ctrlFont = new Font("Helvetica", 16, FontStyle.Regular);
		private Brush ctrlActiveBrush = new SolidBrush(System.Drawing.Color.Orange);
		private Brush ctrlNotPressedBrush = new SolidBrush(System.Drawing.Color.DarkBlue);
		private Brush ctrlTextBrush = new SolidBrush(System.Drawing.Color.Black);
		private Brush ctrlNotAttachedBrush = new SolidBrush(System.Drawing.Color.LightGray);
		private const UInt32 tabletSectionHeight = 125;

		private Pen mTCPShapePen = new Pen(System.Drawing.Color.Black, 2);
		private Pen mTCPShapeErasePen = new Pen(System.Drawing.Color.LightGray, 2);
		private Pen mTCPFingerPen = new Pen(System.Drawing.Color.Orange, 2);
		private Brush mTCPShapeEraseBrush = new SolidBrush(System.Drawing.Color.White);

		/// <summary>
		/// Holds attached devices.
		/// </summary>
		private List<byte> mTabletList = new List<byte>();

		public ExtensionTestForm()
		{
			InitializeComponent();

			mExtensionControlState = new ExtensionControlState();

			mTestImageIconPath =
				System.IO.File.Exists(mTestImageDefaultIconPath) ? mTestImageDefaultIconPath :
				System.IO.File.Exists(mTestImageIDEIconPath) ? mTestImageIDEIconPath :
				"";
		}

		/// <summary>
		/// Set up all the Wintab and Wintab extension properties.
		/// </summary>
		/// <returns></returns>
		private bool InitWintab()
		{
			bool status = false;

			try
			{
				mLogContext = CWintabInfo.GetDefaultDigitizingContext(ECTXOptionValues.CXO_MESSAGES);
				if (mLogContext == null)
				{
					return false;
					//throw new Exception("Oops - FAILED GetDefaultDigitizingContext");
				}

				// Control system cursor.
				mLogContext.Options |= (UInt32)ECTXOptionValues.CXO_SYSTEM;

				// Verify which extensions are available for targeting.
				// Once we know what the tablet supports, we can set up the data packet
				// definition to be sent events from those control types.

				// All tablets should have at least expresskeys.
				mExpKeysMask = CWintabExtensions.GetWTExtensionMask(EWTXExtensionTag.WTX_EXPKEYS2);

				if (mExpKeysMask > 0)
				{
					mLogContext.PktData |= (WTPKT)mExpKeysMask;
				}
				else
				{
					Debug.WriteLine("InitWintab: WTX_EXPKEYS2 mask not found!");
					throw new Exception("Oops - FAILED GetWTExtensionMask for WTX_EXPKEYS2");
				}

				// It's not an error if either / both of these are zero.  It simply means
				// that those control types are not supported.
				mTouchRingMask = CWintabExtensions.GetWTExtensionMask(EWTXExtensionTag.WTX_TOUCHRING);
				if (mTouchRingMask > 0)
				{
					mLogContext.PktData |= (WTPKT)mTouchRingMask;
				}

				mTouchStripMask = CWintabExtensions.GetWTExtensionMask(EWTXExtensionTag.WTX_TOUCHSTRIP);
				if (mTouchStripMask > 0)
				{
					mLogContext.PktData |= (WTPKT)mTouchStripMask;
				}

				status = mLogContext.Open();
				if (!status)
				{
					//throw new Exception("Oops - failed logContext.Open()");
					return false;
				}

				// Query for tablet list
				mTabletList = CWintabInfo.GetFoundDevicesIndexList();

				if (mTabletList.Count == 0)
				{
					MessageBox.Show("There are no attached tablets.");
				}

				// Create a data object and set its WT_PACKET handler.
				m_wtData = new CWintabData(mLogContext);
				m_wtData.SetWTPacketEventHandler(MyWTPacketEventHandler);

				foreach (var tabletIdx in mTabletList)
				{
					SetupControlsForTablet(tabletIdx);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("FormExtTestApp: InitWintab: " + ex.ToString());
			}

			return true;
		}

		/// <summary>
		/// Iterate through and setup all controls on tablet.
		/// </summary>
		/// <param name="tabletIndex_I">Zero-based tablet index</param>
		void SetupControlsForTablet(UInt32 tabletIndex_I)
		{
			if (mExpKeysMask > 0)
			{
				SetupControlsForExtension(tabletIndex_I, EWTXExtensionTag.WTX_EXPKEYS2, mExtensionControlState.SetupExpressKey);
			}

			if (mTouchRingMask > 0)
			{
				SetupControlsForExtension(tabletIndex_I, EWTXExtensionTag.WTX_TOUCHRING, mExtensionControlState.SetupTouchRing);
			}

			if (mTouchStripMask > 0)
			{
				SetupControlsForExtension(tabletIndex_I, EWTXExtensionTag.WTX_TOUCHSTRIP, mExtensionControlState.SetupTouchStrip);
			}
		}

		/// <summary>
		/// Remove all extension overrides for the tablet.
		/// </summary>
		/// <param name="tabletIndex_I"></param>
		void RemoveOverridesForTablet(UInt32 tabletIndex_I)
		{
			// Express Keys
			RemoveOverridesForExtension(tabletIndex_I, EWTXExtensionTag.WTX_EXPKEYS2);
			// Touch Rings
			RemoveOverridesForExtension(tabletIndex_I, EWTXExtensionTag.WTX_TOUCHRING);
			// Touch Strips
			RemoveOverridesForExtension(tabletIndex_I, EWTXExtensionTag.WTX_TOUCHSTRIP);
		}

		/// <summary>
		/// Remove application overrides for extension.
		/// </summary>
		/// <param name="tabletIndex_I"></param>
		/// <param name="extTagIndex_I"></param>
		void RemoveOverridesForExtension(
			UInt32 tabletIndex_I,
			EWTXExtensionTag extTagIndex_I)
		{
			UInt32 numCtrls = 0;
			UInt32 numFuncs = 0;
			UInt32 propOverride = 0;  // false

			try
			{
				// Get number of controls of this type.
				if (!CWintabExtensions.ControlPropertyGet(
					mLogContext.HCtx,
					(byte)extTagIndex_I,
					(byte)tabletIndex_I,
					0, // ignored
					0, // ignored
					(ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_CONTROLCOUNT,
					ref numCtrls))
				{ throw new Exception("Oops - Failed ControlPropertyGet for TABLET_PROPERTY_CONTROLCOUNT"); }

				// All tablets should have ExpressKeys (we assume).
				if (numCtrls == 0 && EWTXExtensionTag.WTX_EXPKEYS2 == extTagIndex_I)
				{ throw new Exception("Oops - SetupControlsForExtension didn't find any ExpressKeys!"); }

				// For each control, find its number of functions ...
				for (UInt32 controlIndex = 0; controlIndex < numCtrls; controlIndex++)
				{
					if (!CWintabExtensions.ControlPropertyGet(
						mLogContext.HCtx,
						(byte)extTagIndex_I,
						(byte)tabletIndex_I,
						(byte)controlIndex,
						0, // ignored
						(ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_FUNCCOUNT,
						ref numFuncs))
					{ throw new Exception("Oops - Failed ControlPropertyGet for TABLET_PROPERTY_FUNCCOUNT"); }

					// ... and override our setting for each function.
					for (UInt32 functionIndex = 0; functionIndex < numFuncs; functionIndex++)
					{
						if (!CWintabExtensions.ControlPropertySet(
							mLogContext.HCtx,
							(byte)extTagIndex_I,
							(byte)tabletIndex_I,
							(byte)controlIndex,
							(byte)functionIndex,
							(ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_OVERRIDE,
							propOverride))
						{ throw new Exception("Oops - FAILED ControlPropertySet for TABLET_PROPERTY_OVERRIDE"); }
					}
				}
			}
			catch (Exception ex)
			{
				//throw ex;
				Debug.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Iterate through and setup all controls on this extension. 
		/// </summary>
		/// <param name="tabletIndex_I">tablet index</param>
		/// <param name="extTagIndex_I">extension index tag</param>
		/// <param name="setupFunc_I">function called to setup extension control layout</param>
		public void SetupControlsForExtension(
			UInt32 tabletIndex_I,
			EWTXExtensionTag extTagIndex_I,
			DrawControlsSetupFunction setupFunc_I)
		{
			UInt32 numCtrls = 0;

			// Get number of controls of this type.
			if (!CWintabExtensions.ControlPropertyGet(
				mLogContext.HCtx,
				(byte)extTagIndex_I,
				(byte)tabletIndex_I,
				0, // ignored
				0, // ignored
				(ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_CONTROLCOUNT,
				ref numCtrls))
			{ throw new Exception("Oops - Failed ControlPropertyGet for TABLET_PROPERTY_CONTROLCOUNT"); }

			// All tablets should have ExpressKeys (we assume).
			if (numCtrls == 0 && EWTXExtensionTag.WTX_EXPKEYS2 == extTagIndex_I)
			{ throw new Exception("Oops - SetupControlsForExtension didn't find any ExpressKeys!"); }

			for (UInt32 idx = 0; idx < numCtrls; idx++)
			{
				SetupFunctionsForControl(tabletIndex_I, extTagIndex_I, idx, numCtrls, setupFunc_I);
			}
		}

		/// <summary>
		/// Iterate through and setup all functions on this control.
		/// </summary>
		/// </summary>
		/// <param name="tabletIndex_I">tablet index</param>
		/// <param name="extTagIndex_I">extension index tag</param>
		/// <param name="controlIndex_I">control index</param>
		/// <param name="setupFunc_I">function called to setup extension control layout</param>
		public void SetupFunctionsForControl(
			UInt32 tabletIndex_I,
			EWTXExtensionTag extTagIndex_I,
			UInt32 controlIndex_I,
			UInt32 numControls_I,
			DrawControlsSetupFunction setupFunc_I)
		{
			UInt32 numFuncs = 0;

			// Get the number of functions for this control.
			if (!CWintabExtensions.ControlPropertyGet(
				mLogContext.HCtx,
				(byte)extTagIndex_I,
				(byte)tabletIndex_I,
				(byte)controlIndex_I,
				0, // ignored
				(ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_FUNCCOUNT,
				ref numFuncs))
			{ throw new Exception("Oops - Failed ControlPropertyGet for TABLET_PROPERTY_FUNCCOUNT"); }

			Debug.Assert(numFuncs > 0);

			for (UInt32 funcIdx = 0; funcIdx < numFuncs; funcIdx++)
			{
				SetupPropertiesForFunctions(tabletIndex_I, extTagIndex_I, controlIndex_I, funcIdx, numControls_I, setupFunc_I);
			}
		}

		/// <summary>
		/// Iterate through all functions on this control.
		/// </summary>
		/// </summary>
		/// <param name="tabletIndex_I">tablet index</param>
		/// <param name="extTagIndex_I">extension index tag</param>
		/// <param name="controlIndex_I">control index</param>
		/// <param name="functionIndex_I">control function index</param>
		/// <param name="setupFunc_I">function called to setup extension control layout</param>
		public void SetupPropertiesForFunctions(
			UInt32 tabletIndex_I,
			EWTXExtensionTag extTagIndex_I,
			UInt32 controlIndex_I,
			UInt32 functionIndex_I,
			UInt32 numControls_I,
			DrawControlsSetupFunction setupFunc_I)
		{
			bool bIsAvailable = false;

			try
			{
				WTPKT propOverride = 1;  // true
				UInt32 ctrlAvailable = 0;
				UInt32 ctrlLocation = 0;
				UInt32 ctrlMinRange = 0;
				UInt32 ctrlMaxRange = 0;
				String indexStr = extTagIndex_I == EWTXExtensionTag.WTX_EXPKEYS2 ?
					Convert.ToString(controlIndex_I) :
					Convert.ToString(functionIndex_I);

				// NOTE - you can use strings in any language here.
				// The strings will be encoded to UTF8 before sent to the driver.
				// For example, you could use the string: "付録A" to indicate "EK" in Japanese.
				String ctrlname =
					extTagIndex_I == EWTXExtensionTag.WTX_EXPKEYS2 ? "EK: " + indexStr :
					extTagIndex_I == EWTXExtensionTag.WTX_TOUCHRING ? "TR: " + indexStr :
					extTagIndex_I == EWTXExtensionTag.WTX_TOUCHSTRIP ? "TS: " + indexStr :
					/* unknown control */                              "UK: " + indexStr;

				do
				{
					// Ask if control is available for override.
					if (!CWintabExtensions.ControlPropertyGet(
						mLogContext.HCtx,
						(byte)extTagIndex_I,
						(byte)tabletIndex_I,
						(byte)controlIndex_I,
						(byte)functionIndex_I,
						(ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_AVAILABLE,
						ref ctrlAvailable))
					{
						Debug.WriteLine($"Oops - FAILED ControlPropertyGet for TABLET_PROPERTY_AVAILABLE for tabletIdx: {tabletIndex_I}");
					}

					bIsAvailable = (ctrlAvailable > 0);

					if (!bIsAvailable)
					{
						Debug.WriteLine("Cannot override control");
						break;
					}

					// Set flag indicating we're overriding the control.
					if (!CWintabExtensions.ControlPropertySet(
					   mLogContext.HCtx,
					   (byte)extTagIndex_I,
					   (byte)tabletIndex_I,
					   (byte)controlIndex_I,
					   (byte)functionIndex_I,
					   (ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_OVERRIDE,
					   propOverride))
					{
						Debug.WriteLine($"Oops - FAILED ControlPropertySet for TABLET_PROPERTY_OVERRIDE for tabletIdx: {tabletIndex_I}");
					}

					// Set the control name.
					if (!CWintabExtensions.ControlPropertySet(
						mLogContext.HCtx,
						(byte)extTagIndex_I,
						(byte)tabletIndex_I,
						(byte)controlIndex_I,
						(byte)functionIndex_I,
						(ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_OVERRIDE_NAME,
						ctrlname))
					{
						Debug.WriteLine($"Oops - FAILED ControlPropertySet for TABLET_PROPERTY_OVERRIDE_NAME for tabletIdx: {tabletIndex_I}");
					}

					// Get the location of the control
					if (!CWintabExtensions.ControlPropertyGet(
						mLogContext.HCtx,
						(byte)extTagIndex_I,
						(byte)tabletIndex_I,
						(byte)controlIndex_I,
						(byte)functionIndex_I,
						(ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_LOCATION,
						ref ctrlLocation))
					{
						Debug.WriteLine($"Oops - FAILED ControlPropertyGet for TABLET_PROPERTY_LOCATION for tabletIdx: {tabletIndex_I}");
					}

					if (!CWintabExtensions.ControlPropertyGet(
						mLogContext.HCtx,
						(byte)extTagIndex_I,
						(byte)tabletIndex_I,
						(byte)controlIndex_I,
						(byte)functionIndex_I,
						(ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_MIN,
						ref ctrlMinRange))
					{
						Debug.WriteLine($"Oops - FAILED ControlPropertyGet for TABLET_PROPERTY_MIN for tabletIdx: {tabletIndex_I}");
					}

					if (!CWintabExtensions.ControlPropertyGet(
						mLogContext.HCtx,
						(byte)extTagIndex_I,
						(byte)tabletIndex_I,
						(byte)controlIndex_I,
						(byte)functionIndex_I,
						(ushort)EWTExtensionTabletProperty.TABLET_PROPERTY_MAX,
						ref ctrlMaxRange))
					{
						Debug.WriteLine($"Oops - FAILED ControlPropertyGet for TABLET_PROPERTY_MAX for tabletIdx: {tabletIndex_I}");
					}

					// Set tablet OLED with icon (if supported by the tablet).
					// Ignore return value for now.
					CWintabExtensions.SetDisplayProperty(
						mLogContext,
						extTagIndex_I,
						tabletIndex_I,
						controlIndex_I,
						functionIndex_I,
						mTestImageIconPath);

					// Finally, call function to setup control layout for rendering.
					// Control will be updated when WT_PACKETEXT packets received. 
					setupFunc_I(
						tabletIndex_I,
						(int)controlIndex_I,
						(int)functionIndex_I,
						bIsAvailable,
						(int)ctrlLocation,
						(int)ctrlMinRange,
						(int)ctrlMaxRange);

				} while (false);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Called when Wintab WT_PACKET events are received.
		/// </summary>
		/// <param name="sender_I">The EventMessage object sending the report.</param>
		/// <param name="eventArgs_I">eventArgs_I.Message.WParam contains ID of packet containing the data.</param>
		public void MyWTPacketEventHandler(Object sender_I, MessageReceivedEventArgs eventArgs_I)
		{
			if (m_wtData == null)
			{
				return;
			}

			if (eventArgs_I.Message.Msg == (Int32)EWintabEventMessage.WT_PACKET)
			{
				// Handle Pen input here if desired
			}
			else if (eventArgs_I.Message.Msg == (Int32)EWintabEventMessage.WT_PACKETEXT)
			{
				//Debug.WriteLine("Received WT_PACKETEXT");
				WTPKT hCtx = (WTPKT)eventArgs_I.Message.LParam;
				WTPKT pktID = (WTPKT)eventArgs_I.Message.WParam;
				WintabPacketExt pktExt = m_wtData.GetDataPacketExt(hCtx, pktID);

				if (pktExt.pkBase.nContext != mLogContext.HCtx)
				{
					throw new Exception("Oops - got a message from unknown context: " + pktExt.pkBase.nContext.ToString());
				}

				if (pktExt.pkBase.nContext == mLogContext.HCtx)
				{
					// Sets current control state on all control types, even though some updates will be NO-OPS
					// because those controls will not be supported for the tablet.
					UInt32 tabletIndex = (UInt32)pktExt.pkExpKey.nTablet;
					mExtensionControlState.UpdateExpressKey(tabletIndex, (int)pktExt.pkExpKey.nControl, (int)pktExt.pkExpKey.nLocation, pktExt.pkExpKey.nState);
					mExtensionControlState.UpdateTouchRing(tabletIndex, (int)pktExt.pkTouchRing.nControl, (int)pktExt.pkTouchRing.nMode, pktExt.pkTouchRing.nPosition);
					mExtensionControlState.UpdateTouchStrip(tabletIndex, (int)pktExt.pkTouchStrip.nControl, (int)pktExt.pkTouchStrip.nMode, pktExt.pkTouchStrip.nPosition);

					foreach (var tabletIdx in mTabletList)
					{
						if (tabletIdx == tabletIndex)
						{
							RefreshControls(tabletIndex, true); // Treat all tablets as attached
							break;
						}
					}
				}
			}
		}

		///////////////////////////////////////////////////////////////////////
		// Control properties methods
		//

		/// <summary>
		/// Refresh all supported controls based on current control state.
		/// (Must be called from a UI thread!)
		/// </summary>
		private void RefreshControls(UInt32 tabletIndex_I, bool attached_I)
		{
			RefreshExpressKeys(tabletIndex_I, attached_I);
			RefreshTouchRings(tabletIndex_I, attached_I);
			RefreshTouchStrips(tabletIndex_I, attached_I);
		}

		/// <summary>
		/// Updates ExpressKeys UI for specified tablet
		/// </summary>
		/// <param name="tabletIndex_I">Index assigned by the tablet driver</param>
		/// <param name="attached_I">true: tablet is USB-attached</param>
		private void RefreshExpressKeys(UInt32 tabletIndex_I, bool attached_I)
		{
			const UInt32 xMargin = 10;
			const UInt32 yMargin = 10;
			const UInt32 ctrlSize = 20;
			const UInt32 xPad = 75;
			const UInt32 yPad = 30;

			UInt32 xLeft = xMargin;
			UInt32 yTop = yMargin + (tabletIndex_I * tabletSectionHeight);

			if ((!mExtensionControlState.Tablets.ContainsKey(tabletIndex_I) ||
				(mExtensionControlState.Tablets[tabletIndex_I].expKeys == null)))
			{
				// get out of Dodge; this tablet has no express keys (or we can't find 'em).
				grfxEKP.DrawString("Tablet: " + Convert.ToString(tabletIndex_I) + " - No displayable controls",
					ctrlFont, ctrlTextBrush, xMargin, yTop);
				return;
			}

			if (grfxEKP == null)
			{
				throw new NullReferenceException("Oops - grfxEKP is null");
			}

			int numExpKeys = mExtensionControlState.Tablets[tabletIndex_I].expKeys.Length;
			int lastLocation = 0;

			Brush textBrush = attached_I ? ctrlTextBrush : ctrlNotAttachedBrush;
			Brush rectBrush = ctrlNotAttachedBrush;

			grfxEKP.DrawString("Tablet: " + Convert.ToString(tabletIndex_I), ctrlFont, textBrush, xMargin, yTop);
			yTop += yPad;

			for (int ctrlIndex = 0; ctrlIndex < numExpKeys; ctrlIndex++)
			{
				int location = mExtensionControlState.Tablets[tabletIndex_I].expKeys[ctrlIndex].location;

				bool down = mExtensionControlState.Tablets[tabletIndex_I].expKeys[ctrlIndex].down;

				if (location != lastLocation)
				{
					lastLocation = location;
					xLeft = xMargin;
					yTop += yPad;
				}

				if (attached_I)
				{
					rectBrush = down ? ctrlActiveBrush : ctrlNotPressedBrush;
				}

				grfxEKP.DrawString(location == 0 ? "Left" : "Right", ctrlFont, textBrush, xMargin, yTop);
				xLeft += xPad;

				grfxEKP.FillRectangle(rectBrush, xLeft, yTop, ctrlSize, ctrlSize);
			}
		}

		/// <summary>
		/// Updates TouchRings UI for specified tablet
		/// </summary>
		/// <param name="tabletIndex_I">Index assigned by the tablet driver</param>
		/// <param name="attached_I">true: tablet is USB-attached</param>
		private void RefreshTouchRings(UInt32 tabletIndex_I, bool attached_I)
		{
			const UInt32 xMargin = 10;
			const UInt32 yMargin = 10;
			const UInt32 ctrlSize = 50;
			const UInt32 xPad = 125;

			const float touchRingWidth = ctrlSize;
			const float touchRingHeight = ctrlSize;
			const float touchRingRadius = touchRingWidth / 2;

			const float modeButtonWidth = ctrlSize / 4;
			const float modeButtonHeight = ctrlSize / 4;

			UInt32 xLeft = xMargin;
			UInt32 yTop = yMargin + (tabletIndex_I * tabletSectionHeight);

			if ((!mExtensionControlState.Tablets.ContainsKey(tabletIndex_I) ||
				(mExtensionControlState.Tablets[tabletIndex_I].touchRings == null)))
			{
				// get out of Dodge; this tablet has no touch rings (or we can't find 'em).
				return;
			}

			if (grfxTCP == null)
			{
				throw new NullReferenceException("Oops - grfxTCP is null");
			}

			int numTouchRings = mExtensionControlState.Tablets[tabletIndex_I].touchRings.Length;

			// Update as many touch rings as we've got
			for (int controlIndex = 0; controlIndex < numTouchRings; controlIndex++)
			{
				bool down = mExtensionControlState.Tablets[tabletIndex_I].touchRings[controlIndex].down;

				float touchRingLeft = xLeft + (controlIndex * xPad);
				float touchRingTop = yTop;

				Point touchRingCenter = new Point((int)(touchRingLeft + touchRingWidth / 2), (int)(touchRingTop + touchRingHeight / 2));
				float fingerPosition =
					mExtensionControlState.Tablets[tabletIndex_I].touchRings[controlIndex].position -
					mExtensionControlState.Tablets[tabletIndex_I].touchRings[controlIndex].min;

				float range =
					mExtensionControlState.Tablets[tabletIndex_I].touchRings[controlIndex].max -
					mExtensionControlState.Tablets[tabletIndex_I].touchRings[controlIndex].min;

				float angle = (fingerPosition / range) * 2 * mPI - mPIDIV2;

				Console.WriteLine($"angle: {angle}");

				int fingerX = (int)(Math.Cos(angle) * touchRingRadius);
				int fingerY = (int)(Math.Sin(angle) * touchRingRadius);
				Point fingerPoint = new Point(touchRingCenter.X + fingerX, touchRingCenter.Y + fingerY);

				grfxTCP.SmoothingMode = SmoothingMode.AntiAlias;

				// Erase old line
				grfxTCP.FillEllipse(mTCPShapeEraseBrush, touchRingLeft, touchRingTop, touchRingWidth, touchRingHeight);

				// Draw empty circle representing touch ring
				grfxTCP.DrawEllipse(attached_I ? mTCPShapePen : mTCPShapeErasePen, touchRingLeft, touchRingTop, touchRingWidth, touchRingHeight);

				if (down)
				{
					// Draw a line from the center of the touchRing to the edge.
					grfxTCP.DrawLine(mTCPFingerPen, touchRingCenter, fingerPoint);
				}

				// Show mode buttons below touch rings
				float modeButtonLeft = touchRingLeft;
				float modeButtonTop = yTop + touchRingHeight + 10;
				foreach (var mode in mExtensionControlState.Tablets[tabletIndex_I].touchRings[controlIndex].modes)
				{
					if (mode.active)
					{
						grfxTCP.FillEllipse(ctrlActiveBrush, modeButtonLeft + 1, modeButtonTop + 1, modeButtonWidth - 2, modeButtonHeight - 2);
					}
					else
					{
						grfxTCP.FillEllipse(attached_I ? ctrlNotPressedBrush : mTCPShapeEraseBrush, modeButtonLeft + 1, modeButtonTop + 1, modeButtonWidth - 2, modeButtonHeight - 2);
						grfxTCP.DrawEllipse(attached_I ? mTCPShapePen : mTCPShapeErasePen, modeButtonLeft, modeButtonTop, modeButtonWidth, modeButtonHeight);
					}

					modeButtonLeft += modeButtonWidth + 10;
				}
			}
		}

		/// <summary>
		/// Updates TouchStrips UI for specified tablet
		/// </summary>
		/// <param name="tabletIndex_I">Index assigned by the tablet driver</param>
		/// <param name="attached_I">true: tablet is USB-attached</param>
		private void RefreshTouchStrips(UInt32 tabletIndex_I, bool attached_I)
		{
			const UInt32 xMargin = 10;
			const UInt32 yMargin = 10;
			const UInt32 xPad = 125;

			const UInt32 touchStripWidth = 100;
			const UInt32 touchStripHeight = 30;

			const UInt32 ctrlSize = 50;
			const float modeButtonWidth = ctrlSize / 4;
			const float modeButtonHeight = ctrlSize / 4;

			UInt32 xLeft = xMargin;
			UInt32 yTop = yMargin + (tabletIndex_I * tabletSectionHeight);

			if ((!mExtensionControlState.Tablets.ContainsKey(tabletIndex_I) ||
				(mExtensionControlState.Tablets[tabletIndex_I].touchStrips == null)))
			{
				// get out of Dodge; this tablet has no touch strips (or we can't find 'em).
				return;
			}

			if (grfxTCP == null)
			{
				throw new NullReferenceException("Oops - grfxTCP is null");
			}

			int numTouchStrips = mExtensionControlState.Tablets[tabletIndex_I].touchStrips.Length;

			if (numTouchStrips != 2)
			{
				throw new Exception("Oops - we should exactly two touch strips");
			}

			for (int controlIndex = 0; controlIndex < numTouchStrips; controlIndex++)
			{
				bool down = mExtensionControlState.Tablets[tabletIndex_I].touchStrips[controlIndex].down;

				float touchStripLeft = xLeft + (controlIndex * xPad);
				float touchStripTop = yTop;

				grfxTCP.SmoothingMode = SmoothingMode.AntiAlias;

				// Erase old line
				grfxTCP.FillRectangle(mTCPShapeEraseBrush, touchStripLeft, touchStripTop, touchStripWidth, touchStripHeight);

				// Draw empty rectangle representing touch strip
				grfxTCP.DrawRectangle(attached_I ? mTCPShapePen : mTCPShapeErasePen, touchStripLeft, touchStripTop, touchStripWidth, touchStripHeight);

				if (down)
				{
					float fingerPosition =
						mExtensionControlState.Tablets[tabletIndex_I].touchStrips[controlIndex].position + 1 -
						mExtensionControlState.Tablets[tabletIndex_I].touchStrips[controlIndex].min;

					float range =
						mExtensionControlState.Tablets[tabletIndex_I].touchStrips[controlIndex].max -
						mExtensionControlState.Tablets[tabletIndex_I].touchStrips[controlIndex].min;

					float scaledFingerPos = fingerPosition * (touchStripWidth / range);

					// Draw a vertical line at the finger position on the strip.
					int xPos = (int)(touchStripLeft + scaledFingerPos);
					grfxTCP.DrawLine(mTCPFingerPen, xPos, touchStripTop + 2, xPos, touchStripTop + touchStripHeight - 2);
				}

				// Show mode buttons below touch strips
				float modeButtonLeft = touchStripLeft;
				float modeButtonTop = yTop + touchStripHeight + 10;
				foreach (var mode in mExtensionControlState.Tablets[tabletIndex_I].touchStrips[controlIndex].modes)
				{
					if (mode.active)
					{
						grfxTCP.FillEllipse(ctrlActiveBrush, modeButtonLeft + 1, modeButtonTop + 1, modeButtonWidth - 2, modeButtonHeight - 2);
					}
					else
					{
						grfxTCP.FillEllipse(attached_I ? ctrlNotPressedBrush : mTCPShapeEraseBrush, modeButtonLeft + 1, modeButtonTop + 1, modeButtonWidth - 2, modeButtonHeight - 2);
						grfxTCP.DrawEllipse(attached_I ? mTCPShapePen : mTCPShapeErasePen, modeButtonLeft, modeButtonTop, modeButtonWidth, modeButtonHeight);
					}

					modeButtonLeft += modeButtonWidth + 10;
				}
			}
		}

		/// <summary>
		/// Initialize Wintab when the test form is loaded.
		/// If you do it at form construction time, you could exception when closing.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ExtensionTestForm_Load(object sender, EventArgs e)
		{
			grfxEKP = EKPanel.CreateGraphics();
			grfxTCP = TCPanel.CreateGraphics();

			if (!InitWintab())
			{
				String errmsg =
					"Oops - couldn't initialize Wintab.\n" +
					"Make sure the tablet driver is running and a tablet is plugged in.\n\n" +
					"Bailing out...";
				MessageBox.Show(errmsg);
				Close();
			}
		}

		/// <summary>
		/// Restore the tablet control functions and close Wintab context.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ExtensionTestForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (m_wtData != null)
			{
				m_wtData.RemoveWTPacketEventHandler(MyWTPacketEventHandler);
				m_wtData = null;
			}
			if (mLogContext != null && mLogContext.HCtx != 0)
			{
				foreach (var tabletIdx in mTabletList)
				{
					RemoveOverridesForTablet(tabletIdx);
				}

				mLogContext.Close();
			}
		}

		/// <summary>
		/// Resize dialog according to number of tablets (attached or not).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EKPanel_Paint(object sender, PaintEventArgs e)
		{
			//List<byte> list = CWintabInfo.GetFoundDevicesIndexList();
			int mult = mTabletList.Count + 1;
			this.Height = mult * (int)tabletSectionHeight;

			foreach (var tabletIdx in mTabletList)
			{
				RefreshControls(tabletIdx, true);
			}
		}
	}
}
