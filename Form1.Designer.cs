namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnCapture = new Button();
            chkAutoOcr = new CheckBox();
            SuspendLayout();
            //
            // btnCapture
            //
            btnCapture.Location = new Point(12, 12);
            btnCapture.Name = "btnCapture";
            btnCapture.Size = new Size(150, 50);
            btnCapture.TabIndex = 0;
            btnCapture.Text = "キャプチャ開始";
            btnCapture.UseVisualStyleBackColor = true;
            btnCapture.Click += btnCapture_Click;
            //
            // chkAutoOcr
            //
            chkAutoOcr.AutoSize = true;
            chkAutoOcr.Checked = true;
            chkAutoOcr.CheckState = CheckState.Checked;
            chkAutoOcr.Location = new Point(12, 70);
            chkAutoOcr.Name = "chkAutoOcr";
            chkAutoOcr.Size = new Size(120, 19);
            chkAutoOcr.TabIndex = 1;
            chkAutoOcr.Text = "自動でOCR実行";
            chkAutoOcr.UseVisualStyleBackColor = true;
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(180, 100);
            Controls.Add(chkAutoOcr);
            Controls.Add(btnCapture);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "OniOCR";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        private Button btnCapture;
        private CheckBox chkAutoOcr;

        #endregion
    }
}
