/*----------------------------------------------------------------------------s
	NAME
		CWacomMTInterface.cs

	PURPOSE
		This demo shows how to use Wacom Feel Multi-Touch in a .NET application
		to detect finger input

	COPYRIGHT
		This file is Copyright (c) Wacom Company, Ltd. 2020

		The text and information contained in this file may be freely used,
		copied, or distributed without compensation or licensing restrictions.
---------------------------------------------------------------------------- */

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


// re: WacomMultiTouchTypes.h

namespace WacomMTDN
{
	using P_HWND = System.IntPtr;

	////////////////////////////////////////////////////////////////////////////
	// Type definitions
	//

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Managed implementation of Wintab HWND typedef. 
	/// Holds native Window handle.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct HWND
	{
		// \cond IGNORED_BY_DOXYGEN
		[MarshalAs(UnmanagedType.I4)]
		public IntPtr value;

		public HWND(IntPtr value)
		{ this.value = value; }

		public static implicit operator IntPtr(HWND hwnd_I)
		{ return hwnd_I.value; }

		public static implicit operator HWND(IntPtr ptr_I)
		{ return new HWND(ptr_I); }

		public static bool operator ==(HWND hwnd1, HWND hwnd2)
		{ return hwnd1.value == hwnd2.value; }

		public static bool operator !=(HWND hwnd1, HWND hwnd2)
		{ return hwnd1.value != hwnd2.value; }

		public override bool Equals(object obj)
		{ return (HWND)obj == this; }

		public override int GetHashCode()
		{ return 0; }
		// \endcond IGNORED_BY_DOXYGEN
	}



	////////////////////////////////////////////////////////////////////////////
	// Enums
	//

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Error/status conditions used as a return value for all functions.
	/// </summary>
	public enum WacomMTError
	{
		/// <summary>
		/// Returned when a call is successful.
		/// </summary>
		WMTErrorSuccess				= 0,

		/// <summary>
		/// Returned when the API does not find an installed and running driver.
		/// </summary>
		WMTErrorDriverNotFound		= 1,

		/// <summary>
		/// Returned when the version of the driver is not compatible with the API.  
		/// This will happen when an application requests newer API data structures 
		/// that are not supported by the driver.
		/// </summary>
		WMTErrorBadVersion			= 2,

		/// <summary>
		/// Returned when an application requests API data structures that are no longer 
		/// supported by the driver.
		/// </summary>
		WMTErrorAPIOutdated			= 3,

		/// <summary>
		/// Returned when one or more parameters are invalid.
		/// </summary>
		WMTErrorInvalidParam			= 4,

		/// <summary>
		/// Returned by wait functions when the API quit call is made or if the API has not been successfully initialized.
		/// </summary>
		WMTErrorQuit					= 5,

		/// <summary>
		/// Returned if the supplied buffer is too small.
		/// </summary>
		WMTErrorBufferTooSmall		= 6
	};



	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Type of touch sensor device.
	/// Used in the capabilities data structure.
	/// </summary>
	public enum WacomMTDeviceType
	{
		/// <summary>
		/// The touch sensor is not integrated with a display.  
		/// Opaque track pad like devices return this value.
		/// </summary>
		WMTDeviceTypeOpaque			= 0,

		/// <summary>
		/// The touch sensor is integrated with a display.  
		/// On screen touch devices, such as Cintiq 24HDT, return this value.
		/// </summary>
		WMTDeviceTypeIntegrated		= 1
	};



	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Flags used in the capabilities structure to indicate the type of data supported by the device.
	/// </summary>
	public enum WacomMTCapabilityFlags
	{
		/// <summary>
		/// If this flag is set, the device supports raw data and raw data can be read.
		/// </summary>
		WMTCapabilityFlagsRawAvailable = (1 << 0),

		/// <summary>
		/// If this flag is set, the device supports blob data and blob data can be read.
		/// </summary>
		WMTCapabilityFlagsBlobAvailable = (1 << 1),

		/// <summary>
		/// If this flag is set, sensitivity data will be available in the finger data.  
		/// Sensitivity data is a value between 0 and 0xFFFF.  If this flag is not set, 
		/// sensitivity data will be set to zero in each up packet and max for the down/hold packets.  
		/// </summary>
		WMTCapabilityFlagsSensitivityAvailable = (1 << 2),

		/// <summary>
		/// Bits 3-31 are reserved for future use.
		/// </summary>
	};



	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Used in the finger structure to indicate the state of the finger.
	/// </summary>
	public enum WacomMTFingerState
	{
		/// <summary>
		/// A finger buffer may contain room for more than one contact.  Any extra unused 
		/// contact is set to "None".  Once a contact has passed through the "Up" state it 
		/// has the TouchState set to "None" to indicate that no further processing is needed.  
		/// Any other data included with this state is not valid.
		/// </summary>
		WMTFingerStateNone			= 0,

		/// <summary>
		/// Indicates initial finger contact.  First touch packet for a particular contact.
		/// </summary>
		WMTFingerStateDown			= 1,

		/// <summary>
		/// Subsequent packets during the finger contact.
		/// </summary>
		WMTFingerStateHold			= 2,

		/// <summary>
		/// Last touch packet for a particular finger contact.  Reported when the finger 
		/// is lifted.  This will be reported at the last known valid position.
		/// </summary>
		WMTFingerStateUp				= 3
	};

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Used by the blob structure to identify the blob type.
	/// </summary>
	public enum WacomMTBlobType
	{
		/// <summary>
		/// This is an outline of an outer primary blob.
		/// </summary>
		WMTBlobTypePrimary = 0,

