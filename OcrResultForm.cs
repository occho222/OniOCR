namespace WinFormsApp1
{
    public class OcrResultForm : Form
    {
        private TextBox textBox;
        private Button copyButton;
        private Button closeButton;

        public OcrResultForm(string ocrText)
        {
            InitializeComponents(ocrText);
        }

        private void InitializeComponents(string ocrText)
        {
            // TextBox設定
            textBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Text = ocrText,
                Font = new Font("Yu Gothic UI", 10),
                ReadOnly = false
            };

            // コピーボタン
            copyButton = new Button
            {
                Text = "クリップボードにコピー",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            copyButton.Click += CopyButton_Click;

            // 閉じるボタン
            closeButton = new Button
            {
                Text = "閉じる",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            closeButton.Click += (s, e) => this.Close();

            // Form設定
            this.Text = "OCR結果";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Controls.Add(textBox);
            this.Controls.Add(copyButton);
            this.Controls.Add(closeButton);

            // キーボードショートカット
            this.KeyPreview = true;
            this.KeyDown += OcrResultForm_KeyDown;
        }

        private void CopyButton_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                Clipboard.SetText(textBox.Text);
                MessageBox.Show("クリップボードにコピーしました。", "完了",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OcrResultForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C && !textBox.Focused)
            {
                CopyButton_Click(null, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
