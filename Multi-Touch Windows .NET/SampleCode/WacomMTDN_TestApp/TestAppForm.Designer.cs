namespace WacomMTTestApp
{
	partial class TestForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.fingerPanel = new System.Windows.Forms.Panel();
			this.functionPanel = new System.Windows.Forms.Panel();
			this.ConfidenceBitsEnabledGroupBox = new System.Windows.Forms.GroupBox();
			this.ConfidenceBitsDisabledRadioButton = new System.Windows.Forms.RadioButton();
			this.ConfidenceBitsEnabledRadioButton = new System.Windows.Forms.RadioButton();
			this.FingerDataFromGroupBox = new System.Windows.Forms.GroupBox();
			this.RawDataFromCallbackRadioButton = new System.Windows.Forms.RadioButton();
			this.BlobDataFromEventRadioButton = new System.Windows.Forms.RadioButton();
			this.BlobDataFromCallbackRadioButton = new System.Windows.Forms.RadioButton();
			this.FingerDataFromEventRadioButton = new System.Windows.Forms.RadioButton();
			this.FingerDataFromCallbackRadioButton = new System.Windows.Forms.RadioButton();
			this.DeviceIDSelectLabel = new System.Windows.Forms.Label();
			this.DeviceIDComboBox = new System.Windows.Forms.ComboBox();
			this.ClientModeGroupBox = new System.Windows.Forms.GroupBox();
			this.ClientModeObserverRadioButton = new System.Windows.Forms.RadioButton();
			this.ClientModeConsumerRadioButton = new System.Windows.Forms.RadioButton();
			this.DeviceIDCapabilitiesFormButton = new System.Windows.Forms.Button();
			this.mClearButton = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.ToolStripStatusFingerCountLabel = new System.Windows.Forms.Label();
			this.ToolStripStatusConfidenceBitsEnabledLabel = new System.Windows.Forms.Label();
			this.ToolStripStatusUserDataLabel = new System.Windows.Forms.Label();
			this.ToolStripStatusClientModeLabel = new System.Windows.Forms.Label();
			this.functionPanel.SuspendLayout();
			this.ConfidenceBitsEnabledGroupBox.SuspendLayout();
			this.FingerDataFromGroupBox.SuspendLayout();
			this.ClientModeGroupBox.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// fingerPanel
			// 
			this.fingerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.fingerPanel.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.fingerPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.fingerPanel.Location = new System.Drawing.Point(5, 43);
			this.fingerPanel.Name = "fingerPanel";
			this.fingerPanel.Size = new System.Drawing.Size(747, 406);
			this.fingerPanel.TabIndex = 0;
			// 
			// functionPanel
			// 
			this.functionPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.functionPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.functionPanel.Controls.Add(this.ConfidenceBitsEnabledGroupBox);
			this.functionPanel.Controls.Add(this.FingerDataFromGroupBox);
			this.functionPanel.Controls.Add(this.DeviceIDSelectLabel);
			this.functionPanel.Controls.Add(this.DeviceIDComboBox);
			this.functionPanel.Controls.Add(this.ClientModeGroupBox);
			this.functionPanel.Controls.Add(this.DeviceIDCapabilitiesFormButton);
			this.functionPanel.Controls.Add(this.mClearButton);
			this.functionPanel.Location = new System.Drawing.Point(758, 9);
			this.functionPanel.Name = "functionPanel";
			this.functionPanel.Size = new System.Drawing.Size(132, 440);
			this.functionPanel.TabIndex = 1;
			// 
			// ConfidenceBitsEnabledGroupBox
			// 
			this.ConfidenceBitsEnabledGroupBox.Controls.Add(this.ConfidenceBitsDisabledRadioButton);
			this.ConfidenceBitsEnabledGroupBox.Controls.Add(this.ConfidenceBitsEnabledRadioButton);
			this.ConfidenceBitsEnabledGroupBox.Location = new System.Drawing.Point(3, 224);
			this.ConfidenceBitsEnabledGroupBox.Name = "ConfidenceBitsEnabledGroupBox";
			this.ConfidenceBitsEnabledGroupBox.Size = new System.Drawing.Size(121, 70);
			this.ConfidenceBitsEnabledGroupBox.TabIndex = 6;
			this.ConfidenceBitsEnabledGroupBox.TabStop = false;
			this.ConfidenceBitsEnabledGroupBox.Text = "Confidence Bits:";
			// 
			// ConfidenceBitsDisabledRadioButton
			// 
			this.ConfidenceBitsDisabledRadioButton.AutoSize = true;
			this.ConfidenceBitsDisabledRadioButton.Location = new System.Drawing.Point(12, 44);
			this.ConfidenceBitsDisabledRadioButton.Name = "ConfidenceBitsDisabledRadioButton";
			this.ConfidenceBitsDisabledRadioButton.Size = new System.Drawing.Size(66, 17);
			this.ConfidenceBitsDisabledRadioButton.TabIndex = 1;
			this.ConfidenceBitsDisabledRadioButton.Text = "Disabled";
			this.ConfidenceBitsDisabledRadioButton.UseVisualStyleBackColor = true;
			this.ConfidenceBitsDisabledRadioButton.CheckedChanged += new System.EventHandler(this.ConfidenceBitsRadioButton_CheckedChanged);
			this.ConfidenceBitsDisabledRadioButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommonHandler_KeyPress);
			// 
			// ConfidenceBitsEnabledRadioButton
			// 
			this.ConfidenceBitsEnabledRadioButton.AutoSize = true;
			this.ConfidenceBitsEnabledRadioButton.Checked = true;
			this.ConfidenceBitsEnabledRadioButton.Location = new System.Drawing.Point(12, 20);
			this.ConfidenceBitsEnabledRadioButton.Name = "ConfidenceBitsEnabledRadioButton";
			this.ConfidenceBitsEnabledRadioButton.Size = new System.Drawing.Size(64, 17);
			this.ConfidenceBitsEnabledRadioButton.TabIndex = 0;
			this.ConfidenceBitsEnabledRadioButton.TabStop = true;
			this.ConfidenceBitsEnabledRadioButton.Text = "Enabled";
			this.ConfidenceBitsEnabledRadioButton.UseVisualStyleBackColor = true;
			this.ConfidenceBitsEnabledRadioButton.CheckedChanged += new System.EventHandler(this.ConfidenceBitsRadioButton_CheckedChanged);
			this.ConfidenceBitsEnabledRadioButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommonHandler_KeyPress);
			// 
			// FingerDataFromGroupBox
			// 
			this.FingerDataFromGroupBox.Controls.Add(this.RawDataFromCallbackRadioButton);
			this.FingerDataFromGroupBox.Controls.Add(this.BlobDataFromEventRadioButton);
			this.FingerDataFromGroupBox.Controls.Add(this.BlobDataFromCallbackRadioButton);
			this.FingerDataFromGroupBox.Controls.Add(this.FingerDataFromEventRadioButton);
			this.FingerDataFromGroupBox.Controls.Add(this.FingerDataFromCallbackRadioButton);
			this.FingerDataFromGroupBox.Location = new System.Drawing.Point(3, 79);
			this.FingerDataFromGroupBox.Name = "FingerDataFromGroupBox";
			this.FingerDataFromGroupBox.Size = new System.Drawing.Size(119, 139);
			this.FingerDataFromGroupBox.TabIndex = 5;
			this.FingerDataFromGroupBox.TabStop = false;
			this.FingerDataFromGroupBox.Text = "Touch data from:";
			// 
			// RawDataFromCallbackRadioButton
			// 
			this.RawDataFromCallbackRadioButton.Location = new System.Drawing.Point(12, 114);
			this.RawDataFromCallbackRadioButton.Name = "RawDataFromCallbackRadioButton";
			this.RawDataFromCallbackRadioButton.Size = new System.Drawing.Size(105, 19);
			this.RawDataFromCallbackRadioButton.TabIndex = 4;
			this.RawDataFromCallbackRadioButton.Text = "Raw Callbacks";
			this.RawDataFromCallbackRadioButton.UseVisualStyleBackColor = true;
			this.RawDataFromCallbackRadioButton.CheckedChanged += new System.EventHandler(this.FingerDataFromRadioButton_CheckedChanged);
			this.RawDataFromCallbackRadioButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommonHandler_KeyPress);
			// 
			// BlobDataFromEventRadioButton
			// 
			this.BlobDataFromEventRadioButton.Location = new System.Drawing.Point(12, 39);
			this.BlobDataFromEventRadioButton.Name = "BlobDataFromEventRadioButton";
			this.BlobDataFromEventRadioButton.Size = new System.Drawing.Size(105, 19);
			this.BlobDataFromEventRadioButton.TabIndex = 3;
			this.BlobDataFromEventRadioButton.Text = "Blob Events";
			this.BlobDataFromEventRadioButton.UseVisualStyleBackColor = true;
			this.BlobDataFromEventRadioButton.CheckedChanged += new System.EventHandler(this.FingerDataFromRadioButton_CheckedChanged);
			this.BlobDataFromEventRadioButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommonHandler_KeyPress);
			// 
			// BlobDataFromCallbackRadioButton
			// 
			this.BlobDataFromCallbackRadioButton.Location = new System.Drawing.Point(12, 89);
			this.BlobDataFromCallbackRadioButton.Name = "BlobDataFromCallbackRadioButton";
			this.BlobDataFromCallbackRadioButton.Size = new System.Drawing.Size(105, 19);
			this.BlobDataFromCallbackRadioButton.TabIndex = 2;
			this.BlobDataFromCallbackRadioButton.Text = "Blob Callbacks";
			this.BlobDataFromCallbackRadioButton.UseVisualStyleBackColor = true;
			this.BlobDataFromCallbackRadioButton.CheckedChanged += new System.EventHandler(this.FingerDataFromRadioButton_CheckedChanged);
			this.BlobDataFromCallbackRadioButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommonHandler_KeyPress);
			// 
			// FingerDataFromEventRadioButton
			// 
			this.FingerDataFromEventRadioButton.Checked = true;
			this.FingerDataFromEventRadioButton.Location = new System.Drawing.Point(12, 14);
			this.FingerDataFromEventRadioButton.Name = "FingerDataFromEventRadioButton";
			this.FingerDataFromEventRadioButton.Size = new System.Drawing.Size(105, 19);
			this.FingerDataFromEventRadioButton.TabIndex = 1;
			this.FingerDataFromEventRadioButton.TabStop = true;
			this.FingerDataFromEventRadioButton.Text = "Finger Events";
			this.FingerDataFromEventRadioButton.UseVisualStyleBackColor = true;
			this.FingerDataFromEventRadioButton.CheckedChanged += new System.EventHandler(this.FingerDataFromRadioButton_CheckedChanged);
			this.FingerDataFromEventRadioButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommonHandler_KeyPress);
			// 
			// FingerDataFromCallbackRadioButton
			// 
			this.FingerDataFromCallbackRadioButton.Location = new System.Drawing.Point(12, 64);
			this.FingerDataFromCallbackRadioButton.Margin = new System.Windows.Forms.Padding(1);
			this.FingerDataFromCallbackRadioButton.Name = "FingerDataFromCallbackRadioButton";
			this.FingerDataFromCallbackRadioButton.Size = new System.Drawing.Size(105, 19);
			this.FingerDataFromCallbackRadioButton.TabIndex = 0;
			this.FingerDataFromCallbackRadioButton.Text = "Finger Callbacks";
			this.FingerDataFromCallbackRadioButton.UseVisualStyleBackColor = true;
			this.FingerDataFromCallbackRadioButton.CheckedChanged += new System.EventHandler(this.FingerDataFromRadioButton_CheckedChanged);
			this.FingerDataFromCallbackRadioButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommonHandler_KeyPress);
			// 
			// DeviceIDSelectLabel
			// 
			this.DeviceIDSelectLabel.Location = new System.Drawing.Point(3, 313);
			this.DeviceIDSelectLabel.Name = "DeviceIDSelectLabel";
			this.DeviceIDSelectLabel.Size = new System.Drawing.Size(57, 30);
			this.DeviceIDSelectLabel.TabIndex = 4;
			this.DeviceIDSelectLabel.Text = "Select DeviceID:";
			// 
			// DeviceIDComboBox
			// 
			this.DeviceIDComboBox.FormattingEnabled = true;
			this.DeviceIDComboBox.Location = new System.Drawing.Point(77, 313);
			this.DeviceIDComboBox.Name = "DeviceIDComboBox";
			this.DeviceIDComboBox.Size = new System.Drawing.Size(47, 21);
			this.DeviceIDComboBox.TabIndex = 3;
			this.DeviceIDComboBox.SelectedIndexChanged += new System.EventHandler(this.DeviceIDComboBox_SelectedIndexChanged);
			this.DeviceIDComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommonHandler_KeyPress);
			// 
			// ClientModeGroupBox
			// 
			this.ClientModeGroupBox.Controls.Add(this.ClientModeObserverRadioButton);
			this.ClientModeGroupBox.Controls.Add(this.ClientModeConsumerRadioButton);
			this.ClientModeGroupBox.Location = new System.Drawing.Point(3, 3);
			this.ClientModeGroupBox.Name = "ClientModeGroupBox";
			this.ClientModeGroupBox.Size = new System.Drawing.Size(119, 70);
			this.ClientModeGroupBox.TabIndex = 2;
			this.ClientModeGroupBox.TabStop = false;
			this.ClientModeGroupBox.Text = "Client Mode:";
			// 
			// ClientModeObserverRadioButton
			// 
			this.ClientModeObserverRadioButton.Location = new System.Drawing.Point(12, 40);
			this.ClientModeObserverRadioButton.Name = "ClientModeObserverRadioButton";
			this.ClientModeObserverRadioButton.Size = new System.Drawing.Size(72, 17);
			this.ClientModeObserverRadioButton.TabIndex = 1;
			this.ClientModeObserverRadioButton.Text = "Observer";
			this.ClientModeObserverRadioButton.UseVisualStyleBackColor = true;
			this.ClientModeObserverRadioButton.CheckedChanged += new System.EventHandler(this.ClientModeRadioButton_CheckedChanged);
			this.ClientModeObserverRadioButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommonHandler_KeyPress);
			// 
			// ClientModeConsumerRadioButton
			// 
			this.ClientModeConsumerRadioButton.Checked = true;
			this.ClientModeConsumerRadioButton.Location = new System.Drawing.Point(12, 19);
			this.ClientModeConsumerRadioButton.Name = "ClientModeConsumerRadioButton";
			this.ClientModeConsumerRadioButton.Size = new System.Drawing.Size(72, 17);
			this.ClientModeConsumerRadioButton.TabIndex = 0;
			this.ClientModeConsumerRadioButton.TabStop = true;
			this.ClientModeConsumerRadioButton.Text = "Consumer";
			this.ClientModeConsumerRadioButton.UseVisualStyleBackColor = true;
			this.ClientModeConsumerRadioButton.CheckedChanged += new System.EventHandler(this.ClientModeRadioButton_CheckedChanged);
			this.ClientModeConsumerRadioButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommonHandler_KeyPress);
			// 
			// DeviceIDCapabilitiesFormButton
			// 
			this.DeviceIDCapabilitiesFormButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DeviceIDCapabilitiesFormButton.Location = new System.Drawing.Point(4, 375);
			this.DeviceIDCapabilitiesFormButton.Name = "DeviceIDCapabilitiesFormButton";
			this.DeviceIDCapabilitiesFormButton.Size = new System.Drawing.Size(119, 27);
			this.DeviceIDCapabilitiesFormButton.TabIndex = 1;
			this.DeviceIDCapabilitiesFormButton.Text = "Dev Capabilities ...";
			this.DeviceIDCapabilitiesFormButton.UseVisualStyleBackColor = true;
			this.DeviceIDCapabilitiesFormButton.Click += new System.EventHandler(this.DeviceCapabilitiesForm_Button_Click);
			this.DeviceIDCapabilitiesFormButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommonHandler_KeyPress);
			// 
			// mClearButton
			// 
			this.mClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.mClearButton.Location = new System.Drawing.Point(4, 408);
			this.mClearButton.Name = "mClearButton";
			this.mClearButton.Size = new System.Drawing.Size(120, 23);
			this.mClearButton.TabIndex = 0;
			this.mClearButton.Text = "Clear";
			this.mClearButton.UseVisualStyleBackColor = true;
			this.mClearButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommonHandler_KeyPress);
			this.mClearButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.mClearButton_MouseClick);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.Add(this.ToolStripStatusFingerCountLabel);
			this.panel1.Controls.Add(this.ToolStripStatusConfidenceBitsEnabledLabel);
			this.panel1.Controls.Add(this.ToolStripStatusUserDataLabel);
			this.panel1.Controls.Add(this.ToolStripStatusClientModeLabel);
			this.panel1.Location = new System.Drawing.Point(5, 9);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(747, 28);
			this.panel1.TabIndex = 2;
			// 
			// ToolStripStatusFingerCountLabel
			// 
			this.ToolStripStatusFingerCountLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ToolStripStatusFingerCountLabel.Location = new System.Drawing.Point(410, 3);
			this.ToolStripStatusFingerCountLabel.Margin = new System.Windows.Forms.Padding(3);
			this.ToolStripStatusFingerCountLabel.Name = "ToolStripStatusFingerCountLabel";
			this.ToolStripStatusFingerCountLabel.Padding = new System.Windows.Forms.Padding(2);
			this.ToolStripStatusFingerCountLabel.Size = new System.Drawing.Size(80, 20);
			this.ToolStripStatusFingerCountLabel.TabIndex = 3;
			this.ToolStripStatusFingerCountLabel.Text = "#fingers:";
			this.ToolStripStatusFingerCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ToolStripStatusConfidenceBitsEnabledLabel
			// 
			this.ToolStripStatusConfidenceBitsEnabledLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ToolStripStatusConfidenceBitsEnabledLabel.Location = new System.Drawing.Point(90, 3);
			this.ToolStripStatusConfidenceBitsEnabledLabel.Margin = new System.Windows.Forms.Padding(3);
			this.ToolStripStatusConfidenceBitsEnabledLabel.Name = "ToolStripStatusConfidenceBitsEnabledLabel";
			this.ToolStripStatusConfidenceBitsEnabledLabel.Padding = new System.Windows.Forms.Padding(2);
			this.ToolStripStatusConfidenceBitsEnabledLabel.Size = new System.Drawing.Size(108, 20);
			this.ToolStripStatusConfidenceBitsEnabledLabel.TabIndex = 2;
			this.ToolStripStatusConfidenceBitsEnabledLabel.Text = "Conf Bits Enabled";
			this.ToolStripStatusConfidenceBitsEnabledLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ToolStripStatusUserDataLabel
			// 
			this.ToolStripStatusUserDataLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ToolStripStatusUserDataLabel.Location = new System.Drawing.Point(204, 3);
			this.ToolStripStatusUserDataLabel.Margin = new System.Windows.Forms.Padding(3);
			this.ToolStripStatusUserDataLabel.Name = "ToolStripStatusUserDataLabel";
			this.ToolStripStatusUserDataLabel.Padding = new System.Windows.Forms.Padding(2);
			this.ToolStripStatusUserDataLabel.Size = new System.Drawing.Size(200, 20);
			this.ToolStripStatusUserDataLabel.TabIndex = 1;
			// 
			// ToolStripStatusClientModeLabel
			// 
			this.ToolStripStatusClientModeLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ToolStripStatusClientModeLabel.Location = new System.Drawing.Point(3, 3);
			this.ToolStripStatusClientModeLabel.Margin = new System.Windows.Forms.Padding(3);
			this.ToolStripStatusClientModeLabel.Name = "ToolStripStatusClientModeLabel";
			this.ToolStripStatusClientModeLabel.Padding = new System.Windows.Forms.Padding(2);
			this.ToolStripStatusClientModeLabel.Size = new System.Drawing.Size(80, 20);
			this.ToolStripStatusClientModeLabel.TabIndex = 0;
			this.ToolStripStatusClientModeLabel.Text = "Consumer";
			this.ToolStripStatusClientModeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(893, 461);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.functionPanel);
			this.Controls.Add(this.fingerPanel);
			this.Name = "TestForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "WacomMTDN Demo";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestForm_FormClosing);
			this.Load += new System.EventHandler(this.TestForm_Load);
			this.Move += new System.EventHandler(this.TestForm_Move);
			this.Resize += new System.EventHandler(this.TestForm_Resize);
			this.functionPanel.ResumeLayout(false);
			this.ConfidenceBitsEnabledGroupBox.ResumeLayout(false);
			this.ConfidenceBitsEnabledGroupBox.PerformLayout();
			this.FingerDataFromGroupBox.ResumeLayout(false);
			this.ClientModeGroupBox.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel fingerPanel;
		private System.Windows.Forms.Panel functionPanel;
		private System.Windows.Forms.Button mClearButton;
		private System.Windows.Forms.Button DeviceIDCapabilitiesFormButton;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label ToolStripStatusClientModeLabel;
		private System.Windows.Forms.Label ToolStripStatusUserDataLabel;
		private System.Windows.Forms.GroupBox ClientModeGroupBox;
		private System.Windows.Forms.RadioButton ClientModeObserverRadioButton;
		private System.Windows.Forms.RadioButton ClientModeConsumerRadioButton;
		private System.Windows.Forms.Label DeviceIDSelectLabel;
		private System.Windows.Forms.ComboBox DeviceIDComboBox;
		private System.Windows.Forms.GroupBox FingerDataFromGroupBox;
		private System.Windows.Forms.RadioButton FingerDataFromEventRadioButton;
		private System.Windows.Forms.RadioButton FingerDataFromCallbackRadioButton;
		private System.Windows.Forms.Label ToolStripStatusConfidenceBitsEnabledLabel;
		private System.Windows.Forms.GroupBox ConfidenceBitsEnabledGroupBox;
		private System.Windows.Forms.RadioButton ConfidenceBitsDisabledRadioButton;
		private System.Windows.Forms.RadioButton ConfidenceBitsEnabledRadioButton;
		private System.Windows.Forms.Label ToolStripStatusFingerCountLabel;
		private System.Windows.Forms.RadioButton BlobDataFromEventRadioButton;
		private System.Windows.Forms.RadioButton BlobDataFromCallbackRadioButton;
		private System.Windows.Forms.RadioButton RawDataFromCallbackRadioButton;




	}
}

