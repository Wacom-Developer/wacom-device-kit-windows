/*----------------------------------------------------------------------------s
	NAME
		CWacomMTConfig.cs

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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WacomMTDN
{
	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class to initialize MTAPI and track connected touch devices.
	/// See http://www.wacomeng.com/touch/WacomFeelMulti-TouchAPI.htm for MTAPI details.
	/// </summary>
	public class CWacomMTConfig
	{
		/// <summary>
		/// Internal "typedef" for managing capabilities for multiple touch devices.
		/// Dictionary key = deviceID
		/// </summary>
		private class WacomMTCapabilityMap : Dictionary<Int32, WacomMTCapability> {}

		private const Int32 mMaxNumTouchDevices = WacomMTConstants.MAX_NUMBER_TOUCH_DEVICES;
		private Int32 mNumDevices = 0;
		private WacomMTDeviceIDArray mDeviceIDArray = new WacomMTDeviceIDArray();
		private WacomMTCapabilityMap mDeviceCapsMap = new WacomMTCapabilityMap();



		////////////////////////////////////////////////////////////////////////////
		public CWacomMTConfig()
		{
			// NOTE: do not allocate mDeviceIDArray.data as that will be done when
			// we query for touch devices.
		}



		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns the current list of attached touch device IDs.
		/// </summary>
		public WacomMTDeviceIDArray DeviceIDList
		{
			get { return mDeviceIDArray; }
		}


		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns object with device capabilities or exceptions if devID_I not valid.
		/// </summary>
		/// <param name="devID_I">Touch device identifier</param>
		/// <returns>Returns a WacomMTCapability object</returns>
		public WacomMTCapability GetDeviceCaps(Int32 devID_I)
		{
			if (mDeviceCapsMap.ContainsKey(devID_I))
			{
				return mDeviceCapsMap[devID_I];
			}
			else
			{
				throw new Exception("Device ID: " + devID_I + " not found");
			}
		}



		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns a string listing MTAPI capabilities for the specified device.
		/// </summary>
		/// <param name="devID_I">Touch device identifier</param>
		/// <returns>A newline-separated string of device capabilities</returns>
		public String GetDeviceCapsString(Int32 devID_I)
		{
			String devCapsStr = "";

			if (UpdateDeviceCaps())
			{

				if (mDeviceCapsMap.ContainsKey(devID_I))
				{
					WacomMTCapability devCaps = mDeviceCapsMap[devID_I];

					devCapsStr += "Capabilities for device: " + devID_I + "\n";
					devCapsStr += "  Version:         " + devCaps.Version + "\n";
					devCapsStr += "  DeviceID:        " + devCaps.DeviceID + "\n";
					devCapsStr += "  Type:            " + devCaps.Type + "\n";
					devCapsStr += "  LogOriginX:      " + devCaps.LogicalOriginX + "\n";
					devCapsStr += "  LogOriginY:      " + devCaps.LogicalOriginY + "\n";
					devCapsStr += "  LogWidth:        " + devCaps.LogicalWidth + "\n";
					devCapsStr += "  LogHeight:       " + devCaps.LogicalHeight + "\n";
					devCapsStr += "  PhysSizeX:       " + devCaps.PhysicalSizeX + "\n";
					devCapsStr += "  PhysSizeY:       " + devCaps.PhysicalSizeY + "\n";
					devCapsStr += "  ReportedSizeX:   " + devCaps.ReportedSizeX + "\n";
					devCapsStr += "  ReportedSizeY:   " + devCaps.ReportedSizeY + "\n";
					devCapsStr += "  ScanSizeX:       " + devCaps.ScanSizeX + "\n";
					devCapsStr += "  ScanSizeY:       " + devCaps.ScanSizeY + "\n";
					devCapsStr += "  FingerMax:       " + devCaps.FingerMax + "\n";
					devCapsStr += "  BlobMax:         " + devCaps.BlobMax + "\n";
					devCapsStr += "  BlobPointsMax:   " + devCaps.BlobPointsMax + "\n";

					{
						String rawAvail = (devCaps.CapabilityFlags & WacomMTCapabilityFlags.WMTCapabilityFlagsRawAvailable) > 0 ? "T" : "F";
						String blobAvail = (devCaps.CapabilityFlags & WacomMTCapabilityFlags.WMTCapabilityFlagsBlobAvailable) > 0 ? "T" : "F";
						String sensAvail = (devCaps.CapabilityFlags & WacomMTCapabilityFlags.WMTCapabilityFlagsSensitivityAvailable) > 0 ? "T" : "F";
						devCapsStr += "  CapabilityFlags: [Raw|Blob|Sensitivity] = [" + rawAvail + "|" + blobAvail + "|" + sensAvail + "]";
					}
				}
				else
				{
					devCapsStr = "No capabilities found for key: " + devID_I;
				}
			}

			return devCapsStr;
		}



		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Disconnect from MTAPI.
		/// </summary>
		public void Quit()
		{
			try
			{
				CWacomMTInterface.WacomMTQuit();
				mNumDevices = 0;

				// The GC should remove mDeviceIDArray.data.
				mDeviceIDArray.data = null;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}



		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns the number of attached MTAPI touch devices.
		/// </summary>
		/// <returns>The number of attached MTAPI touch devices</returns>
		public Int32 GetNumAttachedTouchDevices()
		{
			return mNumDevices;
		}



		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns the ID of the specified touch device.
		/// </summary>
		/// <param name="index_I">Touch device identifier</param>
		/// <returns>-1 if no devices found</returns>
		public Int32 GetAttachedDeviceID(Int32 index_I)
		{
			Int32 devID = -1;

			try
			{
				// Will exception if index_I exceeds array bounds.
				if (mDeviceIDArray.data != null && mDeviceIDArray.data.Length > index_I )
				{
					devID = mDeviceIDArray.data[index_I];
				}
				else
				{
					Trace.WriteLine("WacomMTDN: could not get device for index: " + index_I.ToString());
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			Trace.WriteLine("WacomMTDN: returning deviceID: " + devID.ToString());

			return devID;
		}



		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initialize MTAPI, find/cache connected devices
		/// and their capabilities.
		/// </summary>
		public void Init()
		{
			IntPtr devBuf = IntPtr.Zero;

			try
			{
				WacomMTError res = WacomMTError.WMTErrorSuccess;

				// ----------------------------------------------------------------
				// Initialize the MTAPI.
				//
				res = CWacomMTInterface.WacomMTInitialize(WacomMTConstants.WACOM_MULTI_TOUCH_API_VERSION);
				if (WacomMTError.WMTErrorSuccess != res)
				{
					throw new Exception("Oops - failed WacomMTInitialize - is the tablet touch driver running?");
				}
				Trace.WriteLine("WacomMTDN: WacomMTInitialize successful.");

				// ----------------------------------------------------------------
				// Find all attached touch devices.
				// If successful, this will populate mDeviceIDArray.data.
				//
				devBuf = CMemUtils.AllocUnmanagedBuf(mDeviceIDArray);
				Marshal.StructureToPtr(mDeviceIDArray, devBuf, false);

				if (mNumDevices > WacomMTConstants.MAX_NUMBER_TOUCH_DEVICES)
				{
					throw new Exception("Too many touch devices attached");
				}

				mNumDevices = CWacomMTInterface.WacomMTGetAttachedDeviceIDs(devBuf, mMaxNumTouchDevices * sizeof(Int32));

				if (mNumDevices > 0)
				{
					mDeviceIDArray = (WacomMTDeviceIDArray)Marshal.PtrToStructure(devBuf, typeof(WacomMTDeviceIDArray));
					DumpDeviceIDArray();
				}
				else
				{
					Trace.WriteLine("WacomMTDN: NO TOUCH DEVICES FOUND!");
				}

				CMemUtils.FreeUnmanagedBuf(ref devBuf); 

				// ----------------------------------------------------------------
				// Find capabilities for all attached touch devices.
				// If successful, this will populate mDeviceCapsMap.
				//
				if ((mNumDevices > 0) && !UpdateDeviceCaps())
				{
					throw new Exception("Oops - could not complete query for capabilities");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
			finally
			{
				if (devBuf != IntPtr.Zero)
				{
					CMemUtils.FreeUnmanagedBuf(ref devBuf);
				}
			}
		}



		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Adds touch device with the specified device capability to the current 
		/// list of attached devices.
		/// </summary>
		/// <param name="deviceCapability_I"></param>
		public void AddDevice(WacomMTCapability deviceCapability_I)
		{
			IntPtr devBuf = IntPtr.Zero;

			try
			{
				Int32 deviceID = deviceCapability_I.DeviceID;

				// Only add device if not already added.
				if (!mDeviceCapsMap.ContainsKey(deviceID))
				{
					mDeviceCapsMap[deviceID] = deviceCapability_I;

					// Make sure we're in sync with system's view of #attached devices.
					devBuf = CMemUtils.AllocUnmanagedBuf(mDeviceIDArray);
					Marshal.StructureToPtr(mDeviceIDArray, devBuf, false);

					if ( mNumDevices > WacomMTConstants.MAX_NUMBER_TOUCH_DEVICES )
					{
						throw new Exception("Too many touch devices attached");
					}

					mNumDevices = CWacomMTInterface.WacomMTGetAttachedDeviceIDs(devBuf, mMaxNumTouchDevices * sizeof(Int32));

					if (mNumDevices > 0)
					{
						mDeviceIDArray = (WacomMTDeviceIDArray)Marshal.PtrToStructure(devBuf, typeof(WacomMTDeviceIDArray));
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
			finally
			{
				CMemUtils.FreeUnmanagedBuf(ref devBuf);
			}
		}



		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Removes device of the specified ID.
		/// </summary>
		/// <param name="deviceID_I">Touch device identifier</param>
		public void RemoveDevice( Int32 deviceID_I )
		{
			IntPtr devBuf = IntPtr.Zero;

			try
			{
				if (mDeviceCapsMap.ContainsKey(deviceID_I))
				{
					mDeviceCapsMap.Remove(deviceID_I);

					// Make sure we're in sync with system's view of #attached devices.
					devBuf = CMemUtils.AllocUnmanagedBuf(mDeviceIDArray);
					Marshal.StructureToPtr(mDeviceIDArray, devBuf, false);

					mNumDevices = CWacomMTInterface.WacomMTGetAttachedDeviceIDs(devBuf, mMaxNumTouchDevices * sizeof(Int32));

					if (mNumDevices > 0)
					{
						mDeviceIDArray = (WacomMTDeviceIDArray)Marshal.PtrToStructure(devBuf, typeof(WacomMTDeviceIDArray));
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
			finally
			{
				CMemUtils.FreeUnmanagedBuf(ref devBuf);
			}
		}



		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Checks to see if device is a display tablet.
		/// </summary>
		/// <param name="deviceID_I">Touch device identifier</param>
		/// <returns>Returns true if device is display tablet.</returns>
		public bool IsDisplayTablet( Int32 deviceID_I )
		{
			bool retval = false;

			try
			{
				retval = mDeviceCapsMap[deviceID_I].Type == WacomMTDeviceType.WMTDeviceTypeIntegrated;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			return retval;
		}



		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Dump the contents of the mDeviceIDArray to debug output.
		/// </summary>
		public void DumpDeviceIDArray()
		{
			if (mDeviceIDArray.data != null && mNumDevices > 0)
			{
				Trace.WriteLine("WacomMTDN: number of touch devices found: " + mNumDevices.ToString());
				Trace.Write("List of device IDs:  ");

				for (int idx = 0; idx < mNumDevices; idx++)
				{
					Trace.Write(mDeviceIDArray.data[idx].ToString() + " ");
				}

				Trace.WriteLine("");
			}
			else
			{
				Trace.WriteLine("WacomMTDN: NO TOUCH DEVICES FOUND");
			}
		}



		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Dumps the capabilities of the specified device to debug OUT.
		/// </summary>
		/// <param name="deviceID_I">Touch device identifier</param>
		public void DumpDeviceCaps(Int32 deviceID_I)
		{
			Trace.WriteLine(GetDeviceCaps(deviceID_I));
		}


		/// <summary>
		/// Find capabilities for all attached touch devices.
		/// If successful, this will populate mDeviceCapsMap. 
		/// </summary>
		/// <returns> true if all device capabilities updated</returns>
		private bool UpdateDeviceCaps()
		{
			IntPtr devBuf = IntPtr.Zero;

			try
			{
				if (mNumDevices > 0)
				{
					for (int idx = 0; idx < mNumDevices; idx++)
					{
						Int32 devID = mDeviceIDArray.data[idx];
						WacomMTCapability devCaps = new WacomMTCapability();

						devBuf = CMemUtils.AllocUnmanagedBuf(devCaps);
						Marshal.StructureToPtr(devCaps, devBuf, false);

						if (WacomMTError.WMTErrorSuccess == CWacomMTInterface.WacomMTGetDeviceCapabilities(devID, devBuf))
						{
							devCaps = (WacomMTCapability)Marshal.PtrToStructure(devBuf, typeof(WacomMTCapability));
							mDeviceCapsMap[devID] = devCaps;
							DumpDeviceCaps(devID);

							CMemUtils.FreeUnmanagedBuf(ref devBuf); ;
						}
						else
						{
							throw new Exception("Oops - failed WacomMTGetDeviceCapabilities");
						}
					}

					return true;
				}
			}
			catch(Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (devBuf != IntPtr.Zero)
				{
					CMemUtils.FreeUnmanagedBuf(ref devBuf);
				}
			}

			return false;
		}
	}
}
