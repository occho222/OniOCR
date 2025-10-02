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
        private bool autoOcr;

        public ImageViewerForm(Bitmap image, bool autoOcr = true)
        {
            capturedImage = image;
            this.autoOcr = autoOcr;
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

            // 自動OCR
            if (autoOcr)
            {
                this.Shown += async (s, e) => await PerformOcrAsync();
            }
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
            await PerformOcrAsync(showResult: true);
        }

        private async Task PerformOcrAsync(bool showResult = false)
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

                // 画像を前処理してOCR精度を向上
                using var processedImage = PreprocessImageForOcr(capturedImage);

                // BitmapをSoftwareBitmapに変換
                using var stream = new MemoryStream();
                processedImage.Save(stream, ImageFormat.Bmp);
                stream.Position = 0;

                var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                // OCR実行
                var result = await engine.RecognizeAsync(softwareBitmap);

                // 結果を表示または自動クリップボード
                if (showResult)
                {
                    var ocrResultForm = new OcrResultForm(result.Text);
                    ocrResultForm.Show();
                }
                else
                {
                    // 自動OCRの場合はクリップボードに送信
                    if (!string.IsNullOrEmpty(result.Text))
                    {
                        Clipboard.SetText(result.Text);
                    }
                }
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

        private Bitmap PreprocessImageForOcr(Bitmap original)
        {
            // 小さい画像の場合は拡大してOCR精度を向上
            int minSize = 300;
            int targetWidth = original.Width;
            int targetHeight = original.Height;

            // 画像が小さすぎる場合は拡大
            if (original.Width < minSize || original.Height < minSize)
            {
                float scale = Math.Max((float)minSize / original.Width, (float)minSize / original.Height);
                // さらに2倍にして精度向上
                scale *= 2;
                targetWidth = (int)(original.Width * scale);
                targetHeight = (int)(original.Height * scale);
            }
            else
            {
                // 通常サイズでも少し拡大
                targetWidth = (int)(original.Width * 1.5);
                targetHeight = (int)(original.Height * 1.5);
            }

            var enlarged = new Bitmap(targetWidth, targetHeight);
            using (Graphics g = Graphics.FromImage(enlarged))
            {
                // 高品質な拡大設定
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                g.DrawImage(original, 0, 0, targetWidth, targetHeight);
            }

            // コントラストとシャープネスを強調
            var enhanced = EnhanceContrast(enlarged);
            enlarged.Dispose();

            return enhanced;
        }

        private Bitmap EnhanceContrast(Bitmap original)
        {
            var result = new Bitmap(original.Width, original.Height);

            using (Graphics g = Graphics.FromImage(result))
            {
                // コントラストを調整するカラーマトリックス
                var colorMatrix = new System.Drawing.Imaging.ColorMatrix(new float[][]
                {
                    new float[] {1.2f, 0, 0, 0, 0},      // Red
                    new float[] {0, 1.2f, 0, 0, 0},      // Green
                    new float[] {0, 0, 1.2f, 0, 0},      // Blue
                    new float[] {0, 0, 0, 1, 0},         // Alpha
                    new float[] {-0.1f, -0.1f, -0.1f, 0, 1}  // Brightness adjustment
                });

                using (var attributes = new System.Drawing.Imaging.ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);

                    g.DrawImage(original,
                        new Rectangle(0, 0, original.Width, original.Height),
                        0, 0, original.Width, original.Height,
                        GraphicsUnit.Pixel,
                        attributes);
                }
            }

            return result;
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
                _ = PerformOcrAsync(showResult: true);
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
