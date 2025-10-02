using System.Drawing.Imaging;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace WinFormsApp1
{
    public class ImageViewerForm : Form
    {
        private PictureBox pictureBox;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem saveMenuItem;
        private ToolStripMenuItem copyMenuItem;
        private ToolStripMenuItem ocrMenuItem;
        private ToolStripMenuItem topmostMenuItem;
        private ToolStripMenuItem closeMenuItem;
        private Bitmap capturedImage;

        public ImageViewerForm(Bitmap image)
        {
            capturedImage = image;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // PictureBox設定
            pictureBox = new PictureBox
            {
                Image = capturedImage,
                SizeMode = PictureBoxSizeMode.AutoSize,
                Dock = DockStyle.Fill
            };

            // コンテキストメニュー設定
            contextMenu = new ContextMenuStrip();

            topmostMenuItem = new ToolStripMenuItem("常に最前面に表示");
            topmostMenuItem.CheckOnClick = true;
            topmostMenuItem.Checked = true;
            topmostMenuItem.CheckedChanged += TopmostMenuItem_CheckedChanged;

            copyMenuItem = new ToolStripMenuItem("クリップボードにコピー");
            copyMenuItem.Click += CopyMenuItem_Click;

            saveMenuItem = new ToolStripMenuItem("名前を付けて保存...");
            saveMenuItem.Click += SaveMenuItem_Click;

            ocrMenuItem = new ToolStripMenuItem("テキストを抽出 (OCR)");
            ocrMenuItem.Click += OcrMenuItem_Click;

            closeMenuItem = new ToolStripMenuItem("閉じる");
            closeMenuItem.Click += (s, e) => this.Close();

            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                topmostMenuItem,
                new ToolStripSeparator(),
                copyMenuItem,
                saveMenuItem,
                ocrMenuItem,
                new ToolStripSeparator(),
                closeMenuItem
            });

            pictureBox.ContextMenuStrip = contextMenu;

            // Form設定
            this.ClientSize = new Size(capturedImage.Width, capturedImage.Height);
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.Text = "キャプチャ画像";
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ContextMenuStrip = contextMenu;
            this.Controls.Add(pictureBox);

            // キーボードショートカット
            this.KeyPreview = true;
            this.KeyDown += ImageViewerForm_KeyDown;
        }

        private void TopmostMenuItem_CheckedChanged(object? sender, EventArgs e)
        {
            this.TopMost = topmostMenuItem.Checked;
        }

        private void CopyMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                Clipboard.SetImage(capturedImage);
                MessageBox.Show("クリップボードにコピーしました。", "完了",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"コピーに失敗しました: {ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveMenuItem_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PNG画像|*.png|JPEG画像|*.jpg|BMP画像|*.bmp|すべてのファイル|*.*";
                saveDialog.DefaultExt = "png";
                saveDialog.FileName = $"capture_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ImageFormat format = ImageFormat.Png;
                        string ext = Path.GetExtension(saveDialog.FileName).ToLower();

                        switch (ext)
                        {
                            case ".jpg":
                            case ".jpeg":
                                format = ImageFormat.Jpeg;
                                break;
                            case ".bmp":
                                format = ImageFormat.Bmp;
                                break;
                        }

                        capturedImage.Save(saveDialog.FileName, format);
                        MessageBox.Show("保存しました。", "完了",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"保存に失敗しました: {ex.Message}", "エラー",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void OcrMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                ocrMenuItem.Enabled = false;

                // OCRエンジンを取得（日本語優先、なければ英語）
                var language = OcrEngine.AvailableRecognizerLanguages
                    .FirstOrDefault(lang => lang.LanguageTag == "ja") ??
                    OcrEngine.AvailableRecognizerLanguages
                    .FirstOrDefault(lang => lang.LanguageTag == "en");

                if (language == null)
                {
                    MessageBox.Show("OCR言語がインストールされていません。", "エラー",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var engine = OcrEngine.TryCreateFromLanguage(language);
                if (engine == null)
                {
                    MessageBox.Show("OCRエンジンの初期化に失敗しました。", "エラー",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // BitmapをSoftwareBitmapに変換
                using var stream = new MemoryStream();
                capturedImage.Save(stream, ImageFormat.Bmp);
                stream.Position = 0;

                var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                // OCR実行
                var result = await engine.RecognizeAsync(softwareBitmap);

                // 結果を表示
                var ocrResultForm = new OcrResultForm(result.Text);
                ocrResultForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"OCR処理に失敗しました: {ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                ocrMenuItem.Enabled = true;
            }
        }

        private void ImageViewerForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopyMenuItem_Click(null, EventArgs.Empty);
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                SaveMenuItem_Click(null, EventArgs.Empty);
            }
            else if (e.Control && e.KeyCode == Keys.T)
            {
                OcrMenuItem_Click(null, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                capturedImage?.Dispose();
                pictureBox?.Dispose();
                contextMenu?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
