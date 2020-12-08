namespace WintabDN
{
    partial class QueryDataForm
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
            this.testTextBox = new System.Windows.Forms.RichTextBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.peekRadioButton = new System.Windows.Forms.RadioButton();
            this.removeRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // testTextBox
            // 
            this.testTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.testTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.testTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.testTextBox.Location = new System.Drawing.Point(7, 85);
            this.testTextBox.Name = "testTextBox";
            this.testTextBox.Size = new System.Drawing.Size(1065, 465);
            this.testTextBox.TabIndex = 0;
            this.testTextBox.Text = "";
            // 
            // clearButton
            // 
            this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.clearButton.Location = new System.Drawing.Point(997, 50);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(75, 23);
            this.clearButton.TabIndex = 2;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.peekRadioButton);
            this.groupBox1.Controls.Add(this.removeRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(248, 67);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Select mode and touch the pen to the tablet:";
            // 
            // peekRadioButton
            // 
            this.peekRadioButton.AutoSize = true;
            this.peekRadioButton.Location = new System.Drawing.Point(32, 43);
            this.peekRadioButton.Name = "peekRadioButton";
            this.peekRadioButton.Size = new System.Drawing.Size(188, 17);
            this.peekRadioButton.TabIndex = 1;
            this.peekRadioButton.Text = "Peek at data in queue , then flush.";
            this.peekRadioButton.UseVisualStyleBackColor = true;
            // 
            // removeRadioButton
            // 
            this.removeRadioButton.AutoSize = true;
            this.removeRadioButton.Checked = true;
            this.removeRadioButton.Location = new System.Drawing.Point(32, 20);
            this.removeRadioButton.Name = "removeRadioButton";
            this.removeRadioButton.Size = new System.Drawing.Size(145, 17);
            this.removeRadioButton.TabIndex = 0;
            this.removeRadioButton.TabStop = true;
            this.removeRadioButton.Text = "Remove data from queue";
            this.removeRadioButton.UseVisualStyleBackColor = true;
            // 
            // QueryDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 562);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.testTextBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "QueryDataForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "QueryDataForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox testTextBox;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton peekRadioButton;
        private System.Windows.Forms.RadioButton removeRadioButton;
    }
}