namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            chkAutoOcr.Checked = Properties.Settings.Default.AutoOcr;
            chkDelay.Checked = Properties.Settings.Default.DelayCapture;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.AutoOcr = chkAutoOcr.Checked;
            Properties.Settings.Default.DelayCapture = chkDelay.Checked;
            Properties.Settings.Default.Save();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveSettings();
            base.OnFormClosing(e);
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            this.Hide();

            // 遅延が有効な場合は5秒待つ
            if (chkDelay.Checked)
            {
                Thread.Sleep(5000);
            }
            else
            {
                Thread.Sleep(200); // フォームが消えるのを待つ
            }

            using (var captureForm = new CaptureForm())
            {
                if (captureForm.ShowDialog() == DialogResult.OK)
                {
                    if (captureForm.CapturedImage != null)
                    {
                        var viewerForm = new ImageViewerForm(captureForm.CapturedImage, chkAutoOcr.Checked);
                        viewerForm.Show();
                    }
                }
            }

            this.Show();
        }
    }
}
