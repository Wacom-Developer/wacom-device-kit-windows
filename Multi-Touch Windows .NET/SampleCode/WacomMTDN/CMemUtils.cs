/*----------------------------------------------------------------------------s
	NAME
		CMemUtils.cs

	PURPOSE
		This demo shows how to use Wacom Feel Multi-Touch in a .NET application
		to detect finger input

	COPYRIGHT
		This file is Copyright (c) Wacom Company, Ltd. 2020

		The text and information contained in this file may be freely used,
		copied, or distributed without compensation or licensing restrictions.
---------------------------------------------------------------------------- */

//#define TRACE_RAW_BYTES
// Some code requires a newer .NET version.
#define DOTNET_4_OR_LATER

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WacomMTDN
{
	///////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Class to provide utility methods for marshalling managed to unmanaged code.
    /// </summary>
	public class CMemUtils
	{
		/// <summary>
		/// Allocates a pointer to unmanaged heap memory of sizeof(val_I).
		/// </summary>
		/// <param name="val_I">managed object that determines #bytes of unmanaged buf</param>
		/// <returns>Unmanaged buffer pointer.</returns>
		public static IntPtr AllocUnmanagedBuf(Object val_I)
		{
			IntPtr buf = IntPtr.Zero;

			try
			{
				int numBytes = Marshal.SizeOf(val_I);

				// First allocate a buffer of the correct size.
				buf = Marshal.AllocHGlobal(numBytes);
			}
			catch (Exception ex)
			{
				MessageBox.Show("FAILED AllocUnmanagedBuf: " + ex.ToString());
			}

			return buf;
		}



		///////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Free unmanaged memory pointed to by buf_I and zero the buf.
		/// </summary>
		/// <param name="buf_I">pointer to unmanaged heap memory</param>
		public static void FreeUnmanagedBuf(ref IntPtr buf_I)
		{
			if (buf_I != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(buf_I);
				buf_I = IntPtr.Zero;
			}
		}



		///////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Allocates an unmanaged unicode BSTR.
		/// </summary>
		/// <param name="string_I">Managed string from which to generate unmanaged string.</param>
		/// <returns></returns>
		public static IntPtr AllocateUnmanagedString(String string_I)
		{
			try
			{
				return Marshal.StringToBSTR(string_I);
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			return IntPtr.Zero;
		}



		///////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Frees memory created for an unmanaged string.
		/// </summary>
		/// <param name="stringPtr_I">Allocated memory for this string is freed.</param>
		public static void FreeUnmanagedString(IntPtr stringPtr_I)
		{
			try
			{
				if ( stringPtr_I != IntPtr.Zero )
				{
					Marshal.FreeBSTR(stringPtr_I);
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}



		///////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Marshals an unmanaged string to a managed string
		/// </summary>
		/// <param name="strbuf_I">Unmanaged string buffer</param>
		/// <returns></returns>
		public static String PtrToManagedString(IntPtr strbuf_I)
		{
			try
			{
				if (strbuf_I != IntPtr.Zero)
				{
					return Marshal.PtrToStringBSTR(strbuf_I);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			return String.Empty;
		}


		///////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Marshals an unmanaged blob to the templated managed type.
		/// </summary>
		/// <typeparam name="T">Managed type to convert to.</typeparam>
		/// <param name="data_I">Pointer to blob of unmanaged data</param>
		/// <returns></returns>
		public static T PtrToStructure<T>(IntPtr data_I)
		{
			try
			{
				return (T)Marshal.PtrToStructure(data_I, typeof(T));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			return (T)Marshal.PtrToStructure(IntPtr.Zero, typeof(T));
		}
	}
}