		/// <summary>
		/// This is an outline of an inner blob that is contained within an outer 
		/// primary blob. Void blobs are regions of no touch within primary blobs. 
		/// A primary blob may contain one or more void blobs. Void blobs do not 
		/// contain other blobs.  A unique primary blob can be within another blobs 
		/// void but the void does not reference that blob.
		/// </summary>
		WMTBlobTypeVoid = 1
	}


	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Used by the callback functions.  Provides instructions for how to process the data.  
	/// If no flags are set the data is sent to the callback and not processed by the system.
	/// </summary>
	public enum WacomMTProcessingMode
	{
		/// <summary>
		/// Touch data is sent only to the callback and no other processing is done.
		/// </summary>
		WMTProcessingModeConsumer			= 0,

		/// <summary>
		/// Touch data is consumed and also posted in parallel to the OS for processing.
		/// </summary>
		WMTProcessingModeObserver			= (1 << 0),

		/// <summary>
		/// Touch data is just passed on to the OS for processing.
		/// </summary>
		WMTProcessingModePassThrough		= (1 << 1),

		/// <summary>
		/// Bits 3-31 are reserved for future use.
		/// </summary>
	} ;



	////////////////////////////////////////////////////////////////////////////
	// Structs
	//

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This structure contains the data for an individual touch contact.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct WacomMTFinger
	{
		/// <summary>
		/// Unique identifier of the contact.  This value starts with 1 for the 
		/// first contact and increments for each subsequent contact.  This value 
		/// resets to 1 when all contacts are lifted up.  This is to be used to track 
		/// contacts from frame to frame.  This does not represent a unique value for 
		/// a specific finger but is unique for the contact and represents the same 
		/// contact point for the duration of the contact.
		/// </summary>
		public Int32			FingerID;

		/// <summary>
		/// Scaled X of the contact area in logical units
		/// </summary>
		public float			X;

		/// <summary>
		/// Scaled Y of the contact area in logical units.
		/// </summary>
		public float			Y;

		/// <summary>
		/// Width of the contact area in logical units.
		/// </summary>
		public float			Width;

		/// <summary>
		/// Height of the contact area in logical units.
		/// </summary>
		public float			Height;

		/// <summary>
		/// Strength of the contact.  This is not pressure.  This is a device/user 
		/// specific indication of the strength of the contact point.  Only valid 
		/// in relation to other fingers within the same frame/gesture.
		/// </summary>
		public ushort			Sensitivity;

		/// <summary>
		/// The orientation of the contact point in degrees.
		/// </summary>
		public float			Orientation;

		/// <summary>
		/// If true the driver believes this is a valid touch from a finger.  
		/// If false the driver thinks this may be an accidental touch, forearm or palm.
		/// </summary>
		public bool				Confidence;

		/// <summary>
		/// The state of this finger contact.  
		/// See the WacomMTFingerState enumeration definition.
		/// </summary>
		public WacomMTFingerState	TouchState;
	};



	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This structure allows for a list of fingers.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct WacomMTFingerCollection
	{
		/// <summary>
		/// The version of this data structure.
		/// </summary>
		public UInt32 Version;

		/// <summary>
		/// A value that identifies a touch device.  This is a unique number 
		/// that may vary from machine to machine and session to session, but 
		/// will be the same during any given session.
		/// </summary>
		public UInt32 DeviceID;

		/// <summary>
		/// Numbers indicating an ordered sequence of touch data packets.
		/// </summary>
		public UInt32 FrameNumber;

		/// <summary>
		/// The number of elements in the finger data array.  This value will 
		/// vary from frame to frame but will never be greater than the FingerMax 
		/// value for the specific device.
		/// </summary>
		public UInt32 FingerCount;

		/// <summary>
		/// Pointer to an array of fingers.  The size of the FingerData block is 
		/// FingerCount * sizeof(WacomMTFinger).
		/// </summary>
		public IntPtr fingerArray;	/* WacomMTFinger[] */

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns finger data for the given index.
		/// </summary>
		/// <param name="index_I">Finger index (0,1,...,FingerCount-1)</param>
		/// <returns>A WacomMTFinger object</returns>
		public WacomMTFinger GetFingerByIndex(UInt32 index_I)
		{
			WacomMTFinger finger = new WacomMTFinger();

			try
			{
				if (index_I >= FingerCount)
				{
					throw new Exception("Finger index: " + index_I.ToString() + " is invalid");
				}

				int offset = (Int32)(index_I * Marshal.SizeOf(finger));

				finger = (WacomMTFinger)Marshal.PtrToStructure(
					IntPtr.Add(fingerArray, offset), typeof(WacomMTFinger));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed GetFingerByIndex: " + ex.ToString());
			}

			return finger;
		}
	} // end struct WacomMTFingerCollection



	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This structure contains information about a specific point of blob data.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct WacomMTBlobPoint
	{
		/// <summary>
		/// Scaled X value of a blob point in logical units.
		/// </summary>
		public float	X;

		/// <summary>
		/// Scaled Y value of a blob point in logical units.
		/// </summary>
		public float	Y;

		/// <summary>
		/// Strength of the signal at this blob point.  This is not pressure.  
		/// This is a device/user specific indication of the strength of the 
		/// contact point. Only valid in relation to other blobs within the 
		/// same frame.
		/// </summary>
		public ushort	Sensitivity;
	} // end struct WacomMTBlobPoint

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This structure contains the contact data for an irregular region.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct WacomMTBlob
	{
		/// <summary>
		/// This is a value that uniquely identifies this blob.  This 
		/// value persists from frame to frame.
		/// </summary>
		public UInt32 BlobID;

		/// <summary>
		/// Scaled X center of gravity of the blob area in logical units.
		/// </summary>
		public float X;

		/// <summary>
		/// Scaled Y center of gravity of the blob area in logical units.
		/// </summary>
		public float Y;

		/// <summary>
		/// If true the driver believes this is a valid touch.  If false 
		/// the driver thinks this may be an accidental touch, forearm or 
		/// palm.
		/// </summary>
		public bool Confidence;

		/// <summary>
		/// The blob type this structure represents.  See the 
		/// WacomMTBlobType enumeration definition.
		/// </summary>
		public WacomMTBlobType BlobType;

		/// <summary>
		/// This identifies the parent blob.  Valid only if the BlobType 
		/// is "Void".
		/// </summary>
		public UInt32 ParentID;

		/// <summary>
		/// The number of elements in the blob points array.  This is the
		/// number of points that make up the outline of the blob.
		/// </summary>
		public UInt32 PointCount;

		/// <summary>
		/// Pointer to an array of blob points.  The size of the BlobPoints 
		/// block is PointCount * sizeof(WacomMTBlobPoint).  The blob points
		/// form a closed area.
		/// </summary>
		public IntPtr BlobPointsArray;	/* WacomMTBlobPoint[] */

		/// <summary>
		/// Returns a WacomMTBlobPoint for the given index.
		/// </summary>
		/// <param name="index_I">BlobPointIndex (0,1,...,PointCount-1)</param>
		/// <returns>A WacmMTBlobPoint object</returns>
		public WacomMTBlobPoint GetBlobPointByIndex(UInt32 index_I)
		{
			WacomMTBlobPoint blobPoint = new WacomMTBlobPoint();

			try
			{
				if (index_I >= PointCount)
				{
					throw new Exception("BlobPoint index: " + index_I.ToString() + " is invalid");
				}

				Int32 offset = (Int32)(index_I * Marshal.SizeOf(blobPoint));

				blobPoint = (WacomMTBlobPoint)Marshal.PtrToStructure(
					IntPtr.Add(BlobPointsArray, offset), typeof(WacomMTBlobPoint));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed GetBlobPointByIndex: " + ex.ToString());
			}

			return blobPoint;
		}

	}  // end struct WacomMTBlob

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This structure allows for a list of blobs.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct WacomMTBlobAggregate
	{
		/// <summary>
		/// The version of this data structure.
		/// </summary>
		public UInt32 Version;

		/// <summary>
		/// A value that identifies a touch device. This is a unique 
		/// number that may vary from machine to machine and session
		/// to session, but will be the same during any given session.
		/// </summary>
		public UInt32 DeviceID;

		/// <summary>
		/// Numbers indicating an ordered sequence of touch data packets.
		/// </summary>
		public UInt32 FrameNumber;

		/// <summary>
		/// Number of elements in the blob data array.
		/// </summary>
		public UInt32 BlobCount;

		/// <summary>
		/// An array of blobs. The size of the BlobArray block is 
		/// BlobCount * sizeof(WacomMTBlob).
		/// </summary>
		public IntPtr BlobArray;	/* WacomMTBlob[] */

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns blob data for the given index.
		/// </summary>
		/// <param name="index_I">Blob index (0,1,...,BlobCount-1)</param>
		/// <returns>A WacomMTBlob object</returns>
		public WacomMTBlob GetBlobByIndex(UInt32 index_I)
		{
			WacomMTBlob blob = new WacomMTBlob();

			try
			{
				if (index_I >= BlobCount)
				{
					throw new Exception("Blob index: " + index_I.ToString() + " is invalid");
				}

				Int32 offset = (Int32)(index_I * Marshal.SizeOf(blob));

				blob = (WacomMTBlob)Marshal.PtrToStructure(
					IntPtr.Add(BlobArray, offset), typeof(WacomMTBlob));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed GetBlobByIndex: " + ex.ToString());
			}

			return blob;
		}
	} // end struct WacomMTBlobAggregate

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This structure represents the strength values for the surface of the 
	/// device.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct WacomMTRawData
	{
		/// <summary>
		/// The version of this data structure.
		/// </summary>
		public UInt32	Version;

		/// <summary>
		/// A value that identifies a touch device.  This is a unique number 
		/// that may vary from machine to machine and session to session, but 
		/// will be the same during any given session.
		/// </summary>
		public UInt32	DeviceID;

		/// <summary>
		/// Numbers indicating an ordered sequence of touch data packets.
		/// </summary>
		public UInt32	FrameNumber;

		/// <summary>
		/// Number of elements in the sensitivity array. This value should be 
		/// ScanSizeY * ScanSizeX.
		/// </summary>
		public UInt32	ElementCount;

		/// <summary>
		/// Pointer to an array of sensitivity values. The size of the Sensitivity block 
		/// is ElementCount * sizeof(UInt16).  The location of each value of this array
		/// is calculated as (tabletY * ScanSizeX) + tabletX.
		/// </summary>
		public IntPtr	SensitivityArray;	/* UInt16[] */

		/// <summary>
		/// Returns raw sensitivity value for the given row and column indecies.
		/// See SensitivityArray for a description of how to access values in this array.
		/// Can be used only for tablets that support raw touch data.
		/// </summary>
		/// <param name="index_I">index into SensitivityArray</param>
		/// <returns>A tablet sensitivity value</returns>
		public UInt16 GetRawSensitivityByIndex(UInt32 index_I)
		{
			UInt16 sensitivityValue = 0;

			try
			{
				if (index_I >= ElementCount)
				{
					throw new Exception("Bad index value: " + index_I);
				}

				Int32 offset = (Int32)(index_I * sizeof(UInt16));

				sensitivityValue = (UInt16)Marshal.PtrToStructure(
					IntPtr.Add(SensitivityArray, offset), typeof(UInt16));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed GetRawSensitivityByIndex: " + ex.ToString());
			}

			return sensitivityValue;
		}
	} // endstruct WacomMTRawData

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This structure represents a hit test rectangle.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct WacomMTHitRect
	{
		/// <summary>
		/// The horizontal value of the origin point of the rectangle.
		/// </summary>
		public float			OriginX;

		/// <summary>
		/// The vertical value of the origin point of the rectangle.
		/// </summary>
		public float			OriginY;

		/// <summary>
		/// The horizontal width of the rectangle.
		/// </summary>
		public float			Width;

		/// <summary>
		/// The vertical height of the rectangle.
		/// </summary>
		public float			Height;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="originX_I">Left origin of hitrect (in logical units)</param>
		/// <param name="originY_I">Top origin of hitrect (in logical units)</param>
		/// <param name="width_I">Hitrect width (in logical units)</param>
		/// <param name="height_I">Hitrect height (in logical units)</param>
		public WacomMTHitRect(float originX_I, float originY_I, float width_I, float height_I)
		{ OriginX = originX_I; OriginY = originY_I; Width = width_I; Height = height_I; }
	}



	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This structure contains the physical and logical capabilities of a 
	/// multi-touch device. 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct WacomMTCapability
	{
		/// <summary>
		/// The version of this data structure.
		/// </summary>
		public UInt32 Version;

		/// <summary>
		/// A value that identifies a touch device.  This is a unique number that 
		/// may vary from machine to machine and session to session, but will be 
		/// the same during any given session.  This value is used with all other 
		/// calls to identify the device.
		/// </summary>
		public Int32 DeviceID;

		/// <summary>
		/// A value that indicates the device type.  An opaque device does not 
		/// have a fixed relationship with the screen.  An integrated device is 
		/// has a one to one relationship to a single monitor.
		/// </summary>
		public WacomMTDeviceType Type;

		/// <summary>
		/// Minimum horizontal value of the device reported after interpolation.  
		/// For an opaque device this value will be zero.  For integrated devices 
		/// this value is the (upper) left point in pixels of the mapped monitor 
		/// in relation to the entire desktop.  This may map to an adjacent monitor 
		/// if the touch device is larger than the integrated display.
		/// </summary>
		public float LogicalOriginX;

		/// <summary>
		/// Minimum vertical value of the device reported after interpolation.  
		/// For an opaque device this value will be zero.  For integrated devices 
		/// this value is the upper (left) point in pixels of the mapped monitor 
		/// in relation to the entire desktop.  This may map to an adjacent monitor 
		/// if the touch device is larger than the integrated display.
		/// </summary>
		public float LogicalOriginY;

		/// <summary>
		/// Width of the device reported after interpolation.  For an opaque device 
		/// this value is 1 and is unit neutral.  For integrated devices this value 
		/// is the number of pixels the device covers.  This may map to an adjacent 
		/// display if the touch device is larger than the integrated display.
		/// </summary>
		public float LogicalWidth;

		/// <summary>
		/// Height of the device reported after interpolation.  For an opaque device 
		/// this value is 1 and is unit neutral.  For integrated devices this value 
		/// is the number of pixels the device covers.  This may map to an adjacent 
		/// display if the touch device is larger than the integrated display.
		/// </summary>
		public float LogicalHeight;

		/// <summary>
		/// Width of the sensing area of the device in mm.  Used with other size 
		/// factors to convert the device data from one coordinate system to another.
		/// </summary>
		public float PhysicalSizeX;

		/// <summary>
		/// Height of the sensing area of the device in mm.  Used with other size 
		/// factors to convert the device data from one coordinate system to another.
		/// </summary>
		public float PhysicalSizeY;

		/// <summary>
		/// Width of the device in native counts.  Used with other size factors to 
		/// determine the maximum resolution of the data.  The horizontal resolution 
		/// is calculated by dividing this value by PhysicalSizeX.
		/// </summary>
		public UInt32 ReportedSizeX;

		/// <summary>
		/// Height of the device in native counts.  Used with other size factors to 
		/// determine the maximum resolution of the data.  The vertical resolution 
		/// is calculated by dividing this value by PhysicalSizeY.
		/// </summary>
		public UInt32 ReportedSizeY;

		/// <summary>
		/// Width of the device in scan coils.  This is only provided for devices 
		/// that support raw data.  This value is used to calculate the size of the 
		/// raw data buffer.  (ScanSizeX * ScanSizeY)  If the device does not support 
		/// raw data this value should be ignored.
		/// </summary>
		public UInt32 ScanSizeX;

		/// <summary>
		/// Height of the device in scan coils.  This is only provided for devices 
		/// that support raw data.  This value is used to calculate the size of the 
		/// raw data buffer.  (ScanSizeX * ScanSizeY)  If the device does not support 
		/// raw data this value should be ignored.
		/// </summary>
		public UInt32 ScanSizeY;

		/// <summary>
		/// Maximum number of fingers that are supported.  This is the number of 
		/// fingers this device can report as down at any given time.  The can be used 
		/// to help calculate the maximum size of the finger collection structure.  
		/// This is the not the maximum value for FingerID.
		/// </summary>
		public UInt32 FingerMax;

		/// <summary>
		/// Maximum number of blobs in a blob aggregate.  A blob is a series of 
		/// points that define its outline.  A blob can be a primary blob or a void blob.  
		/// Each void blob has one and only one parent while a primary blob can have zero
		/// or more child blobs which define void areas within the parent blob.  For example,
		/// a doughnut shape is a blob aggregate with the outer ring being the parent blob 
		/// and the inner ring being a child blob.
		/// </summary>
		public UInt32 BlobMax;

		/// <summary>
		/// Maximum number of blob points that make up a blob.  These blob values 
		/// (BlobMax and BlobPointsMax) can be used to calculate the maximum size of a 
		/// blob aggregate.  (BlobMax * BlobPointsMax)
		/// </summary>
		public UInt32 BlobPointsMax;

		/// <summary>
		/// A value to indicate the presence of specific data elements.  See the 
		/// WacomMTCapabilityFlags enumeration definition.
		/// </summary>
		public WacomMTCapabilityFlags CapabilityFlags;
	}



	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// An array of all attached device IDs.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct WacomMTDeviceIDArray
	{
		// \cond IGNORED_BY_DOXYGEN
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = WacomMTConstants.MAX_NUMBER_TOUCH_DEVICES * sizeof(Int32))]
		public Int32[] data;
		// \endcond IGNORED_BY_DOXYGEN
	}



	////////////////////////////////////////////////////////////////////////////
	// Callback delegate definitions
	//

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Callback profile used with WacomMTRegisterFingerReadCallback.
	/// This delegate is deprecated; use WacomMTCallback instead.
	/// </summary>
	/// <param name="fingerPacket_O">Finger data packet</param>
	/// <param name="userData_I">A parameter provided by the caller and echoed back 
	/// in the callback function.</param>
	/// <returns>Should always be zero</returns>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate UInt32 WacomMTFingerCallback(
		IntPtr fingerPacket_O,	/* WacomMTFingerCollection */
		IntPtr userData_I);

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Callback profile used with:
	///		WacomMTRegisterFingerReadCallback
	///		WacomMTRegisterBlobReadCallback
	/// </summary>
	/// <param name="fingerPacket_O">Finger data packet</param>
	/// <param name="userData_I">A parameter provided by the caller and echoed back 
	/// in the callback function.</param>
	/// <returns>Should always be zero</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt32 WacomMTCallback(
        IntPtr fingerPacket_O,
        IntPtr userData_I);

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Callback profile used with WacomMTRegisterAttachCallback.
	/// </summary>
	/// <param name="deviceCapability_O">Touch device capability structure</param>
	/// <param name="userData_I">Registered user data</param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void WacomMTAttachCallback(
		WacomMTCapability deviceCapability_O,
		IntPtr userData_I);

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Callback profile used with WacomMTRegisterDetachCallback.
	/// </summary>
	/// <param name="deviceID_I">Touch device identifier</param>
	/// <param name="userData_I">Registered user data</param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void WacomMTDetachCallback(
		Int32 deviceID_I, 
		IntPtr userData_I);

	//////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Globals used by this DLL and client code.
	/// </summary>
	public static class WacomMTConstants
	{
		public const int WACOM_MULTI_TOUCH_API_VERSION = 4;

		public const int MAX_NUMBER_FINGERS = 10;
		public const int MAX_NUMBER_TOUCH_DEVICES = 10;

		/// <summary>
		/// Custom Windows messages.
		/// </summary>
		public const int WM_FINGERDATA = 0x6205;
		public const int WM_BLOBDATA = 0x6206;
		public const int WM_RAWDATA = 0x6207;

		public const WacomMTProcessingMode CONSUMER_CLIENT = WacomMTProcessingMode.WMTProcessingModeConsumer;
		public const WacomMTProcessingMode OBSERVER_CLIENT = WacomMTProcessingMode.WMTProcessingModeObserver;
	}

	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Exported Multi-Touch API (MTAPI) interface functions.
	/// </summary>
	public class CWacomMTInterface
	{
		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function attempts to connect the application to the driver and 
		/// must be called successfully before any other Wacom Multi-Touch 
		/// function is called.
		/// </summary>
		/// <param name="mtVersion_I">This is the version of the API that the 
		/// application is using.  This value will be used by the API to 
		/// construct the expected data structures for the application.  
		/// Please use the provided predefined value 
		/// WacomMTConstants.WACOM_MULTI_TOUCH_API_VERSION</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto, 
			CallingConvention=CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTInitialize(
			Int32 mtVersion_I
		);

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function closes the connection to the driver service.  This 
		/// should be called when the application is closing or no longer needs 
		/// touch data.  After calling WacomMTQuit, WacomMTInitialize would need 
		/// to be called again to resume touch interaction.
		/// </summary>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto, 
			CallingConvention = CallingConvention.Cdecl)]
		public static extern void WacomMTQuit();

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function returns the number of multi-touch sensors attached to 
		/// the system.
		/// </summary>
		/// <param name="deviceArray_O">A user allocated buffer used to return the 
		/// deviceIDs of the currently attached devices.  This call will provide 
		/// as many devices as will fit into the provided buffer.  This can be 
		/// NULL.</param>
		/// <param name="bufferSize_I">The size of the buffer provided.  Should 
		/// be zero if no buffer provided.</param>
		/// <returns>The return value is the number of sensors attached to the system.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto, 
			CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 WacomMTGetAttachedDeviceIDs(
			IntPtr deviceArray_O,
			Int32 bufferSize_I
		);

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// his function fills in a caller allocated WacomMTCapability structure 
		/// with the capability information for the requested device identifier.
		/// </summary>
		/// <param name="deviceID_I">The ID of the device.  These IDs can be from 
		/// the GetAttachedDeviceIDs array, the finger packet deviceID member, 
		/// the blob packet deviceID member or the raw packet deviceID member.
		/// </param>
		/// <param name="capabilityBuffer_O">his is the caller allocated structure 
		/// that is filled in with device capability data upon success.  This 
		/// structure will be defined by the API version provided at 
		/// initialization.</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto, 
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTGetDeviceCapabilities(
			 Int32 deviceID_I,
			 IntPtr capabilityBuffer_O
		);

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to register a callback function that you would 
		/// like called when a finger touch packet is ready and within the requested 
		/// device's hit rectangle.  A process can create as many callbacks as 
		/// needed but only one callback per device hit rectangle.  If you wish to 
		/// cancel a callback, call this function on an existing device hit rectangle 
		/// with NULL for the function.  The callbacks are processed in the order in 
		/// which they are created.
		/// </summary>
		/// <param name="deviceID_I">The deviceID of the device.</param>
		/// <param name="hitRect_I">A rectangle used to hit test the data.  If a 
		/// finger begins contact with in the hit rectangle the callback will be called.
		/// The callback will continue to be called for that finger until that contact 
		/// is removed from the device.  If a contact begins outside the hit rectangle, 
		/// the callback will not be called for that finger.  A NULL rectangle will 
		/// assume the entire device surface.  The hit rectangle is defined in the 
		/// logical units of the device.  See WacomMTCapability structure for a 
		/// definition of logical units.</param>
		/// <param name="mode_I">Specifies what the API does with the data during 
		/// the callback.</param>
		/// <param name="fingerCallback_I">The function will be called with the fingerPacket 
		/// structure filled in and the userData provided when the call was registered.  
		/// The fingerPacket is only valid while the callback is being processed.  
		/// This memory is release/reused upon return from the callback.  The return 
		/// value is reserved and should be zero.</param>
		/// <param name="userData_I">A parameter provided by the caller and echoed back in 
		/// the callback function.</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto, 
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTRegisterFingerReadCallback(
			Int32 deviceID_I,
			ref WacomMTHitRect hitRect_I,
			WacomMTProcessingMode mode_I,
			[MarshalAs(UnmanagedType.FunctionPtr)] WacomMTFingerCallback fingerCallback_I,
			IntPtr userData_I
		);

        [DllImport("WacomMT.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern WacomMTError WacomMTRegisterFingerReadCallback(
            Int32 deviceID_I,
            ref WacomMTHitRect hitRect_I,
            WacomMTProcessingMode mode_I,
            [MarshalAs(UnmanagedType.FunctionPtr)] WacomMTCallback fingerCallback_I,
            IntPtr userData_I
        );

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to cancel a callback registered with 
		/// WacomMTRegisterFingerReadCallback
		/// </summary>
		/// <param name="deviceID_I">The deviceID of the device.</param>
		/// <param name="hitRect_I">A rectangle that was registered for callbacks. 
		/// A NULL rectangle will assume the entire device surface. The hit rectangle 
		/// is defined in the logical units of the device. See WacomMTCapability 
		/// structure for a definition of logical units.</param>
		/// <param name="mode_I">Specifies what the API does with the data during the 
		/// callback.</param>
		/// <param name="userData_I">A parameter provided by the caller and echoed back 
		/// in the callback function.</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTUnRegisterFingerReadCallback(
			Int32 deviceID_I,
			ref WacomMTHitRect hitRect_I,
			WacomMTProcessingMode mode_I,
			IntPtr userData_I
		);

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to register a new finger hitrect without having 
		/// to first explicitly unregister an old hitrect.
		/// </summary>
		/// <param name="deviceID_I">The deviceID of the device.</param>
		/// <param name="oldHitRect_I">A rectangle that was registered for callbacks. 
		/// A NULL rectangle will assume the entire device surface. The hit rectangle is 
		/// defined in the logical units of the device. See WacomMTCapability structure 
		/// for a definition of logical units.</param>
		/// <param name="mode_I">Specifies what the API does with the data during the 
		/// callback.</param>
		/// <param name="newHitRect_I">A new rectangle to be registered for callbacks. 
		/// A NULL rectangle will assume the entire device surface.  The hit rectangle is 
		/// defined in the logical units of the device. See WacomMTCapability structure 
		/// for a definition of logical units.</param>
		/// <param name="userData_I">A parameter provided by the caller and echoed back 
		/// in the callback function.
		/// </param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTMoveRegisteredFingerReadCallback(
			Int32 deviceID_I,
			ref WacomMTHitRect oldHitRect_I,
			WacomMTProcessingMode mode_I,
			ref WacomMTHitRect newHitRect_I,
			IntPtr userData_I
		);

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to register a window handle to receive finger data.  
		/// When a packet of finger data for the given device is ready a WM_FINGERDATA 
		/// message will be sent to the window handle.  The lParam parameter will be a 
		/// pointer to a WacomMTFingerCollection structure that contains the finger data.
		/// The bufferDepth parameter determines the number of callback buffers created.
		/// The buffers are used in a ring format.  Only one finger data callback is 
		/// allowed for each device per window handle.  The window handle is also used 
		/// to produce a hit rectangle.  The hit rectangle is calculated when this 
		/// function is called.  If the window is moved or resized this function should 
		/// be called again.  If the bufferDepth is changed, all existing buffers will 
		/// be invalid.  To cancel the device callback this function should be called 
		/// with the bufferDepth of zero.
		/// </summary>
		/// <param name="deviceID_I">The deviceID of the device.</param>
		/// <param name="mode_I">Specifies what the API does with the data during 
		/// the callback.</param>
		/// <param name="hwnd_I">This is the window handle that will receive the 
		/// WM_FINGERDATA message.</param>
		/// <param name="bufferDepth_I">This is the number of WacomMTFingerCollection 
		/// data structures that will be allocated for the message callbacks.</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto, 
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTRegisterFingerReadHWND(
			Int32 deviceID_I,
			WacomMTProcessingMode mode_I,
			P_HWND hwnd_I,
			Int32 bufferDepth_I
		);

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to unregister a Window handle from receiving 
		/// touch finger data updates. The function will return an error for an 
		/// invalid Window handle.
		/// </summary>
		/// <param name="hwnd_I">This is a window handle that was registered to
		/// receive the WM_FINGERDATA message.
		/// </param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTUnRegisterFingerReadHWND(
			P_HWND hwnd_I
		);

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to register a callback function that you would 
		/// like called when a new touch device is attached.  When registered the 
		/// API will issue a callback for each device currently attached.  Only one 
		/// attach callback can be registered for each process.  To cancel the 
		/// attach callback, a process can call this function with NULL.
		/// </summary>
		/// <param name="attachCallback_I">This function will be called with the 
		/// WacomMTCapability structure for the device attached and the userData 
		/// provided when the call was registered.</param>
		/// <param name="userData_I">User-specified data to be sent back to client</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto, 
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTRegisterAttachCallback(
			[MarshalAs(UnmanagedType.FunctionPtr)] WacomMTAttachCallback attachCallback_I,
			IntPtr userData_I
		);

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to register a callback function that you would 
		/// like called when a touch device is detached. Only one detach callback 
		/// can be registered for each process.  To cancel the detach callback, a 
		/// process can call this function with NULL.
		/// </summary>
		/// <param name="detachCallback_I">This function will be called with the 
		/// WacomMTCapability structure for the device detached and the userData 
		/// provided when the call was registered.</param>
		/// <param name="userData_I">User-specified data to be sent back to client</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto, 
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTRegisterDetachCallback(
			[MarshalAs(UnmanagedType.FunctionPtr)] WacomMTDetachCallback detachCallback_I,
			IntPtr userData_I
		);

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to register a callback function that you would like 
		/// called when blob data is ready and within the requested device's hit rectangle. 
		/// A process can create as many callbacks as needed but only one callback per 
		/// device hit rectangle. If you wish to cancel a callback, call this function on 
		/// an existing device hit rectangle with NULL for the function. The callbacks are 
		/// processed in the order in which they are created.
		/// </summary>
		/// <param name="deviceID_I">The deviceID of the device.</param>
		/// <param name="hitRect_I">A rectangle used to hit test the data. If any part of 
		/// the blob contact is within the hit rectangle the callback will be called. A NULL 
		/// rectangle will assume the entire device surface. The hit rectangle is defined 
		/// in the logical units of the device. See WacomMTCapability structure for a 
		/// efinition of logical units.</param>
		/// <param name="mode_I">Specifies what the API does with the data during the 
		/// callback.</param>
		/// <param name="blobCallback_I">This function will be called with the blobPacket 
		/// structure filled in and the userData provided when the call was registered. The 
		/// blobPacket is only valid while the callback is being processed. This memory is 
		/// release/reused upon return from the callback. The return value is reserved and 
		/// should be zero.</param>
		/// <param name="userData_I">A parameter provided by the caller and echoed back in 
		/// the callback function.</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTRegisterBlobReadCallback(
			Int32 deviceID_I, 
			ref WacomMTHitRect hitRect_I,
			WacomMTProcessingMode mode_I,
			[MarshalAs(UnmanagedType.FunctionPtr)] WacomMTCallback blobCallback_I,
			IntPtr userData_I
		);

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to cancel a callback registered with 
		/// WacomMTRegisterBlobReadCallback.
		/// </summary>
		/// <param name="deviceID_I">The deviceID of the device.</param>
		/// <param name="hitRect_I">A rectangle that was registered for callbacks. 
		/// A NULL rectangle will assume the entire device surface. The hit rectangle 
		/// is defined in the logical units of the device. See WacomMTCapability 
		/// structure for a definition of logical units.</param>
		/// <param name="mode_I">Specifies what the API does with the data during the 
		/// callback.</param>
		/// <param name="userData_I">A parameter provided by the caller and echoed back 
		/// in the callback function.</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTUnRegisterBlobReadCallback(
			Int32 deviceID_I,
			ref WacomMTHitRect hitRect_I,
			WacomMTProcessingMode mode_I,
			IntPtr userData_I
		);

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to register a window handle to receive blob data. 
		/// When a blob packet for the given device is ready a WM_BLOBDATA message will 
		/// be sent to the window handle. The lParam parameter will be a pointer to a 
		/// WacomMTBlobAggregate structure that contains the blob data. The bufferDepth 
		/// parameter determines the number of callback buffers created. The buffers are 
		/// used in a ring format. Only one blob data callback is allowed for each device 
		/// per window handle. The window handle is also used to produce a hit rectangle. 
		/// The hit rectangle is calculated when this function is called. If the window 
		/// is moved or resized this function should be called again. If the bufferDepth 
		/// is changed, all existing buffers will be invalid. To cancel the device callback, 
		/// this function can be called with a bufferDepth of zero, or the function 
		/// WacomMTUnRegisterBlobReadHWND can be used.
		/// </summary>
		/// <param name="deviceID_I">The deviceID of the device.</param>
		/// <param name="mode_I">Specifies what the API does with the data during 
		/// the callback.</param>
		/// <param name="hwnd_I">This is the window handle that will receive the 
		/// WM_BLOBDATA message.</param>
		/// <param name="bufferDepth_I">This is the number of WacomMTBlobAggregate 
		/// data structures that will be allocated for the message callbacks.</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTRegisterBlobReadHWND(
			Int32 deviceID_I,
			WacomMTProcessingMode mode_I,
			P_HWND hwnd_I,
			Int32 bufferDepth_I
		);

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to unregister a Window handle from receiving 
		/// touch blob data updates. The function will return an error for an invalid 
		/// Window handle.
		/// </summary>
		/// <param name="hwnd_I">This is a window handle that was registered to receive 
		/// the WM_BLOBDATA message.</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTUnRegisterBlobReadHWND(
			P_HWND hwnd_I
		);

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to register a new blob hitrect without having 
		/// to first explicitly unregister an old hitrect.
		/// </summary>
		/// <param name="deviceID_I">The deviceID of the device.</param>
		/// <param name="oldHitRect_I">A rectangle that was registered for callbacks. 
		/// A NULL rectangle will assume the entire device surface. The hit rectangle is 
		/// defined in the logical units of the device. See WacomMTCapability structure 
		/// for a definition of logical units.</param>
		/// <param name="mode_I">Specifies what the API does with the data during the 
		/// callback.</param>
		/// <param name="newHitRect_I">A new rectangle to be registered for callbacks. 
		/// A NULL rectangle will assume the entire device surface.  The hit rectangle is 
		/// defined in the logical units of the device. See WacomMTCapability structure 
		/// for a definition of logical units.</param>
		/// <param name="userData_I">A parameter provided by the caller and echoed back 
		/// in the callback function.
		/// </param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTMoveRegisteredBlobReadCallback(
			Int32 deviceID_I,
			ref WacomMTHitRect oldHitRect_I,
			WacomMTProcessingMode mode_I,
			ref WacomMTHitRect newHitRect_I,
			IntPtr userData_I
		);

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to register a callback function that you would like 
		/// called when raw data is ready for the specified device. Only one callback can 
		/// be registered per device. If you wish to cancel the device callback call this
		/// function with NULL for the callback. The callbacks are processed in the order 
		/// in which they are created.
		/// </summary>
		/// <param name="deviceID_I">The deviceID of the device.</param>
		/// <param name="mode_I">Specifies what the API does with the data during the 
		/// callback.</param>
		/// <param name="rawCallback_I">This function will be called with the rawPacket 
		/// structure filled in and the userData provided when the call was registered. 
		/// The rawPacket is only valid while the callback is being processed. This memory 
		/// is release/reused upon return from the callback. The return value is reserved 
		/// and should be zero.</param>
		/// <param name="userData_I">A parameter provided by the caller and echoed back 
		/// in the callback function.</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTRegisterRawReadCallback(
			Int32 deviceID_I,
			WacomMTProcessingMode mode_I,
			[MarshalAs(UnmanagedType.FunctionPtr)] WacomMTCallback rawCallback_I,
			IntPtr userData_I
		);

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function allows you to cancel a callback registered with 
		/// WacomMTRegisterRawReadCallback
		/// </summary>
		/// <param name="deviceID_I">The deviceID of the device.</param>
		/// <param name="mode_I">Specifies what the API does with the data during the 
		/// callback.</param>
		/// <param name="userData_I">A parameter provided by the caller and echoed back 
		/// in the callback function.</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTUnRegisterRawReadCallback(
			Int32 deviceID_I,
			WacomMTProcessingMode mode_I,
			IntPtr userData_I
		);

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// THIS FUNCTION IS DEPRECATED. 
		/// Use WacomMTRegisterRawReadCallback() instead.
		///
		/// This function allows you to register a window handle to receive 
		/// raw data. When a data frame for the given device is ready a 
		/// WM_RAWDATA message will be sent to the window handle. The 
		/// lParam parameter will be a pointer to a WacomMTRawData structure. 
		/// The bufferDepth parameter determines the number of callback buffers 
		/// created. The buffers are used in a ring format. Only one raw data 
		/// callback is allowed for each device per window handle. If the 
		/// bufferDepth is changed, all existing buffers will be invalid. 
		/// To cancel the device callback this function should be called 
		/// with the bufferDepth equal to zero.
		/// </summary>
		/// <param name="deviceID_I">The deviceID of the device.</param>
		/// <param name="mode_I">Specifies what the API does with the data during 
		/// the callback.</param>
		/// <param name="hwnd_I">This is the window handle that will receive the 
		/// WM_RAWDATA message.</param>
		/// <param name="bufferDepth_I">This is the number of WacomMTFingerCollection 
		/// data structures that will be allocated for the message callbacks.</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		[DllImport("WacomMT.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern WacomMTError WacomMTRegisterRawReadHWND(
			Int32 deviceID_I,
			WacomMTProcessingMode mode_I,
			P_HWND hwnd_I,
			Int32 bufferDepth_I
		);
	}
} // end namespace WacomMTDN



/////////////////////////////////////////////////////////////////////////
