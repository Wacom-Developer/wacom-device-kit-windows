///////////////////////////////////////////////////////////////////////////////
//
//	PURPOSE
//		Wintab extensions test dialog for WintabDN
//
//	COPYRIGHT
//		Copyright (c) 2019-2020 Wacom Co., Ltd.
//
//		The text and information contained in this file may be freely used,
//		copied, or distributed without compensation or licensing restrictions.
//
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Windows.Forms;

namespace FormExtTestApp
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ExtensionTestForm());
		}
	}
}
