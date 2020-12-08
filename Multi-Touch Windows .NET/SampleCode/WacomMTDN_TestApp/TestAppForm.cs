/*----------------------------------------------------------------------------s
	NAME
		TestAppForm.cs

	PURPOSE
		This demo shows how to use Wacom Feel Multi-Touch in a .NET application
		to detect finger input

	COPYRIGHT
		This file is Copyright (c) Wacom Company, Ltd. 2020

		The text and information contained in this file may be freely used,
		copied, or distributed without compensation or licensing restrictions.
---------------------------------------------------------------------------- */

//#define TRACE_SYSTEM_INFO
//#define TRACE_FINGER_DATA
//#define TRACE_FINGER_POINTS
//#define TRACE_BLOB_DATA
//#define TRACE_RAW_DATA
//#define TRACE_UI_UPDATE
//#define TRACE_PEN_DATA
//#define TRACE_HIT_RECTS

using System;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using WacomMTDN;
using WintabDN;
using System.Threading;

namespace WacomMTTestApp
{
	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Shape drawn at finger position.
	/// </summary>
	public enum FingerShape
	{
		Circle,
		Square,
		Triangle,
		FilledCircle,
		FilledSquare,
		Cross,
		HorzBar,
		VertBar
	}

	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Used to help control what type of touch data client we are.
	/// </summary>
	public enum TouchDataType
	{
		Finger,
		Blob,
		Raw,
		Unknown
	}

	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Attributes when rendering finger position.
	/// </summary>
	public struct FingerRenderAttributes
	{
		Color		color;
		FingerShape	shape;
		float		penWidth;
		UInt32		length;		// in pixels

		public Color			Color { set { color = value; } get { return color; } }
		public FingerShape	Shape { set { shape = value; } get { return shape; } }
		public float			PenWidth { set { penWidth = value; } get { return penWidth; } }
		public UInt32			Length { set { length = value; } get { return length; } }

		public FingerRenderAttributes(Color color_I, FingerShape shape_I, float width_I, UInt32 length_I)
		{ color = color_I; shape = shape_I; penWidth = width_I; length = length_I; }
	}
	
	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Attributes when rendering blobs.
	/// </summary>
	public struct RenderAttributes
	{
		Color color;
		float penWidth;

		public Color Color { set { color = value; } get { return color; } }
		public float PenWidth { set { penWidth = value; } get { return penWidth; } }

		public RenderAttributes(Color color_I, float width_I)
		{ color = color_I; penWidth = width_I; }
	}
	
	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Test app to exercise WacomMTDN.
	/// </summary>
	public partial class TestForm : Form
	{
		/// <summary>
		/// Keep a map of attributes for rendering finger points.
		/// Key is finger index.
		/// </summary>
		private class FingerAttributesMap : Dictionary<Int32, FingerRenderAttributes> { }
		private FingerAttributesMap mFingerAttributesMap = new FingerAttributesMap();

		public delegate void UpdateFingerDataUIDelegate(ref WacomMTFingerCollection fingerCollection);
		public UpdateFingerDataUIDelegate mUpdateFingerDataUIDelegate = null;

		public delegate void UpdateBlobDataUIDelegate(ref WacomMTBlobAggregate blobCollection);
		public UpdateBlobDataUIDelegate mUpdateBlobDataUIDelegate = null;

		public delegate void UpdateRawDataUIDelegate(ref WacomMTRawData rawData);
		public UpdateRawDataUIDelegate mUpdateRawDataUIDelegate = null;

		public delegate void UpdateUIControlsDelegate(bool enable);
		public UpdateUIControlsDelegate mUpdateUIControlsDelegate;

		private CWacomMTConfig mWacomMTConfig = new CWacomMTConfig();
		private WacomMTCallback mTouchDataCallback = null;
		private WacomMTAttachCallback mAttachCallback;
		private WacomMTDetachCallback mDetachCallback;

		// This MTAPI client used for finger or blob data.
		private CWacomMTWindowClient mWacomMTWindowClient = null;

		// This MTAPI client used for raw data only.
		private CWacomMTRawClient mWacomMTRawClient = null;

		// This MTAPI client lets unprocessed touch data pass through to Windows.
		private CWacomMTWindowClient mWacomMTPassthroughWindowClient = null;

		// Userdata is a unicode string; used only when registering by hitrect.
		private String mUserDataString = "User data returned to client...";
		private String mUserDataStringReturned = "";
		private IntPtr mUserDataIntPtr = IntPtr.Zero;

		// Initialize as consumer; user can change from the UI.
		private WacomMTProcessingMode mClientProcessingMode = WacomMTConstants.CONSUMER_CLIENT;

		// Controls whether we use hitrects or hwnd to register client.
		private bool mRegisterClientByHitRect = false;

		// Current touch client.
		TouchDataType mTouchDataType = TouchDataType.Finger;

		// Flag to control whether we check finger confidence before rendering.
		private bool mCheckFingerConfidence = true;

		// This value should always reflect the device ID of the selected tablet.
		// Note that this is not the DeviceIDComboBox selection index.
		private Int32 mCurrentDeviceID = -1;

		//-----------------------------------------------------------------------
		// Data elements used for rendering blob and raw data.
		//
		private static RenderAttributes mBlobRenderAttributes = new RenderAttributes(Color.Blue, 2.0F);
		private static RenderAttributes mRawRenderAttributes = new RenderAttributes(Color.Purple, 2.0F);
		private static Pen mBlobPen = new Pen(mBlobRenderAttributes.Color, mBlobRenderAttributes.PenWidth);
		private static Pen mRawPen = new Pen(mRawRenderAttributes.Color, mRawRenderAttributes.PenWidth);

		//-----------------------------------------------------------------------
		// Data elements used for setting up Wintab pen context.
		//
		private CWintabContext m_logContext = null;
		private CWintabData m_wtData = null;
		private Int32 m_pkX = 0;
		private Int32 m_pkY = 0;
		private UInt32 m_pressure = 0;
		private UInt32 m_pkTime = 0;
		private Int32 m_maxPressure = CWintabInfo.GetMaxPressure();
		private UInt32 m_pkTimeLast = 0;

		private const Int32 m_TABEXTX = 10000;
		private const Int32 m_TABEXTY = 10000;

		// Data elements used for pen scribbling
		private Point m_lastPoint = Point.Empty;
		private Graphics m_graphics = null;
		private Pen m_pen;
		private Pen m_backPen;

