namespace WacomMTTestApp
{
	partial class DevCapsForm
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
			this.DevCapsLabel = new System.Windows.Forms.Label();
			this.okButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// DevCapsLabel
			// 
			this.DevCapsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DevCapsLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.DevCapsLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DevCapsLabel.Location = new System.Drawing.Point(35, 26);
			this.DevCapsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.DevCapsLabel.Name = "DevCapsLabel";
			this.DevCapsLabel.Size = new System.Drawing.Size(741, 318);
			this.DevCapsLabel.TabIndex = 0;
			this.DevCapsLabel.Text = "label1";
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(351, 359);
			this.okButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(117, 31);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// DevCapsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(819, 403);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.DevCapsLabel);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.Name = "DevCapsForm";
			this.Text = "DevCapsForm";
			this.Load += new System.EventHandler(this.DevCapsForm_Load);
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Label DevCapsLabel;
		private System.Windows.Forms.Button okButton;
	}
}