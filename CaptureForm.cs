using System.Drawing.Imaging;

namespace WinFormsApp1
{
    public class CaptureForm : Form
    {
        private Point startPoint;
        private Rectangle selectRect;
        private Bitmap? screenShot;
        private bool isSelecting = false;

        public Bitmap? CapturedImage { get; private set; }

        public CaptureForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.Cursor = Cursors.Cross;
            this.DoubleBuffered = true;
            this.BackColor = Color.White;
            this.Opacity = 0.3;

            CaptureScreen();
        }

        private void CaptureScreen()
        {
            // 全画面のスクリーンショットを取得
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            screenShot = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(screenShot))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                startPoint = e.Location;
                selectRect = new Rectangle(e.Location, new Size(0, 0));
                isSelecting = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isSelecting)
            {
                int x = Math.Min(startPoint.X, e.X);
                int y = Math.Min(startPoint.Y, e.Y);
                int width = Math.Abs(e.X - startPoint.X);
                int height = Math.Abs(e.Y - startPoint.Y);
                selectRect = new Rectangle(x, y, width, height);
                this.Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && isSelecting)
            {
                isSelecting = false;

                if (selectRect.Width > 5 && selectRect.Height > 5 && screenShot != null)
                {
                    // 選択範囲を切り取る
                    CapturedImage = new Bitmap(selectRect.Width, selectRect.Height);
                    using (Graphics g = Graphics.FromImage(CapturedImage))
                    {
                        g.DrawImage(screenShot,
                            new Rectangle(0, 0, selectRect.Width, selectRect.Height),
                            selectRect,
                            GraphicsUnit.Pixel);
                    }
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    this.DialogResult = DialogResult.Cancel;
                }

                this.Close();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (isSelecting && selectRect.Width > 0 && selectRect.Height > 0)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectRect);
                }

                // 選択領域の外側を暗くする
                using (SolidBrush darkBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                {
                    Rectangle[] regions = new Rectangle[]
                    {
                        new Rectangle(0, 0, this.Width, selectRect.Y), // 上
                        new Rectangle(0, selectRect.Y, selectRect.X, selectRect.Height), // 左
                        new Rectangle(selectRect.Right, selectRect.Y, this.Width - selectRect.Right, selectRect.Height), // 右
                        new Rectangle(0, selectRect.Bottom, this.Width, this.Height - selectRect.Bottom) // 下
                    };

                    foreach (var region in regions)
                    {
                        e.Graphics.FillRectangle(darkBrush, region);
                    }
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                screenShot?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