		public TestForm()
		{
			InitializeComponent();

			Version version = Assembly.GetEntryAssembly().GetName().Version;
			this.Text = this.Text + " - " + version.ToString();

			mUpdateFingerDataUIDelegate = new UpdateFingerDataUIDelegate(this.DoFingerDataUpdateUI);
			mUpdateBlobDataUIDelegate = new UpdateBlobDataUIDelegate(this.DoBlobDataUpdateUI);
			mUpdateRawDataUIDelegate = new UpdateRawDataUIDelegate(this.DoRawDataUpdateUI);

			mUpdateUIControlsDelegate = new UpdateUIControlsDelegate(this.DoUpdateUIControls);

			// Preset attributes for each finger (key is finger index).
			// Use a circle shape for all fingers - less clutter in the display.
			mFingerAttributesMap.Add(0, new FingerRenderAttributes(Color.Blue, FingerShape.Circle, 2, 10));
			mFingerAttributesMap.Add(1, new FingerRenderAttributes(Color.Green, FingerShape.Circle, 2, 10));
			mFingerAttributesMap.Add(2, new FingerRenderAttributes(Color.Red, FingerShape.Circle, 2, 10));
			mFingerAttributesMap.Add(3, new FingerRenderAttributes(Color.Violet, FingerShape.Circle, 2, 10));
			mFingerAttributesMap.Add(4, new FingerRenderAttributes(Color.Orange, FingerShape.Circle, 2, 10));

			mFingerAttributesMap.Add(5, new FingerRenderAttributes(Color.DarkBlue, FingerShape.Circle, 2, 10));
			mFingerAttributesMap.Add(6, new FingerRenderAttributes(Color.DarkGreen, FingerShape.Circle, 2, 10));
			mFingerAttributesMap.Add(7, new FingerRenderAttributes(Color.DarkRed, FingerShape.Circle, 2, 10));
			mFingerAttributesMap.Add(8, new FingerRenderAttributes(Color.DarkViolet, FingerShape.Circle, 2, 10));
			mFingerAttributesMap.Add(9, new FingerRenderAttributes(Color.DarkOrange, FingerShape.Circle, 2, 10));

			UpdateUIStatus();
			
			try
			{
				// Prepares some test userdata to send when client is registered.
				mUserDataIntPtr = WacomMTDN.CMemUtils.AllocateUnmanagedString(mUserDataString);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Callback invoked by MTAPI when finger data is available.
		/// Finger data is rendered in the client window.
		/// </summary>
		/// <param name="packet_I">One packet of finger data.</param>
		/// <param name="userData_I">Client-defined userdata</param>
		public UInt32 DoFingerDataUpdateCallback(IntPtr packet_I, IntPtr userData_I)
		{
			try
			{
				// Recover the finger collection sent back.
				WacomMTFingerCollection fingerCollection =
					WacomMTDN.CMemUtils.PtrToStructure<WacomMTFingerCollection>(packet_I);

				// Recover the user data sent back.
				mUserDataStringReturned = WacomMTDN.CMemUtils.PtrToManagedString(userData_I);

#if TRACE_FINGER_DATA
				DumpFingerCollection(ref fingerCollection);
				Trace.WriteLine("Userdata: " + mUserDataStringReturned);
#endif //TRACE_FINGER_DATA

				// Does a synchronous UI update in the UI thread...
				Invoke(mUpdateFingerDataUIDelegate, fingerCollection);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			return 0;
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Callback invoked by MTAPI when blob data is available.
		/// Blob data is rendered in the client window.
		/// </summary>
		/// <param name="packet_I">One packet of blob data.</param>
		/// <param name="userData_I">Client-defined userdata</param>
		public UInt32 DoBlobDataUpdateCallback(IntPtr packet_I, IntPtr userData_I)
		{
			try
			{
				// Recover the finger collection sent back.
				WacomMTBlobAggregate blobCollection =
					WacomMTDN.CMemUtils.PtrToStructure<WacomMTBlobAggregate>(packet_I);

				// Recover the user data sent back.
				mUserDataStringReturned = WacomMTDN.CMemUtils.PtrToManagedString(userData_I);

#if TRACE_BLOB_DATA
				DumpBlobData(ref blobCollection);
#endif //TRACE_BLOB_DATA

				// Does a synchronous UI update in the UI thread...
				Invoke(mUpdateBlobDataUIDelegate, blobCollection);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			return 0;
		}


		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Callback invoked by MTAPI when raw data is available.
		/// Raw data is rendered in the client window.
		/// </summary>
		/// <param name="packet_I">One packet of raw data.</param>
		/// <param name="userData_I">Client-defined userdata</param>
		public UInt32 DoRawDataUpdateCallback(IntPtr packet_I, IntPtr userData_I)
		{
			try
			{
				// Recover the finger collection sent back.
				WacomMTRawData rawData =
					WacomMTDN.CMemUtils.PtrToStructure<WacomMTRawData>(packet_I);

				// Recover the user data sent back.
				mUserDataStringReturned = WacomMTDN.CMemUtils.PtrToManagedString(userData_I);

#if TRACE_RAW_DATA
				DumpRawData(ref rawData);
#endif //TRACE_RAW_DATA

				// Does a synchronous UI update in the UI thread...
				Invoke(mUpdateRawDataUIDelegate, rawData);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			return 0;
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Callback invoked by MTAPI when a device is attached.
		/// </summary>
		/// <param name="deviceCapability_I">Touch device capability structure</param>
		/// <param name="userData_I">Registered user data</param>
		private void DoAttachWindowClientCallback(WacomMTCapability deviceCapability_I, IntPtr userData_I)
		{
			try
			{
				// This will add the device to our configuration only if not already added.
				mWacomMTConfig.AddDevice(deviceCapability_I);

				//MessageBox.Show("DeviceID: " + deviceCapability_I.DeviceID + " has attached");

				// Enable UI controls only if we have attached touch devices.
				bool enable = mWacomMTConfig.GetNumAttachedTouchDevices() > 0;
				Invoke(mUpdateUIControlsDelegate, enable);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Callback invoked by MTAPI when a device is detached.
		/// </summary>
		/// <param name="deviceID_I">Device to which the client belongs</param>
		/// <param name="userData_I">Client-defined userdata</param>
		private void DoDetachWindowClientCallback(Int32 deviceID_I, IntPtr userData_I)
		{
			try
			{
				//MessageBox.Show("DeviceID: " + deviceID_I + " has detached");

				UnregisterWindowClient();
				mWacomMTConfig.RemoveDevice(deviceID_I);

				// It's possible we may still have an attached touch device.
				bool enable = mWacomMTConfig.GetNumAttachedTouchDevices() > 0;

				Invoke(mUpdateUIControlsDelegate, enable);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates a new MTAPI client according to the specified mode.
		/// </summary>
		/// <param name="deviceID_I">Device to which the client belongs</param>
		/// <param name="mode_I">Client mode (eg: Consumer or Observer)</param>
		private void CreateNewClient(Int32 deviceID_I, WacomMTProcessingMode mode_I)
		{
			try
			{
				// If client already created, then unregister first.
				UnregisterWindowClient();

				// Allow the driver to finish unregistering before
				// before creating new client.
				// Real world applications shouldn't be switching between
				// finger data and finger callback modes though.
				Thread.Sleep(500);

				switch (mTouchDataType)
				{
					case TouchDataType.Finger:
					case TouchDataType.Blob:
						CreateWindowTouchClient(deviceID_I, mode_I);
						break;

					case TouchDataType.Raw:
						CreateRawTouchClient(deviceID_I, mode_I);
						break;

					default:
						throw new Exception("Unsupported touch data type");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates an MTAPI windowed client (currently finger or blob).
		/// </summary>
		/// <param name="deviceID_I">touch device identifier</param>
		/// <param name="mode_I">MTAPI mode for processing touch data</param>
		private void CreateWindowTouchClient(Int32 deviceID_I, WacomMTProcessingMode mode_I)
		{
			try
			{
				if (mClientProcessingMode != mode_I)
				{
					mClientProcessingMode = mode_I;
				}

				if (mTouchDataType == TouchDataType.Finger)
				{
					mWacomMTWindowClient = new CWacomMTFingerClient(mClientProcessingMode);

					if (mRegisterClientByHitRect)
					{
						// Client must register a callback to receive finger data.
						mTouchDataCallback = new WacomMTCallback(this.DoFingerDataUpdateCallback);
					}
				}
				else if (mTouchDataType == TouchDataType.Blob)
				{
					mWacomMTWindowClient = new CWacomMTBlobClient(mClientProcessingMode);

					if (mRegisterClientByHitRect)
					{
						// Client must register a callback to receive blob data.
						mTouchDataCallback = new WacomMTCallback(this.DoBlobDataUpdateCallback);
					}
				}
				else
				{
					mClientProcessingMode = WacomMTProcessingMode.WMTProcessingModePassThrough;
					mWacomMTWindowClient = null;
					throw new Exception("Unsupported touch data type: " + mTouchDataType.ToString());
				}

				if (mRegisterClientByHitRect)
				{
					// Register a new client at the initial position of this form.
					// Initial client update will be wherever this form opens.
					mWacomMTWindowClient.RegisterHitRectClient(
						deviceID_I,
						GetClientHitRect(deviceID_I),
						ref mTouchDataCallback,
						mUserDataIntPtr);
				}
				else
				{
					// Client registers window handle to get data events.
					WacomMTDN.HWND hwnd = this.Handle;
					mWacomMTWindowClient.RegisterHWNDClient(deviceID_I, hwnd);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to create MTAPI window client: " + ex.ToString());
			}
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		///	Creates an MTAPI raw data client.
		/// </summary>
		/// <param name="deviceID_I">touch device identifier</param>
		/// <param name="mode_I">MTAPI mode for processing touch data</param>
		private void CreateRawTouchClient(Int32 deviceID_I, WacomMTProcessingMode mode_I)
		{
			try
			{
				if (mClientProcessingMode != mode_I)
				{
					mClientProcessingMode = mode_I;
				}

				if (mTouchDataType == TouchDataType.Raw)
				{
					mWacomMTRawClient = new CWacomMTRawClient(mClientProcessingMode);

					// Client must register a callback to receive raw data.
					mTouchDataCallback = new WacomMTCallback(this.DoRawDataUpdateCallback);

					mWacomMTRawClient.RegisterClient(deviceID_I, ref mTouchDataCallback, mUserDataIntPtr);
				}
				else
				{
					mClientProcessingMode = WacomMTProcessingMode.WMTProcessingModePassThrough;
					mWacomMTRawClient = null;
					throw new Exception("Unhandled touch data type");
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show("Failed to create MTAPI raw client: " + ex.ToString());
			}
		}


		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// For a display device that is a hitrect client, unregisters client at 
		/// its last hitrect position; re-registers client at the current app 
		/// window location. 
		/// 
		/// This is a NO-OP for non-display devices (eg: Intuos Pro tablet) since 
		/// the hitrect for a non-display tablet never changes; it's the entire 
		/// tablet surface.
		/// 
		/// Also a NO-OP if the client is a registered HWND client since such
		/// clients are followed around automatically by the MTAPI.
		/// 
		/// Used when app is moved or resized.
		/// </summary>
		private void UpdateWindowClientRegistration()
		{
			try
			{
				if (mWacomMTWindowClient != null)
				{
					Int32 deviceID = mWacomMTWindowClient.DeviceID;
					if (mWacomMTWindowClient.IsRegisteredAsHitRectClient() &&
						 mWacomMTConfig.IsDisplayTablet(deviceID))
					{
						// Automatically re-register client at current hitrect.
						// Note that MoveHitRectClient takes the place of these two calls:
						//		UnregisterHitRectClient and RegisterHitRectClient
						mWacomMTWindowClient.MoveHitRectClient(
							GetClientHitRect(deviceID),
							mUserDataIntPtr);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}


		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Unregisters a Window MTAPI client.
		/// </summary>
		private void UnregisterWindowClient()
		{
			try
			{
				if (mWacomMTWindowClient != null)
				{
					if (mRegisterClientByHitRect)
					{
						// Unregister client at its last registered hitrect, before
						// removing the device.
						mWacomMTWindowClient.UnregisterHitRectClient();
					}
					else
					{
						mWacomMTWindowClient.UnregisterHWNDClient();
					}

					mWacomMTWindowClient = null;
				}
				else if ( mWacomMTRawClient != null )
				{
					mWacomMTRawClient.UnregisterClient();

					mWacomMTRawClient = null;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Dump finger collection data to debug OUT.
		/// </summary>
		/// <param name="fingerCollection_I"></param>
		public void DumpFingerCollection(ref WacomMTFingerCollection fingerCollection_I)
		{
			try
			{
				int numFingers = (int)fingerCollection_I.FingerCount;

				if (numFingers > 0)
				{
					// Enumerate the fingers from the unmanaged data passed in.
					for (UInt32 idx = 0; idx < numFingers; idx++)
					{
						WacomMTFinger finger = fingerCollection_I.GetFingerByIndex(idx);

						Trace.WriteLine("Finger ID:     " + finger.FingerID);
						Trace.WriteLine("  X:           " + finger.X);
						Trace.WriteLine("  Y:           " + finger.Y);
						Trace.WriteLine("  Width:       " + finger.Width);
						Trace.WriteLine("  Height:      " + finger.Height);
						Trace.WriteLine("  Sensitivity: " + finger.Sensitivity);
						Trace.WriteLine("  Orientation: " + finger.Orientation);
						Trace.WriteLine("  Confidence:  " + finger.Confidence);
						Trace.WriteLine("  TouchState:  " + finger.TouchState);
					}
				}
				else
				{
					Trace.WriteLine("WacomMTDN Test: WARNING: no fingers in fingerCollection!");
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show("DumpFingerCollection failed: " + ex.ToString());
			}
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Dump blob data to debug OUT.
		/// </summary>
		/// <param name="blobData_I"></param>
		public void DumpBlobData(ref WacomMTBlobAggregate blobData_I)
		{
			Trace.WriteLine("BlobData:");
			try
			{
				Int32 numBlobs = (Int32)blobData_I.BlobCount;

				Trace.WriteLine("  DeviceID:  " + blobData_I.DeviceID);
				Trace.WriteLine("  FrameNum:  " + blobData_I.FrameNumber);
				Trace.WriteLine("  BlobCount: " + numBlobs);

				if (numBlobs > 0)
				{
					// Enumerate the blobs from the unmanaged data passed in.
					for (UInt32 idx = 0; idx < numBlobs; idx++)
					{
						Trace.WriteLine("  BlobPoint: " + idx);
						WacomMTBlob blob = blobData_I.GetBlobByIndex(idx);

						Int32 numBlobPoints = (Int32)blob.PointCount;

						Trace.WriteLine("    BlobID:     " + blob.BlobID);
						Trace.WriteLine("    BlobType:   " + blob.BlobType);
						Trace.WriteLine("    Confidence: " + blob.Confidence);
						Trace.WriteLine("    NumPoints:  " + blob.PointCount);

						for (UInt32 jdx = 0; jdx < numBlobPoints; jdx++)
						{
							WacomMTBlobPoint blobPoint = blob.GetBlobPointByIndex(jdx);
							Trace.WriteLine("      X,Y: [" + (Int32)blobPoint.X + "," + (Int32)blobPoint.Y + "]");
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("DumpBlobData failed: " + ex.ToString());
			}
		}
		
		/////////////////////////////////////////////////////////////////////////
		public void DumpRawData(ref WacomMTRawData rawData_I)
		{
			Trace.WriteLine("DumpRawData:");
			try
			{
				Int32 numRawPoints = (Int32)rawData_I.ElementCount;
				Int32 deviceID = (Int32)rawData_I.DeviceID;

				WacomMTCapability caps = mWacomMTConfig.GetDeviceCaps(deviceID);
				UInt32 scanSizeX = caps.ScanSizeX;
				UInt32 scanSizeY = caps.ScanSizeY;
				float logWidth = caps.LogicalWidth;
				float logHeight = caps.LogicalHeight;

				Trace.WriteLine("  DeviceID:          " + rawData_I.DeviceID);
				Trace.WriteLine("  FrameNum:          " + rawData_I.FrameNumber);
				Trace.WriteLine("  RawPointCount:     " + numRawPoints);
				Trace.WriteLine("  ScanSize (tablet): [" + scanSizeX + "," + scanSizeY + "]");
				Trace.WriteLine("  Logical (pixels):  [" + logWidth + "," + logHeight + "]");
				Trace.WriteLine("  Sensitivity values at tablet [x,y] locations:");

				if (numRawPoints > 0)
				{
					// Enumerate the sensitivity values from the unmanaged data passed in.
					// Display only the non-zero sensitivity values.
					for (UInt32 sy = 0; sy < scanSizeY; sy++)
					{
						for (UInt32 sx = 0; sx < scanSizeX; sx++)
						{
							UInt16 index = (UInt16)(sy * scanSizeX + sx);
							UInt16 sensitivity = rawData_I.GetRawSensitivityByIndex(index);
							if (sensitivity > 0)
							{
								Trace.WriteLine("    [" + sx + "," + sy + "]: " + sensitivity);
							}
						}
					}
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show("DumpRawData failed: " + ex.ToString());
			}
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Update the UI with finger data during a callback.
		/// </summary>
		/// <param name="fingerCollection_I">finger data to render</param>
		private void DoFingerDataUpdateUI(ref WacomMTFingerCollection fingerCollection_I)
		{
#if TRACE_UI_UPDATE
			Trace.WriteLine("DoFingerDataUpdateUI ...");
			DumpFingerCollection(ref fingerCollection_I);
#endif //TRACE_UI_UPDATE
			bool allFingersUp = AllFingersUp(ref fingerCollection_I);
			uint numFingers = fingerCollection_I.FingerCount;

			//Trace.WriteLine("DoFingerDataUpdateUI: numFingers: " + numFingers + ", AllFingersUp: " + (allFingersUp ? "TRUE":"FALSE"));

			UpdateUIStatusBarUserData(mUserDataStringReturned, allFingersUp);
			UpdateUIStatusBarFingerCount(numFingers, allFingersUp);

			// Render each finger in the collection.
			for (UInt32 idx = 0; idx < numFingers; idx++)
			{
				WacomMTFinger finger = fingerCollection_I.GetFingerByIndex(idx);
				RenderFinger((Int32)idx, ref finger);
			}
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Update the UI with blob data during a callback.
		/// </summary>
		/// <param name="blobData_I">blob data to render</param>
		private void DoBlobDataUpdateUI(ref WacomMTBlobAggregate blobData_I)
		{
			Int32 numBlobs = (Int32)blobData_I.BlobCount;


			for (UInt32 idx = 0; idx < numBlobs; idx++)
			{
				WacomMTBlob blob = blobData_I.GetBlobByIndex(idx);
				RenderBlob(ref blob);
			}
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Update the UI with raw data during a callback.
		/// </summary>
		/// <param name="blobCollection_I">raw data to render</param>
		private void DoRawDataUpdateUI(ref WacomMTRawData rawData_I)
		{
			try
			{
				Int32 numRawPoints = (Int32)rawData_I.ElementCount;
				Int32 deviceID = (Int32)rawData_I.DeviceID;
				WacomMTCapability caps = mWacomMTConfig.GetDeviceCaps(deviceID);

				if (numRawPoints > 0)
				{
					UInt32 scanSizeX = caps.ScanSizeX;
					UInt32 scanSizeY = caps.ScanSizeY;

					// Enumerate the sensitivity values from the unmanaged data passed in.
					// Display only the non-zero sensitivity values.
					for (UInt32 sy = 0; sy < scanSizeY; sy++)
					{
						for (UInt32 sx = 0; sx < scanSizeX; sx++)
						{
							UInt16 index = (UInt16)(sy * scanSizeX + sx);
							UInt16 sensitivity = rawData_I.GetRawSensitivityByIndex(index);
							if (sensitivity > 0)
							{
								RenderRawPoint(sx, sy, sensitivity, ref caps);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("DoRawDataUpdateUI failed: " + ex.ToString());
			}
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Update the UI controls during a callback.
		/// </summary>
		/// <param name="enable"></param>
		private void DoUpdateUIControls(bool enable)
		{
			UpdateUIStatus();
			UpdateUIControls(enable);
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Draw finger data.
		/// </summary>
		/// <param name="fingerIndex_I">finger index used for render attributes</param>
		/// <param name="finger_I">One finger's data to render.></param>
		private void RenderFinger(Int32 fingerIndex_I, ref WacomMTFinger finger_I)
		{
			Rectangle rect = new Rectangle();

			if (fingerIndex_I >= WacomMTConstants.MAX_NUMBER_FINGERS)
			{
				throw new Exception("RenderFinger bailing due to large finger index: " + fingerIndex_I);
			}

			// If using finger confidence to determine whether to render the finger,
			// and the finger is not confident, then don't render it.
			if (mCheckFingerConfidence && !finger_I.Confidence)
			{
				return;
			}

			Graphics panelGraphics = fingerPanel.CreateGraphics();
			Pen myPen = null;
			Brush myBrush = null;
			Point clientPoint = new Point(0, 0);

			if (mWacomMTConfig.IsDisplayTablet(mWacomMTWindowClient.DeviceID))
			{
				// This is a display tablet (eg: Cintiq 24HDT). 
				// The returned X/Y are in screen pixel coordinates.
				// Convert screen to client coordinates.
				clientPoint = fingerPanel.PointToClient(new Point((int)finger_I.X, (int)finger_I.Y));

#if TRACE_FINGER_POINTS
				Trace.WriteLine(
					"fingerPoint: [" + finger_I.X + "," + finger_I.Y + "], " +
					"clientPoint: [" + clientPoint.X + "," + clientPoint.Y + "]");
#endif
			}
			else
			{
				// This is an opaque tablet (eg: Intuos 5).
				// The returned X/Y are in logical coordinates (0 to 1.0).
				// Scale to the client rectangle.
				clientPoint = new Point(
					(int)(this.ClientRectangle.Left + finger_I.X * this.ClientRectangle.Width),
					(int)(this.ClientRectangle.Top + finger_I.Y * this.ClientRectangle.Height));
			}

			try
			{
				FingerRenderAttributes attribs = mFingerAttributesMap[fingerIndex_I];
				Int32 radius = (Int32)attribs.Length;
				myPen = new Pen(attribs.Color, attribs.PenWidth);
				myBrush = new SolidBrush(attribs.Color);

				switch (attribs.Shape)
				{
					case FingerShape.Square:
						{
							panelGraphics.DrawRectangle(myPen, new Rectangle(clientPoint.X - radius, clientPoint.Y - radius, 2 * radius, 2 * radius));
						}
						break;

					case FingerShape.Cross:
						{
							panelGraphics.DrawLine(myPen, new Point(clientPoint.X - radius, clientPoint.Y), new Point(clientPoint.X + radius, clientPoint.Y));
							panelGraphics.DrawLine(myPen, new Point(clientPoint.X, clientPoint.Y - radius), new Point(clientPoint.X, clientPoint.Y + radius));
						}
						break;

					case FingerShape.HorzBar:
						{
							panelGraphics.DrawLine(myPen, new Point(clientPoint.X - radius, clientPoint.Y), new Point(clientPoint.X + radius, clientPoint.Y));
							panelGraphics.DrawLine(myPen, new Point(clientPoint.X - radius, clientPoint.Y - 2), new Point(clientPoint.X - radius, clientPoint.Y + 2));
							panelGraphics.DrawLine(myPen, new Point(clientPoint.X + radius, clientPoint.Y - 2), new Point(clientPoint.X + radius, clientPoint.Y + 2));
						}
						break;

					case FingerShape.VertBar:
						{
							panelGraphics.DrawLine(myPen, new Point(clientPoint.X, clientPoint.Y - radius), new Point(clientPoint.X, clientPoint.Y + radius));
							panelGraphics.DrawLine(myPen, new Point(clientPoint.X - 2, clientPoint.Y - radius), new Point(clientPoint.X + 2, clientPoint.Y - radius));
							panelGraphics.DrawLine(myPen, new Point(clientPoint.X - 2, clientPoint.Y + radius), new Point(clientPoint.X + 2, clientPoint.Y + radius));
						}
						break;

					case FingerShape.Circle:
					default:
						{
							rect = new Rectangle(clientPoint.X - radius, clientPoint.Y - radius, 2 * radius, 2 * radius);

							if ( finger_I.Confidence )
							{
								panelGraphics.DrawEllipse(myPen, rect);
							}
							else
							{
								panelGraphics.FillEllipse(myBrush, rect);
							}
						}
						break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString() + " - rect: [" + rect.Left + "," + rect.Top + "," + rect.Width + "," + rect.Height + "]");
			}
			finally
			{
				panelGraphics.Dispose();

				if (myPen != null) { myPen.Dispose(); }
			}
		}

		/////////////////////////////////////////////////////////////////////////
		private void RenderBlob(ref WacomMTBlob blob_I)
		{
			Graphics panelGraphics = fingerPanel.CreateGraphics();
			try
			{
				Point[] pointArray = GetBlobPointArray(ref blob_I);

				if (pointArray != null)
				{
					int length = pointArray.GetLength(0);

					if (length >= 4)
					{
						// This draws a spline connecting the blob's points.
						// DrawClosedCurve cannot accept any fewer points, else it exceptions.
					   panelGraphics.DrawClosedCurve(mBlobPen, pointArray);
					}
					else
					{
						// This draws an ellipse for 1-3 points.
						float locX = 0, locY = 0;
						float width = 0, height = 0;
						Int32 minX = Int32.MaxValue, minY = Int32.MaxValue;
						Int32 maxX = Int32.MinValue, maxY = Int32.MinValue;

						foreach (Point point in pointArray)
						{
							locX += point.X; locY += point.Y;
							minX = Math.Min(minX, point.X); minY = Math.Min(minY, point.Y);
							maxX = Math.Max(maxX, point.X); maxY = Math.Max(maxY, point.Y);
						}

						locX /= length;
						locY /= length;
						width = minX == maxX ? 10 : maxX - minX;
						height = minY == maxY ? 10 : maxY - minY;

						panelGraphics.DrawEllipse(mBlobPen, locX, locY, width, height);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
			finally
			{
				panelGraphics.Dispose();
			}
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns a Point[] suitable for rendering in a Graphics context.
		/// </summary>
		/// <param name="blob_I"></param>
		/// <returns></returns>
		private Point[] GetBlobPointArray(ref WacomMTBlob blob_I)
		{
			Point[] pointArray = null;

			try
			{
				UInt32 numBlobPoints = blob_I.PointCount;

				if (numBlobPoints >= 0)
				{
					pointArray = new Point[numBlobPoints];
					for (UInt32 idx = 0; idx < numBlobPoints; idx++)
					{
						WacomMTBlobPoint blobPoint = blob_I.GetBlobPointByIndex(idx);
						pointArray[idx] = fingerPanel.PointToClient(new Point((Int32)blobPoint.X, (Int32)blobPoint.Y));
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed GetBlobPointArray: " + ex.ToString());
			}

			return pointArray;
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Render a single raw tablet point.
		/// </summary>
		/// <param name="sx_I">tablet column location</param>/// 
		/// <param name="sy_I">tablet row location</param>
		/// <param name="sensitivity_I">raw sensitivity at [row, col]</param>
		/// <param name="caps_I">touch device capabilities</param>
		private void RenderRawPoint(UInt32 sx_I, UInt32 sy_I, UInt16 sensitivity_I, ref WacomMTCapability caps_I)
		{
			Graphics panelGraphics = fingerPanel.CreateGraphics();
			try
			{
				// Map tablet location to display pixels.
				Point displayPoint = new Point(
					(Int32)sx_I * (Int32)(caps_I.LogicalWidth / caps_I.ScanSizeX) + (Int32)caps_I.LogicalOriginX,
					(Int32)sy_I * (Int32)(caps_I.LogicalHeight / caps_I.ScanSizeY) + (Int32)caps_I.LogicalOriginY);

				// Map display pixels to client pixels.
				Point clientPoint = fingerPanel.PointToClient(displayPoint);

				// Draw circle proportional to the sensitivity
				Int32 radius = sensitivity_I / 100 + 10;
				panelGraphics.DrawEllipse(mRawPen, new Rectangle(
					clientPoint.X - radius, clientPoint.Y - radius, 2 * radius, 2 * radius));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
			finally
			{
				panelGraphics.Dispose();
			}
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns hitrect for the specified client.
		/// Hitrect will reflect whether the tablet is a Display tablet or not.
		/// </summary>
		/// <param name="deviceID_I">Touch device identifier</param>
		/// <returns></returns>
		private WacomMTHitRect GetClientHitRect(Int32 deviceID_I)
		{
			if (mWacomMTConfig.IsDisplayTablet(deviceID_I))
			{
				// Logical units are in screen pixels.
				Point panelScreenLoc = fingerPanel.PointToScreen(new Point(0, 0));
				WacomMTHitRect hitRect = new WacomMTHitRect(
					panelScreenLoc.X,
					panelScreenLoc.Y,
					fingerPanel.Width,
					fingerPanel.Height
					);

#if TRACE_HIT_RECTS
				Trace.WriteLine("Hitrect: [" +
					hitRect.OriginX + "," + 
					hitRect.OriginY + "," +
					hitRect.Width   + "," +
					hitRect.Height + "]");
#endif
				return hitRect;
			}
			else
			{
				// Logical units: 0 -> 1.0
				return new WacomMTHitRect(
					0,
					0,
					1,
					1);
			}
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Updates UI status and control elements.
		/// </summary>
		private void UpdateUIStatus()
		{
			UpdateUIStatusBarClientMode();
			UpdateUIStatusBarUserData();
			UpdateUIStatusBarFingerCount();
			UpdateUIStatusBarConfidenceBitsEnabled();
			UpdateUIDeviceIDList();
		}

				/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Updates client mode status indicator in UI.
		/// </summary>
		private void UpdateUIStatusBarClientMode()
		{
			this.ToolStripStatusClientModeLabel.Text =
				mClientProcessingMode == WacomMTConstants.CONSUMER_CLIENT ? "CONSUMER" : "OBSERVER";

			this.ToolStripStatusClientModeLabel.BackColor =
			mClientProcessingMode == WacomMTConstants.CONSUMER_CLIENT ? Color.Yellow : Color.LightGray;
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Updates user data indicator in UI.
		/// </summary>
		/// <param name="userDataStr_I">client userdata string</param>
		/// <param name="allFingersUp_I">indicates if all fingers off the tablet.</param>
		private void UpdateUIStatusBarUserData(String userDataStr_I = "", bool allFingersUp_I = true)
		{
			try
			{
				if (mRegisterClientByHitRect)
				{
					this.ToolStripStatusUserDataLabel.Text = allFingersUp_I ? "" : userDataStr_I;
					this.ToolStripStatusUserDataLabel.Enabled = true;
				}
				else
				{
					this.ToolStripStatusUserDataLabel.Text = "<HWND client: no userdata supported>";
					this.ToolStripStatusUserDataLabel.Enabled = false;
				}

				this.ToolStripStatusClientModeLabel.Invalidate();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Updates finger count in status bar.
		/// </summary>
		/// <param name="numFingers_I"></param>
		/// <param name="allFingersUp_I"></param>
		private void UpdateUIStatusBarFingerCount(UInt32 numFingers_I = 0, bool allFingersUp_I = true)
		{
			try
			{
				this.ToolStripStatusFingerCountLabel.Text = 
					allFingersUp_I ? "#fingers: 0" : "#fingers: " + numFingers_I.ToString();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Update confidence bits enabled indicator.
		/// </summary>
		private void UpdateUIStatusBarConfidenceBitsEnabled()
		{
			this.ToolStripStatusConfidenceBitsEnabledLabel.Text =
				mCheckFingerConfidence ? "Conf. Bits Enabled" : "Conf. Bits Disabled";

			this.ToolStripStatusConfidenceBitsEnabledLabel.BackColor =
				mCheckFingerConfidence ? Color.Lime : Color.Orange;
		}

				/////////////////////////////////////////////////////////////////////////
		/// <summary>
		///Updates UI controls along side of client rectangle.
		/// </summary>
		/// <param name="enable"></param>
		private void UpdateUIControls(bool enable)
		{
			bool selectedDeviceHasBlobAvailable = false;
			bool selectedDeviceHasRawAvailable = false;

			if (enable)
			{
				Int32 selectedDeviceIndex = this.DeviceIDComboBox.SelectedIndex;
				if (selectedDeviceIndex >= 0)
				{
					Int32 deviceID = mWacomMTConfig.DeviceIDList.data[selectedDeviceIndex];
					WacomMTCapability caps = mWacomMTConfig.GetDeviceCaps(deviceID);

					selectedDeviceHasBlobAvailable =
						(caps.CapabilityFlags & WacomMTCapabilityFlags.WMTCapabilityFlagsBlobAvailable) > 0;

					selectedDeviceHasRawAvailable =
						(caps.CapabilityFlags & WacomMTCapabilityFlags.WMTCapabilityFlagsRawAvailable) > 0;
				}
			}
			else
			{
				MessageBox.Show(this, "No touch devices found", "WacomMTDN Demo");
			}

			this.ClientModeGroupBox.Enabled = enable;
			this.FingerDataFromGroupBox.Enabled = enable;
			this.BlobDataFromCallbackRadioButton.Enabled = enable && selectedDeviceHasBlobAvailable;
			this.BlobDataFromEventRadioButton.Enabled = enable && selectedDeviceHasBlobAvailable;
			this.RawDataFromCallbackRadioButton.Enabled = enable && selectedDeviceHasRawAvailable;
			this.DeviceIDSelectLabel.Enabled = enable;
			this.DeviceIDComboBox.Enabled = enable;
			this.DeviceIDCapabilitiesFormButton.Enabled = enable;
			this.ConfidenceBitsEnabledGroupBox.Enabled = enable;
			this.ToolStripStatusClientModeLabel.Enabled = enable;
			this.ToolStripStatusConfidenceBitsEnabledLabel.Enabled = enable;
			this.ToolStripStatusFingerCountLabel.Enabled = enable;

			UpdateUIStatusBarUserData();
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Updates the list of attached devices in the UI
		/// </summary>
		/// <returns>true if any touch devices detected</returns>
		private bool UpdateUIDeviceIDList()
		{
			Int32 savedIndex = this.DeviceIDComboBox.SelectedIndex;

			this.DeviceIDComboBox.Items.Clear();
			Int32 numAttachedDevices = mWacomMTConfig.GetNumAttachedTouchDevices();

			try
			{
				if (numAttachedDevices > 0)
				{
					int newIndex = 0;
					WacomMTDeviceIDArray deviceIDList = mWacomMTConfig.DeviceIDList;

					for (int idx = 0; idx < numAttachedDevices; idx++)
					{
						Int32 deviceID = deviceIDList.data[idx];
						this.DeviceIDComboBox.Items.Add(deviceID);
					}

					// Maintain the same device selection if possible.
					if ( savedIndex >= numAttachedDevices )
					{
						newIndex = numAttachedDevices - 1;
					}
					else if (savedIndex >= 0)
					{
						newIndex = savedIndex;
					}

					this.DeviceIDComboBox.SelectedIndex = newIndex;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			return numAttachedDevices > 0;
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns true if all fingers have status UP in the specified finger collection.
		/// </summary>
		/// <param name="fingerCollection_I">Finger collection being assessed for all UP state.</param>
		/// <returns></returns>
		private bool AllFingersUp(ref WacomMTFingerCollection fingerCollection_I)
		{
			for (UInt32 idx = 0; idx < fingerCollection_I.FingerCount; idx++)
			{
				if (fingerCollection_I.GetFingerByIndex(idx).TouchState != WacomMTFingerState.WMTFingerStateUp)
				{
					return false;
				}
			}

			return true;
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initialize to capture pen data using Wintab.
		/// </summary>
		private void InitWintab()
		{
			ClearDisplay();
			Enable_Scribble(true);

			// Control the system cursor with the pen.
			bool controlSystemCursor = true;

			// Open a context and try to capture pen data;
			InitDataCapture(m_TABEXTX, m_TABEXTY, controlSystemCursor);
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Set up the graphics context to do pen scribbling.
		/// </summary>
		/// <param name="enable"></param>
		private void Enable_Scribble(bool enable = false)
		{
			if (enable)
			{
				// Init scribble graphics.
				//m_graphics = CreateGraphics();
				m_graphics = fingerPanel.CreateGraphics();
				m_graphics.SmoothingMode = SmoothingMode.AntiAlias;

				m_pen = new Pen(Color.Black);
				m_backPen = new Pen(Color.White);

				// You should now be able to scribble in the scribblePanel.
			}
			else
			{
				// Remove Wintab scribble context.
				CloseCurrentContext();

				// Turn off graphics.
				if (m_graphics != null)
				{
					fingerPanel.Invalidate();
					m_graphics.Dispose();
					m_graphics = null;
				}
			}
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initialize a Wintab context for data capture.
		/// </summary>
		/// <param name="ctxWidth_I"></param>
		/// <param name="ctxHeight_I"></param>
		/// <param name="ctrlSysCursor_I"></param>
		private void InitDataCapture(
			 int ctxWidth_I = m_TABEXTX, int ctxHeight_I = m_TABEXTY, bool ctrlSysCursor_I = true)
		{
			try
			{
				// Close context from any previous test.
				CloseCurrentContext();

				m_logContext = OpenSystemContext(ctxWidth_I, ctxHeight_I, ctrlSysCursor_I);

				if (m_logContext == null)
				{
					return;
				}

				// Create a data object and set its WT_PACKET handler.
				m_wtData = new CWintabData(m_logContext);
				m_wtData.SetWTPacketEventHandler(MyWTPacketEventHandler);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Handler for Wintab WM_PACKET data events.
		/// </summary>
		/// <param name="sender_I"></param>
		/// <param name="eventArgs_I"></param>
		public void MyWTPacketEventHandler(Object sender_I, MessageReceivedEventArgs eventArgs_I)
		{
			//System.Diagnostics.Debug.WriteLine("Received WT_PACKET event");
			if (m_wtData == null || m_graphics == null)
			{
				return;
			}

			try
			{
				uint pktID = (uint)eventArgs_I.Message.WParam;
				WintabPacket pkt = m_wtData.GetDataPacket((uint)eventArgs_I.Message.LParam, pktID);
				//DEPRECATED WintabPacket pkt = m_wtData.GetDataPacket(pktID);

				if (pkt.pkContext != 0)
				{
					m_pkX = pkt.pkX;
					m_pkY = pkt.pkY;
					m_pressure = pkt.pkNormalPressure;

#if TRACE_PEN_DATA
					Trace.Write(
						"pkX,PkY: [" + m_pkX + "," + m_pkY + "]; ");
#endif

					m_pkTime = pkt.pkTime;

					// m_pkX and m_pkY are in screen (system) coordinates.
					Point clientPoint = fingerPanel.PointToClient(new Point(m_pkX, m_pkY));

#if TRACE_PEN_DATA
					Trace.WriteLine(
						"clientPoint: [" + clientPoint.X + "," + clientPoint.Y + "]");
#endif

					if (m_lastPoint.Equals(Point.Empty))
					{
						m_lastPoint = clientPoint;
						m_pkTimeLast = m_pkTime;
					}

					m_pen.Width = (int)(Math.Floor(10 * (double)m_pressure / m_maxPressure));

					//Trace.WriteLine("pressure: " + m_pressure + ", penwidth: " + m_pen.Width);

					if (m_pen.Width > 0)
					{
						m_graphics.DrawLine(m_pen, clientPoint, m_lastPoint);
					}

					m_lastPoint = clientPoint;
					m_pkTimeLast = m_pkTime;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("FAILED to get packet data: " + ex.ToString());
			}
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Open a Wintab system context.
		/// </summary>
		/// <param name="width_I"></param>
		/// <param name="height_I"></param>
		/// <param name="ctrlSysCursor"></param>
		/// <returns></returns>
		private CWintabContext OpenSystemContext(
			 int width_I = m_TABEXTX, int height_I = m_TABEXTY, bool ctrlSysCursor = true)
		{
			bool status = false;
			CWintabContext logContext = null;

			try
			{
				// Get the default system context.
				// Default is to receive data events.
				//logContext = CWintabInfo.GetDefaultDigitizingContext(ECTXOptionValues.CXO_MESSAGES);
				logContext = CWintabInfo.GetDefaultSystemContext(ECTXOptionValues.CXO_MESSAGES);

				// Set system cursor if caller wants it.
				if (ctrlSysCursor)
				{
					logContext.Options |= (uint)ECTXOptionValues.CXO_SYSTEM;
				}
				else
				{
					logContext.Options &= ~(uint)ECTXOptionValues.CXO_SYSTEM;
				}

				if (logContext == null)
				{
					Trace.WriteLine("FAILED to get default digitizing context.");
					return null;
				}

				// Modify the digitizing region.
				logContext.Name = "WintabDN Event Data Context";

				WintabAxis tabletX = CWintabInfo.GetTabletAxis(EAxisDimension.AXIS_X);
				WintabAxis tabletY = CWintabInfo.GetTabletAxis(EAxisDimension.AXIS_Y);

				logContext.InOrgX = 0;
				logContext.InOrgY = 0;
				logContext.InExtX = tabletX.axMax;
				logContext.InExtY = tabletY.axMax;

				logContext.OutOrgX = (int)System.Windows.Forms.SystemInformation.VirtualScreen.Left;
				logContext.OutOrgY = (int)System.Windows.Forms.SystemInformation.VirtualScreen.Top;
				logContext.OutExtX = (int)System.Windows.Forms.SystemInformation.VirtualScreen.Width;

				// In Wintab, the tablet origin is lower left.  Move origin to upper left
				// so that it coincides with screen origin.
				logContext.OutExtY = -(int)System.Windows.Forms.SystemInformation.VirtualScreen.Height;

				// Open the context, which will also tell Wintab to send data packets.
				status = logContext.Open();

				Trace.WriteLine("Context Open: " + (status ? "PASSED [ctx=" + logContext.HCtx + "]" : "FAILED"));
			}
			catch (Exception ex)
			{
				Trace.WriteLine("OpenTestDigitizerContext ERROR: " + ex.ToString());
			}

			return logContext;
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Remove the Wintab scribble context
		/// </summary>
		private void CloseCurrentContext()
		{
			try
			{
				if (m_logContext != null)
				{
					m_logContext.Close();
					m_logContext = null;
					m_wtData = null;
				}

			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.ToString());
			}
		}

		/////////////////////////////////////////////////////////////////////////
		private void ClearDisplay()
		{
			fingerPanel.Invalidate();
		}

		/////////////////////////////////////////////////////////////////////////
		// App event handlers
		//

		/// <summary>
		/// Respond to system messages.  Used to hook WM_FINGERDATA messages.
		/// </summary>
		/// <param name="message_I">System message.</param>
		[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
		protected override void WndProc(ref Message message_I)
		{
			switch (message_I.Msg)
			{
				// MTAPI sends this event when finger data is available for the
				// hwnd associated with this form.
				case WacomMTConstants.WM_FINGERDATA:
				{
					try
					{
						WacomMTFingerCollection fingerCollection =
							WacomMTDN.CMemUtils.PtrToStructure<WacomMTFingerCollection>((IntPtr)message_I.LParam);

						//Trace.WriteLine("WM_FINGERDATA: numFingers = " + fingerCollection.FingerCount);

						DoFingerDataUpdateUI(ref fingerCollection);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.ToString());
					}
				}
				break;

				case WacomMTConstants.WM_BLOBDATA:
				{
					try
					{
						WacomMTBlobAggregate blobData =
							WacomMTDN.CMemUtils.PtrToStructure<WacomMTBlobAggregate>((IntPtr)message_I.LParam);

						DoBlobDataUpdateUI(ref blobData);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.ToString());
					}
				}
				break;
			}
			base.WndProc(ref message_I);
		}

		/////////////////////////////////////////////////////////////////////////
		private void TestForm_Load(object sender, EventArgs e)
		{
			IntPtr userDataBuf = IntPtr.Zero;

			try
			{
				WacomMTError status = WacomMTError.WMTErrorSuccess;

				// Initialize MTAPI and client.
				// Will do the following:
				// - Initialize MTAPI
				// - Find/cache any connected touch device capabilities
				mWacomMTConfig.Init();

				// Register the Attach callback (used for all clients)
				mAttachCallback = new WacomMTAttachCallback(this.DoAttachWindowClientCallback);
				status = CWacomMTInterface.WacomMTRegisterAttachCallback(mAttachCallback, IntPtr.Zero);

				if (status != WacomMTError.WMTErrorSuccess)
				{
					throw new Exception("Failed to register for device attaches - err: " + status.ToString());
				}

				// Register the Detach callback (used for all clients)
				mDetachCallback = new WacomMTDetachCallback(this.DoDetachWindowClientCallback);
				status = CWacomMTInterface.WacomMTRegisterDetachCallback(mDetachCallback, IntPtr.Zero);

				if (status != WacomMTError.WMTErrorSuccess)
				{
					throw new Exception("Failed to register for device detaches - err: " + status.ToString());
				}

				// Updates the deviceID combo.
				bool touchDevicesAttached = UpdateUIDeviceIDList();

				// UI will disable controls if no touch devices attached.
				UpdateUIControls(touchDevicesAttached);

				if (touchDevicesAttached)
				{
					// Making this selection will cause a client to be created.
					this.DeviceIDComboBox.SelectedIndex = 0;
				}

				// Create the passthrough client for later use.
				mWacomMTPassthroughWindowClient =
					new CWacomMTFingerClient(WacomMTProcessingMode.WMTProcessingModePassThrough);

				// Load Wintab so that we can use the pen.
				// Let the user scribble even if no touch devices attached.
				InitWintab();

#if TRACE_SYSTEM_INFO
				{
					Graphics panelGraphics = fingerPanel.CreateGraphics();
					Trace.WriteLine("dpiX/dpiY: " + panelGraphics.DpiX + ", " + panelGraphics.DpiY);
				}
#endif
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/////////////////////////////////////////////////////////////////////////
		private void TestForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				// Remove our client (if one created).
				UnregisterWindowClient();

				if (mWacomMTConfig != null)
				{
					// Close the WacomMT connection.
					mWacomMTConfig.Quit();
					mWacomMTConfig = null;
				}

				if (mUserDataIntPtr != IntPtr.Zero)
				{
					WacomMTDN.CMemUtils.FreeUnmanagedString(mUserDataIntPtr);
					mUserDataIntPtr = IntPtr.Zero;
				}

				if (m_graphics != null)
				{
					m_graphics.Dispose();
					m_graphics = null;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/////////////////////////////////////////////////////////////////////////
		private void TestForm_Resize(object sender, EventArgs e)
		{
			UpdateWindowClientRegistration();
			if (m_graphics != null)
			{
				m_graphics.Dispose();
				m_graphics = fingerPanel.CreateGraphics();
				m_graphics.SmoothingMode = SmoothingMode.AntiAlias;
			}
		}

		/////////////////////////////////////////////////////////////////////////
		private void TestForm_Move(object sender, EventArgs e)
		{
			UpdateWindowClientRegistration();
		}

		/////////////////////////////////////////////////////////////////////////
		private void mClearButton_MouseClick(object sender, MouseEventArgs e)
		{
			ClearDisplay();
		}

		/////////////////////////////////////////////////////////////////////////
		private void DeviceCapabilitiesForm_Button_Click(object sender, EventArgs e)
		{
			try
			{
				if (mCurrentDeviceID == -1)
				{
					throw new Exception("No device selected");
				}

				WacomMTDN.HWND hwnd = IntPtr.Zero;
				Int32 dlgLocationX = 0;
				Int32 dlgLocationY = 0;

				// Show a form with the current device capabilities listed.
				DevCapsForm devCapsForm = new DevCapsForm();

				// Register the passthrough client to this window so the user
				// can push the "OK" button with his finger.
				hwnd = devCapsForm.Handle;
				if (hwnd.value == IntPtr.Zero)
				{
					throw new Exception("Could not get handle to caps dialog.");
				}

				mWacomMTPassthroughWindowClient.RegisterHWNDClient(mCurrentDeviceID, hwnd);

				devCapsForm.DevCapsText = mWacomMTConfig.GetDeviceCapsString(mCurrentDeviceID);

				dlgLocationX = this.Location.X + (this.Width - devCapsForm.Width) / 2;
				dlgLocationY = this.Location.Y + (this.Height - devCapsForm.Height) / 2;

				devCapsForm.SetDesktopLocation(dlgLocationX, dlgLocationY);

				devCapsForm.ShowDialog();

				mWacomMTPassthroughWindowClient.UnregisterHWNDClient();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/////////////////////////////////////////////////////////////////////////
		private void ClientModeRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			try
			{

				WacomMTProcessingMode newMode;

				if (sender == this.ClientModeConsumerRadioButton &&
									this.ClientModeConsumerRadioButton.Checked)
				{
					newMode = WacomMTConstants.CONSUMER_CLIENT;
				}
				else if (sender == this.ClientModeObserverRadioButton &&
											this.ClientModeObserverRadioButton.Checked)
				{
					newMode = WacomMTConstants.OBSERVER_CLIENT;
				}
				else
				{
					return;	// ignore unchecked states
				}

				mClientProcessingMode = newMode;

				if (mWacomMTWindowClient != null)
				{
					CreateNewClient(mWacomMTWindowClient.DeviceID, newMode);
				}

				UpdateUIStatus();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/////////////////////////////////////////////////////////////////////////
		private void FingerDataFromRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (mWacomMTWindowClient != null || mWacomMTRawClient != null)
				{
					bool useHitRect = true;
					TouchDataType touchDataType = TouchDataType.Unknown;

					if (sender == this.FingerDataFromCallbackRadioButton && 
									  this.FingerDataFromCallbackRadioButton.Checked)
					{ useHitRect = true; touchDataType = TouchDataType.Finger; }

					else if (sender == this.FingerDataFromEventRadioButton && 
											 this.FingerDataFromEventRadioButton.Checked)
					{ useHitRect = false; touchDataType = TouchDataType.Finger; }

					else if (sender == this.BlobDataFromCallbackRadioButton && 
											 this.BlobDataFromCallbackRadioButton.Checked)
					{ useHitRect = true; touchDataType = TouchDataType.Blob; }

					else if (sender == this.BlobDataFromEventRadioButton && 
											 this.BlobDataFromEventRadioButton.Checked)
					{ useHitRect = false; touchDataType = TouchDataType.Blob; }

					else if (sender == this.RawDataFromCallbackRadioButton && 
											 this.RawDataFromCallbackRadioButton.Checked)
					{ useHitRect = false; touchDataType = TouchDataType.Raw; }

					else { return;	/* ignore unchecked states */ }

					// This needs to happen before setting new mode.
					UnregisterWindowClient();

					mRegisterClientByHitRect = useHitRect;
					mTouchDataType = touchDataType;

					CreateNewClient(mCurrentDeviceID, mClientProcessingMode);
					UpdateUIStatus();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/////////////////////////////////////////////////////////////////////////
		private void ConfidenceBitsRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (sender == this.ConfidenceBitsEnabledRadioButton)
				{
					mCheckFingerConfidence = true;
				}
				else if (sender == this.ConfidenceBitsDisabledRadioButton)
				{
					mCheckFingerConfidence = false;
				}
				else
				{
					throw new Exception("Oops - unknown object");
				}

				UpdateUIStatusBarConfidenceBitsEnabled();
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates new MTAPI client and updates UI when a new device is selected.
		/// If there is an existing MTAPI client, it will be unregistered before 
		/// creating the new one.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeviceIDComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			Int32 idx = this.DeviceIDComboBox.SelectedIndex;

			try
			{
				mCurrentDeviceID = mWacomMTConfig.GetAttachedDeviceID(idx);
				CreateNewClient(mCurrentDeviceID, mClientProcessingMode);

				UpdateUIControls(true);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/// <summary>
		/// Common handler for keyboard input.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CommonHandler_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Use Escape key to clear the display.
			if (e.KeyChar == Convert.ToChar(Keys.Escape))
			{
				this.ClearDisplay();
			}
		}
	}
}

/////////////////////////////////////////////////////////////////////////
