namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            this.Hide();
            Thread.Sleep(200); // フォームが消えるのを待つ

            using (var captureForm = new CaptureForm())
            {
                if (captureForm.ShowDialog() == DialogResult.OK)
                {
                    if (captureForm.CapturedImage != null)
                    {
                        var viewerForm = new ImageViewerForm(captureForm.CapturedImage);
                        viewerForm.Show();
                    }
                }
            }

            this.Show();
        }
    }
}
