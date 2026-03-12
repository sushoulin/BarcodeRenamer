namespace BarcodeRenamer.Forms
{
    /// <summary>
    /// 人工审核窗体
    /// </summary>
    public class ManualProcessForm : Form
    {
        private readonly string _filePath;
        private PictureBox _pictureBox = null!;
        private TextBox _barcodeTextBox = null!;
        private Button _okButton = null!;
        private Button _skipButton = null!;
        private Label _fileLabel = null!;
        private PictureBox _zoomPictureBox = null!;

        public string? EnteredBarcode { get; private set; }
        public bool IsSkipped { get; private set; }

        public ManualProcessForm(string filePath)
        {
            _filePath = filePath;
            InitializeComponent();
            LoadImage();
        }

        private void InitializeComponent()
        {
            this.Text = "人工审核 - 输入条形码";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Microsoft YaHei UI", 9F);

            // 文件名标签
            _fileLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Text = $"文件: {Path.GetFileName(_filePath)}",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
            };

            // 图片显示区域
            _pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(45, 52, 54)
            };

            // 缩放显示区域
            var zoomPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 250,
                BackColor = Color.FromArgb(30, 39, 46),
                Padding = new Padding(10)
            };

            var zoomLabel = new Label
            {
                Text = "🔍 放大预览",
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold)
            };

            _zoomPictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(45, 52, 54)
            };

            zoomPanel.Controls.Add(_zoomPictureBox);
            zoomPanel.Controls.Add(zoomLabel);

            // 输入区域
            var inputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(15)
            };

            var inputLabel = new Label
            {
                Text = "条形码内容:",
                AutoSize = true,
                Location = new Point(15, 15)
            };

            _barcodeTextBox = new TextBox
            {
                Location = new Point(100, 12),
                Width = 350,
                Height = 30,
                Font = new Font("Consolas", 12F)
            };

            _okButton = new Button
            {
                Text = "确定",
                Location = new Point(470, 10),
                Width = 100,
                Height = 32,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold)
            };
            _okButton.FlatAppearance.BorderSize = 0;
            _okButton.Click += OkButton_Click;

            _skipButton = new Button
            {
                Text = "跳过",
                Location = new Point(580, 10),
                Width = 80,
                Height = 32,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold)
            };
            _skipButton.FlatAppearance.BorderSize = 0;
            _skipButton.Click += SkipButton_Click;

            inputPanel.Controls.Add(inputLabel);
            inputPanel.Controls.Add(_barcodeTextBox);
            inputPanel.Controls.Add(_okButton);
            inputPanel.Controls.Add(_skipButton);

            // 添加控件
            this.Controls.Add(_pictureBox);
            this.Controls.Add(zoomPanel);
            this.Controls.Add(inputPanel);
            this.Controls.Add(_fileLabel);

            // 设置 AcceptButton
            this.AcceptButton = _okButton;

            // 鼠标移动事件用于放大
            _pictureBox.MouseMove += PictureBox_MouseMove;
        }

        private void LoadImage()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
                    var image = Image.FromStream(stream);
                    _pictureBox.Image = new Bitmap(image);
                    _zoomPictureBox.Image = new Bitmap(image);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法加载图片: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PictureBox_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_pictureBox.Image == null) return;

            try
            {
                // 计算缩放后的实际坐标
                var imgRect = GetImageRectangle();
                if (!imgRect.Contains(e.Location)) return;

                var relX = (e.X - imgRect.X) / (float)imgRect.Width;
                var relY = (e.Y - imgRect.Y) / (float)imgRect.Height;

                var sourceX = (int)(relX * _pictureBox.Image.Width);
                var sourceY = (int)(relY * _pictureBox.Image.Height);

                // 创建放大区域
                var zoomSize = 100;
                var zoomRect = new Rectangle(
                    Math.Max(0, sourceX - zoomSize / 2),
                    Math.Max(0, sourceY - zoomSize / 2),
                    zoomSize,
                    zoomSize);

                // 绘制放大图
                var zoomBitmap = new Bitmap(_zoomPictureBox.Width, _zoomPictureBox.Height);
                using (var g = Graphics.FromImage(zoomBitmap))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.DrawImage(_pictureBox.Image, new Rectangle(0, 0, zoomBitmap.Width, zoomBitmap.Height), zoomRect, GraphicsUnit.Pixel);
                }

                _zoomPictureBox.Image?.Dispose();
                _zoomPictureBox.Image = zoomBitmap;
            }
            catch
            {
                // 忽略放大错误
            }
        }

        private Rectangle GetImageRectangle()
        {
            if (_pictureBox.Image == null) return Rectangle.Empty;

            var imgWidth = _pictureBox.Image.Width;
            var imgHeight = _pictureBox.Image.Height;
            var boxWidth = _pictureBox.ClientSize.Width;
            var boxHeight = _pictureBox.ClientSize.Height;

            var ratio = Math.Min((float)boxWidth / imgWidth, (float)boxHeight / imgHeight);
            var displayWidth = (int)(imgWidth * ratio);
            var displayHeight = (int)(imgHeight * ratio);

            var x = (boxWidth - displayWidth) / 2;
            var y = (boxHeight - displayHeight) / 2;

            return new Rectangle(x, y, displayWidth, displayHeight);
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_barcodeTextBox.Text))
            {
                MessageBox.Show("请输入条形码内容", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EnteredBarcode = _barcodeTextBox.Text.Trim();
            IsSkipped = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SkipButton_Click(object? sender, EventArgs e)
        {
            IsSkipped = true;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
