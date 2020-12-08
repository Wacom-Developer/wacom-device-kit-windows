/*----------------------------------------------------------------------------s
	NAME
		DevCapsForm.cs

	PURPOSE
		This demo shows how to use Wacom Feel Multi-Touch in a .NET application
		to detect finger input

	COPYRIGHT
		This file is Copyright (c) Wacom Company, Ltd. 2020

		The text and information contained in this file may be freely used,
		copied, or distributed without compensation or licensing restrictions.
---------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WacomMTTestApp
{
	public partial class DevCapsForm : Form
	{
		private String mDevCapsText = "";

		public DevCapsForm()
		{
			InitializeComponent();
		}

		public String DevCapsText
		{
			set { mDevCapsText = value; }
		}

		private void DevCapsForm_Load(object sender, EventArgs e)
		{
			this.DevCapsLabel.Text = mDevCapsText;
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
