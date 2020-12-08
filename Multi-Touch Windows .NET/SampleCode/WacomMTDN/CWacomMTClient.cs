/*----------------------------------------------------------------------------s
	NAME
		CWacomMTClient.cs

	PURPOSE
		This demo shows how to use Wacom Feel Multi-Touch in a .NET application
		to detect finger input

	COPYRIGHT
		This file is Copyright (c) Wacom Company, Ltd. 2020

		The text and information contained in this file may be freely used,
		copied, or distributed without compensation or licensing restrictions.
---------------------------------------------------------------------------- */

//#define TRACE_CLIENT

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace WacomMTDN
{
	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class to manage configuration and touch data retrieval for one MTAPI client.
	/// See http://www.wacomeng.com/touch/WacomFeelMulti-TouchAPI.htm for MTAPI details.
	/// </summary>
	public abstract class CWacomMTClient
	{
		protected Int32 mDeviceID = -1;
		protected WacomMTProcessingMode mClientMode = WacomMTProcessingMode.WMTProcessingModeConsumer;
		protected WacomMTCallback mTouchCallback = null;
		protected IntPtr mUserData = IntPtr.Zero;

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="mode_I">Specifies what type of client to create (cannot be changed).</param>
		public CWacomMTClient(WacomMTProcessingMode mode_I) 
		{ 
			mClientMode = mode_I; 
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initalize MTAPI client parameters
		/// </summary>
		/// <param name="deviceID_I">touch device identifier</param>
		/// <param name="callback_I">callback function used by MTAPI to send touch data</param>
		/// <param name="userData_I">custom user data</param>
		protected void InitWacomMTClientParams(Int32 deviceID_I, ref WacomMTCallback callback_I, IntPtr userData_I)
		{
			ThrowIfInvalidDeviceID(deviceID_I);
			mDeviceID = deviceID_I;
			ThrowIfInvalidCallback(ref callback_I);
			mTouchCallback = callback_I;
			mUserData = userData_I;
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Clears MTAPI client parameters
		/// </summary>
		protected void ClearWacomMTClientParams()
		{
			mDeviceID = -1;
			mTouchCallback = null;
			mUserData = IntPtr.Zero;
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Throws an exception if deviceID is invalid.
		/// </summary>
		/// <param name="deviceID_I">touch device identifier</param>
		protected void ThrowIfInvalidDeviceID(Int32 deviceID_I)
		{
			if (deviceID_I < 0)
			{
				throw new Exception("Oops - bad deviceID_I: " + deviceID_I);
			}
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Throws an exception if callback_I is null.
		/// </summary>
		/// <param name="callback_I">callback function used by MTAPI to send touch data</param>
		protected void ThrowIfInvalidCallback(ref WacomMTCallback callback_I)
		{
			if (callback_I == null)
			{
				throw new Exception("Oops - null callback");
			}
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns the touch device to which this client is registered.
		/// </summary>
		public Int32 DeviceID
		{
			get { return mDeviceID; }
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Utility string to dump out client device and hitrect.
		/// </summary>
		/// <param name="msg_I">Optional message to preface the dump report.</param>
		public virtual void DumpClient(String msg_I)
		{
			Trace.Write("WacomMTDN: <<" + msg_I + ">> clientMode: " + mClientMode.ToString());
			Trace.WriteLine("; Device: " + mDeviceID);
		}
	} // end class CWacomMTClient

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class to manage configuration and touch data retrieval for one MTAPI windowed client.
	/// See http://www.wacomeng.com/touch/WacomFeelMulti-TouchAPI.htm for MTAPI details.
	/// </summary>
	public abstract class CWacomMTWindowClient : CWacomMTClient
	{
		protected WacomMTHitRect mHitRect = new WacomMTHitRect(0, 0, 0, 0);
		protected HWND mHwnd = IntPtr.Zero;
		protected Int32 mBufferDepth = 1;

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="mode_I">MTAPI mode for processing touch data</param>
		public CWacomMTWindowClient(WacomMTProcessingMode mode_I) 
		: base(mode_I)
		{
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Sets/verifies MTAPI hitrect client params.
		/// </summary>
		/// <param name="deviceID_I">touch device identifier</param>
		/// <param name="hitrect_I">rectangle tracked by MTAPI for touch contacts</param>
		/// <param name="callback_I">callback function used by MTAPI to send touch data</param>
		/// <param name="userData_I">custom user data</param>s
		protected void InitWacomMTHitRectClientParams(Int32 deviceID_I, 
			WacomMTHitRect hitrect_I, ref WacomMTCallback callback_I, IntPtr userData_I)
		{
			InitWacomMTClientParams(deviceID_I, ref callback_I, userData_I);
			
			ThrowIfInvalidHitRect(ref hitrect_I);
			mHitRect = hitrect_I;
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Sets/verifies MTAPI hwnd client params.
		/// </summary>
		/// <param name="deviceID_I">touch device identifier</param>
		/// <param name="hwnd_I">window handle tracked by MTAPI for touch contacts</param>
		/// <param bufferDepth_I="userData_I">number of callback buffers that MTAPI creates</param>
		protected void InitWacomMTHwndClientParams(Int32 deviceID_I, HWND hwnd_I, Int32 bufferDepth_I)
		{
			ThrowIfInvalidDeviceID(deviceID_I);
			mDeviceID = deviceID_I;
			ThrowIfInvalidHWND(hwnd_I);
			mHwnd = hwnd_I;
			ThrowIfInvalidBufferDepth(bufferDepth_I);
			mBufferDepth = bufferDepth_I;
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Sets client params to a default "not a client" condition.
		/// </summary>
		protected void ClearWacomMTWindowClientParams()
		{
			ClearWacomMTClientParams();
			mHitRect = new WacomMTHitRect(0, 0, 0, 0);
			mHwnd = IntPtr.Zero;
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Throws exception for invalid HWND buffer depth.
		/// </summary>
		/// <param bufferDepth_I="userData_I">number of callback buffers that MTAPI creates</param>
		public void ThrowIfInvalidBufferDepth(Int32 bufferDepth_I)
		{
			if (bufferDepth_I < 1)
			{
				throw new Exception("Oops - invalid bufferDepth");
			}
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Throws an exception if invalid window handle.
		/// </summary>
		/// <param name="hwnd_I">window handle tracked by MTAPI for touch contacts</param>
		public void ThrowIfInvalidHWND(HWND hwnd_I)
		{
			if (hwnd_I.value == IntPtr.Zero)
			{
				throw new Exception("Oops - trying to use a null window handle!");
			}
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Throws an exception if hitrect is invalid.
		/// </summary>
		/// <param name="hitrect_I">rectangle tracked by MTAPI for touch contacts</param>
		public void ThrowIfInvalidHitRect(ref WacomMTHitRect hitRect_I)
		{
			if (hitRect_I.Width == 0 || hitRect_I.Height == 0)
			{
				throw new Exception("Oops - bad hitrect: [" +
					hitRect_I.Width + "," + hitRect_I.Height + "]");
			}
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns true if client has been registered as a hitrect or HWND client
		/// </summary>
		/// <returns>True if registered</returns>
		public bool IsRegistered()
		{
			return IsRegisteredAsHitRectClient() || IsRegisteredAsHwndClient();
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns true if registed as a hitrect MTAPI client.
		/// </summary>
		/// <returns>True if registered</returns>
		public bool IsRegisteredAsHitRectClient()
		{
			return mDeviceID >= 0 && mTouchCallback != null;
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns true if registed as an HWND MTAPI client.
		/// </summary>
		/// <returns>True if registered</returns>
		public bool IsRegisteredAsHwndClient()
		{
			return mDeviceID >= 0 && mHwnd.value != IntPtr.Zero;
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Utility string to dump out client device and hitrect.
		/// </summary>
		/// <param name="msg_I">Optional message to preface the dump report.</param>
		public override void DumpClient(String msg_I)
		{
			Trace.Write("WacomMTDN: <<" + msg_I + ">> clientMode: " + mClientMode.ToString());
			Trace.Write("; Device: " + mDeviceID);
			Trace.Write("; hitrect: [" + mHitRect.OriginX + "," + mHitRect.OriginY);
			Trace.WriteLine("," + mHitRect.Width + "," + mHitRect.Height + "]");
		}

		// -------------------------------------------------------------------------
		// HitRect methods
		// -------------------------------------------------------------------------

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Registers an MTAPI windowed client using a hitrect.
		/// </summary>
		/// <param name="deviceID_I">touch device identifier</param>
		/// <param name="hitrect_I">rectangle tracked by MTAPI for touch contacts</param>
		/// <param name="callback_I">callback function used by MTAPI to send touch data</param>
		/// <param name="userData_I">custom user data</param>
		public void RegisterHitRectClient(Int32 deviceID_I, WacomMTHitRect hitrect_I,
			ref WacomMTCallback callback_I, IntPtr userData_I)
		{
			try
			{
				InitWacomMTHitRectClientParams(deviceID_I, hitrect_I, ref callback_I, userData_I);

				WacomMTError res = DoRegisterHitRectCallback();

				if (WacomMTError.WMTErrorSuccess != res)
				{
					String errMsg = "Oops - failed DoRegisterHitRectCallback with error: " + res.ToString();
					throw new Exception(errMsg);
				}

#if TRACE_CLIENT 
				DumpClient("RegisterHitRectClient");
#endif //TRACE_CLIENT
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Unregisters an MTAPI hitrect client
		/// </summary>
		public void UnregisterHitRectClient()
		{
			try
			{
				ThrowIfInvalidDeviceID(mDeviceID);
				ThrowIfInvalidHitRect(ref mHitRect);

#if TRACE_CLIENT 
				DumpClient("UnregisterHitRectClient");
#endif //TRACE_CLIENT

				WacomMTError res = DoUnregisterHitRectCallback();

				ClearWacomMTWindowClientParams();

				if (WacomMTError.WMTErrorSuccess != res)
				{
					String errMsg = "Oops - failed to unregister a hitrect client with error: " + res.ToString();
					throw new Exception(errMsg);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Changes a hitrect client's current hitrect.
		/// </summary>
		/// <param name="newHitRect_I">rectangle tracked by MTAPI for touch contacts</param>
		/// <param name="userData_I">custom user data</param>
		public void MoveHitRectClient(WacomMTHitRect newHitRect_I, IntPtr userData_I)
		{
			try
			{
				WacomMTError res = WacomMTError.WMTErrorSuccess;
				WacomMTHitRect oldHitRect = mHitRect;
				WacomMTHitRect newHitRect = newHitRect_I;

				ThrowIfInvalidDeviceID(mDeviceID);
				ThrowIfInvalidHitRect(ref oldHitRect);
				ThrowIfInvalidHitRect(ref newHitRect);

				res = DoMoveHitRectCallback(newHitRect, userData_I);

				if (WacomMTError.WMTErrorSuccess != res)
				{
					String errMsg = "Oops - failed DoMoveHitRectCallback with error: " + res.ToString();
					throw new Exception(errMsg);
				}

				mHitRect = newHitRect;
				mUserData = userData_I;

#if TRACE_CLIENT 
				DumpClient("MoveHitRectClient");
#endif //TRACE_CLIENT
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Send updated hitrect to MTAPI for tracking this client.
		/// </summary>
		/// <param name="hitrect_I">rectangle tracked by MTAPI for touch contacts</param>
		public void UpdateHitRect(WacomMTHitRect hitrect_I)
		{
			try
			{
				// Unregister the client at the old hitrect.
				UnregisterHitRectClient();

				// Register the client at the new hitrect.
				RegisterHitRectClient(mDeviceID, hitrect_I, ref mTouchCallback, mUserData);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to register a client by hitrect.
		/// </summary>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected abstract WacomMTError DoRegisterHitRectCallback();

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to unregiser a client by hitrect.
		/// </summary>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected abstract WacomMTError DoUnregisterHitRectCallback();

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Help function to notify MTAPI that a client has moved.
		/// </summary>
		/// <param name="newHitRect_I">rectangle tracked by MTAPI for touch contacts</param>
		/// <param name="userData_I">custom user data</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected abstract WacomMTError DoMoveHitRectCallback(
			WacomMTHitRect newHitRect_I, IntPtr userData_I);

		// -------------------------------------------------------------------------
		// Hwnd methods
		// -------------------------------------------------------------------------

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Register an MTAPI client by its window handle.
		/// </summary>
		/// <param name="deviceID_I">touch device identifier</param>
		/// <param name="hwnd_I">window handle tracked by MTAPI for touch contacts</param>
		/// <param bufferDepth_I="userData_I">number of callback buffers that MTAPI creates</param>
		public void RegisterHWNDClient(Int32 deviceID_I, HWND hwnd_I,
			Int32 bufferDepth_I = 1)
		{
			try
			{
				InitWacomMTHwndClientParams(deviceID_I, hwnd_I, bufferDepth_I);

				WacomMTError res = DoRegisterHwndCallback();

				if (WacomMTError.WMTErrorSuccess != res)
				{
					String errMsg = "Oops - failed DoRegisterHwndCallback with error: " + res.ToString();
					throw new Exception(errMsg);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Unregisters an MTAPI HWND client.
		/// </summary>
		public void UnregisterHWNDClient()
		{
			try
			{
				ThrowIfInvalidHWND(mHwnd);

				WacomMTError res = DoUnregisterHwndCallback();

				ClearWacomMTWindowClientParams();

				if (WacomMTError.WMTErrorSuccess != res)
				{
					String errMsg = "Oops - failed DoUnregisterHwndCallback with error: " + res.ToString();
					throw new Exception(errMsg);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to register an MTAPI client by its HWND.
		/// </summary>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected abstract WacomMTError DoRegisterHwndCallback();

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to unregister an MTAPI client by its HWND.
		/// </summary>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected abstract WacomMTError DoUnregisterHwndCallback();

	} // end CWacomMTWindowClient


	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class to manage configuration and finger data retrieval for one MTAPI finger data client.
	/// See http://www.wacomeng.com/touch/WacomFeelMulti-TouchAPI.htm for MTAPI details.
	/// </summary>
	public class CWacomMTFingerClient : CWacomMTWindowClient
	{
		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="mode_I">MTAPI mode for processing touch data</param>
		public CWacomMTFingerClient(WacomMTProcessingMode mode_I) 
		: base(mode_I)
		{ 
		}

		// -------------------------------------------------------------------------
		// HitRect methods
		// -------------------------------------------------------------------------

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to register an MTAPI finger client by a hitrect.
		/// </summary>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected override WacomMTError DoRegisterHitRectCallback()
		{
			return CWacomMTInterface.WacomMTRegisterFingerReadCallback(
				mDeviceID, ref mHitRect, mClientMode, mTouchCallback, mUserData);
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to unregister an MTAPI finger client by its hitrect.
		/// </summary>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected override WacomMTError DoUnregisterHitRectCallback()
		{
			return CWacomMTInterface.WacomMTUnRegisterFingerReadCallback(
				mDeviceID,
				ref mHitRect,
				mClientMode,
				mUserData);
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to move an MTAPI finger client hitrect.
		/// </summary>
		/// <param name="hitrect_I">rectangle tracked by MTAPI for touch contacts</param>
		/// <param name="userData_I">custom user data</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected override WacomMTError DoMoveHitRectCallback(
			WacomMTHitRect newHitRect_I, IntPtr userData_I)
		{
			WacomMTHitRect oldHitRect = mHitRect;

			return CWacomMTInterface.WacomMTMoveRegisteredFingerReadCallback(
				mDeviceID,
				ref oldHitRect,
				mClientMode,
				ref newHitRect_I,
				userData_I
				);
		}

		// -------------------------------------------------------------------------
		// Hwnd methods
		// -----------------------------------------------------------------------

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to register an MTAPI finger client by its HWND.
		/// </summary>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected override WacomMTError DoRegisterHwndCallback()
		{
			return CWacomMTInterface.WacomMTRegisterFingerReadHWND(
				mDeviceID, mClientMode, mHwnd, mBufferDepth);
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to unregister an MTAPI finger client by its HWND.
		/// </summary>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected override WacomMTError DoUnregisterHwndCallback()
		{
			return CWacomMTInterface.WacomMTUnRegisterFingerReadHWND(mHwnd);
		}

	} // end class CWacomMTFingerClient

	///////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class to manage configuration and blob data retrieval for one MTAPI client.
	/// See http://www.wacomeng.com/touch/WacomFeelMulti-TouchAPI.htm for MTAPI details.
	/// </summary>
	public class CWacomMTBlobClient : CWacomMTWindowClient
	{
		///////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="mode_I"></param>
		public CWacomMTBlobClient(WacomMTProcessingMode mode_I) 
		: base(mode_I)
		{ 
		}

		// -------------------------------------------------------------------------
		// HitRect methods
		// -------------------------------------------------------------------------

		///////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to register an MTAPI blob client by a hitrect.
		/// </summary>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected override WacomMTError DoRegisterHitRectCallback()
		{
			return CWacomMTInterface.WacomMTRegisterBlobReadCallback(
				mDeviceID, ref mHitRect, mClientMode, mTouchCallback, mUserData);
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to unregister an MTAPI blob client by a hitrect.
		/// </summary>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected override WacomMTError DoUnregisterHitRectCallback()
		{
			return CWacomMTInterface.WacomMTUnRegisterBlobReadCallback(
				mDeviceID,
				ref mHitRect,
				mClientMode,
				mUserData);
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to move an MTAPI blob callback by hitrect.
		/// </summary>
		/// <param name="newHitRect_I">rectangle tracked by MTAPI for touch contacts</param>
		/// <param name="userData_I">custom user data</param>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected override WacomMTError DoMoveHitRectCallback(
			WacomMTHitRect newHitRect_I, IntPtr userData_I)
		{
			WacomMTHitRect oldHitRect = mHitRect;

			return CWacomMTInterface.WacomMTMoveRegisteredBlobReadCallback(
				mDeviceID,
				ref oldHitRect,
				mClientMode,
				ref newHitRect_I,
				userData_I
				);
		}

		// -------------------------------------------------------------------------
		// Hwnd methods
		// -----------------------------------------------------------------------

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to register an MTAPI blob client by its HWND.
		/// </summary>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected override WacomMTError DoRegisterHwndCallback()
		{
			return CWacomMTInterface.WacomMTRegisterBlobReadHWND(
				mDeviceID, mClientMode, mHwnd, mBufferDepth);
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Helper function to unregister and MTAPI blob client by its HWND.
		/// </summary>
		/// <returns>See the WacomMTError enumeration definition.</returns>
		protected override WacomMTError DoUnregisterHwndCallback()
		{
			return CWacomMTInterface.WacomMTUnRegisterBlobReadHWND(mHwnd);
		}
	} // end class CWacomMTBlobClient



	///////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class to manage configuration and raw data retrieval for one MTAPI client.
	/// See http://www.wacomeng.com/touch/WacomFeelMulti-TouchAPI.htm for MTAPI details.
	/// </summary>
	public class CWacomMTRawClient : CWacomMTClient
	{
		public CWacomMTRawClient(WacomMTProcessingMode mode_I) 
		: base(mode_I)
		{ 
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Registers an MTAPI raw data client over the entire tablet surface
		/// (no hitrect or HWND required).
		/// </summary>
		/// <param name="deviceID_I">touch device identifier</param>
		/// <param name="callback_I">callback function used by MTAPI to send touch data</param>
		/// <param name="userData_I">custom user data</param>
		public void RegisterClient(Int32 deviceID_I,
			ref WacomMTCallback callback_I, IntPtr userData_I)
		{
			try
			{
				InitWacomMTClientParams(deviceID_I, ref callback_I, userData_I);

				WacomMTError res = CWacomMTInterface.WacomMTRegisterRawReadCallback(
						mDeviceID, mClientMode, mTouchCallback, mUserData);

				if (WacomMTError.WMTErrorSuccess != res)
				{
					String errMsg = "Oops - failed WacomMTRegisterRawReadCallback with error: " + res.ToString();
					throw new Exception(errMsg);
				}

#if TRACE_CLIENT 
		        DumpClient("RegisterClient");
#endif //TRACE_CLIENT
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Unregisters an MTAPI raw data client
		/// </summary>
		public void UnregisterClient()
		{
			try
			{
				ThrowIfInvalidDeviceID(mDeviceID);

				WacomMTError res = 
					CWacomMTInterface.WacomMTUnRegisterRawReadCallback(
					mDeviceID,
					mClientMode,
					mUserData);

				if (res != WacomMTError.WMTErrorSuccess)
				{
					throw new Exception("Failed UnregisterClient");
				}

				ClearWacomMTClientParams();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
	} // end class CWacomMTRawClient

} // end namespace WacomMTDN



////////////////////////////////////////////////////////////////////////////
