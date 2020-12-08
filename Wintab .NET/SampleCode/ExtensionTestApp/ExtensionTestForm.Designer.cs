namespace FormExtTestApp
{
    partial class ExtensionTestForm
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
            this.EKPanel = new System.Windows.Forms.Panel();
            this.TCPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // EKPanel
            // 
            this.EKPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.EKPanel.BackColor = System.Drawing.Color.LightSteelBlue;
            this.EKPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.EKPanel.Location = new System.Drawing.Point(0, 0);
            this.EKPanel.Name = "EKPanel";
            this.EKPanel.Size = new System.Drawing.Size(783, 838);
            this.EKPanel.TabIndex = 0;
            this.EKPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.EKPanel_Paint);
            // 
            // TCPanel
            // 
            this.TCPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.TCPanel.BackColor = System.Drawing.Color.White;
            this.TCPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TCPanel.Location = new System.Drawing.Point(790, 0);
            this.TCPanel.Name = "TCPanel";
            this.TCPanel.Size = new System.Drawing.Size(320, 838);
            this.TCPanel.TabIndex = 1;
            // 
            // ExtensionTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(1112, 216);
            this.Controls.Add(this.TCPanel);
            this.Controls.Add(this.EKPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExtensionTestForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Wintab .NET Extensions Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExtensionTestForm_FormClosing);
            this.Load += new System.EventHandler(this.ExtensionTestForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel EKPanel;
        private System.Windows.Forms.Panel TCPanel;
    }
}

