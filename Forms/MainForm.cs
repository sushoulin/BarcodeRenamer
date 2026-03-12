using BarcodeRenamer.Models;
using BarcodeRenamer.Services;
using BarcodeRenamer.Helpers;

namespace BarcodeRenamer.Forms
{
    /// <summary>
    /// 主窗体
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly AppSettings _settings;
        private readonly BarcodeService _barcodeService;
        private readonly FileService _fileService;

        // UI 控件
        private Panel _headerPanel = null!;
        private Panel _configPanel = null!;
        private Panel _controlPanel = null!;
        private Panel _statsPanel = null!;
        private Panel _logPanel = null!;
        private Panel _progressPanel = null!;

        private Label _titleLabel = null!;
        private Label _scanFolderLabel = null!;
        private Label _outputFolderLabel = null!;

        private TextBox _scanFolderTextBox = null!;
        private TextBox _outputFolderTextBox = null!;

        private Button _browseScanFolderButton = null!;
        private Button _browseOutputFolderButton = null!;
        private Button _startScanButton = null!;
        private Button _stopScanButton = null!;
        private Button _manualProcessButton = null!;
        private Button _clearLogButton = null!;
        private Button _openLogFolderButton = null!;
        private Button _saveConfigButton = null!;

        private ProgressBar _progressBar = null!;
        private Label _progressLabel = null!;

        private Label _totalLabel = null!;
        private Label _successLabel = null!;
        private Label _failedLabel = null!;
        private Label _manualLabel = null!;
        private Label _totalValueLabel = null!;
        private Label _successValueLabel = null!;
        private Label _failedValueLabel = null!;
        private Label _manualValueLabel = null!;

        private ListBox _logListBox = null!;
        private ListBox _pendingListBox = null!;

        private GroupBox _configGroupBox = null!;
        private GroupBox _statsGroupBox = null!;
        private GroupBox _pendingGroupBox = null!;
        private GroupBox _logGroupBox = null!;

        private List<string> _pendingFiles = new List<string>();

        public MainForm()
        {
            _settings = AppSettings.Load();
            _barcodeService = new BarcodeService();
            _fileService = new FileService(_barcodeService);
            
            InitializeComponent();
            InitializeEventHandlers();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // 窗体设置
            this.Text = "Barcode Renamer - 图片条形码识别重命名工具";
            this.Size = new Size(1280, 900);
            this.MinimumSize = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei UI", 9F);
            this.BackColor = Color.FromArgb(240, 240, 240);

            // 创建各区域
            CreateHeaderPanel();
            CreateProgressPanel();
            CreateMainLayout();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CreateHeaderPanel()
        {
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(41, 128, 185)
            };

            _titleLabel = new Label
            {
                Text = "📷 Barcode Renamer - 图片条形码识别重命名工具",
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 0, 0, 0)
            };

            _headerPanel.Controls.Add(_titleLabel);
            this.Controls.Add(_headerPanel);
        }

        private void CreateProgressPanel()
        {
            _progressPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(15, 8, 15, 8)
            };

            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Style = ProgressBarStyle.Continuous,
                Height = 25
            };

            _progressLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Text = "就绪",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = false,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 8.5F)
            };

            _progressPanel.Controls.Add(_progressBar);
            _progressPanel.Controls.Add(_progressLabel);
            this.Controls.Add(_progressPanel);
        }

        private void CreateMainLayout()
        {
            // 主内容区域
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 5, 10, 5)
            };

            // 使用嵌套的 SplitContainer 实现所有区域可拖动调整
            // 结构：从上到下依次分割
            // 
            // ┌─────────────────────┐
            // │ 配置区域            │ ← split1.Panel1
            // ├─────────────────────┤ ← split1.Splitter
            // │ 控制按钮            │ ← split1.Panel2 → split2.Panel1
            // ├─────────────────────┤ ← split2.Splitter
            // │ 统计信息            │ ← split2.Panel2 → split3.Panel1
            // ├─────────────────────┤ ← split3.Splitter
            // │ 待处理文件          │ ← split3.Panel2 → split4.Panel1
            // ├─────────────────────┤ ← split4.Splitter
            // │ 操作日志            │ ← split4.Panel2
            // └─────────────────────┘

            // 创建所有区域面板
            CreateConfigPanel();
            CreateControlPanel();
            CreateStatsPanel();
            CreatePendingPanel();
            CreateLogPanel();

            // 创建嵌套的 SplitContainer
            // split4: 待处理 vs 日志
            var split4 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 150,
                SplitterWidth = 6,
                BackColor = Color.FromArgb(200, 200, 200)
            };
            split4.Panel1.Controls.Add(_pendingGroupBox);
            split4.Panel2.Controls.Add(_logPanel);
            split4.Panel1MinSize = 80;
            split4.Panel2MinSize = 100;

            // split3: 统计 vs (待处理 + 日志)
            var split3 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 70,
                SplitterWidth = 6,
                BackColor = Color.FromArgb(200, 200, 200)
            };
            split3.Panel1.Controls.Add(_statsPanel);
            split3.Panel2.Controls.Add(split4);
            split3.Panel1MinSize = 60;
            split3.Panel2MinSize = 200;

            // split2: 控制 vs (统计 + 待处理 + 日志)
            var split2 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 60,
                SplitterWidth = 6,
                BackColor = Color.FromArgb(200, 200, 200)
            };
            split2.Panel1.Controls.Add(_controlPanel);
            split2.Panel2.Controls.Add(split3);
            split2.Panel1MinSize = 50;
            split2.Panel2MinSize = 300;

            // split1: 配置 vs (控制 + 统计 + 待处理 + 日志)
            var split1 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 110,
                SplitterWidth = 6,
                BackColor = Color.FromArgb(200, 200, 200)
            };
            split1.Panel1.Controls.Add(_configPanel);
            split1.Panel2.Controls.Add(split2);
            split1.Panel1MinSize = 80;
            split1.Panel2MinSize = 350;

            contentPanel.Controls.Add(split1);
            this.Controls.Add(contentPanel);
        }

        private void CreateConfigPanel()
        {
            _configPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(5)
            };

            _configGroupBox = new GroupBox
            {
                Text = " 📁 配置设置 ",
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 5)
            };

            var innerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(5)
            };

            innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));

            // 扫描文件夹
            _scanFolderLabel = new Label
            {
                Text = "扫描文件夹:",
                AutoSize = true,
                Margin = new Padding(5, 8, 10, 5),
                Font = new Font("Microsoft YaHei UI", 9F)
            };

            _scanFolderTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 5, 10, 5),
                Font = new Font("Microsoft YaHei UI", 9.5F),
                PlaceholderText = "选择要扫描的图片文件夹..."
            };

            _browseScanFolderButton = new Button
            {
                Text = "浏览...",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 5, 5),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            _browseScanFolderButton.FlatAppearance.BorderSize = 0;

            innerPanel.Controls.Add(_scanFolderLabel, 0, 0);
            innerPanel.Controls.Add(_scanFolderTextBox, 1, 0);
            innerPanel.Controls.Add(_browseScanFolderButton, 2, 0);

            // 输出文件夹
            _outputFolderLabel = new Label
            {
                Text = "输出文件夹:",
                AutoSize = true,
                Margin = new Padding(5, 8, 10, 5),
                Font = new Font("Microsoft YaHei UI", 9F)
            };

            _outputFolderTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 5, 10, 5),
                Font = new Font("Microsoft YaHei UI", 9.5F),
                PlaceholderText = "选择重命名后文件的输出文件夹..."
            };

            _browseOutputFolderButton = new Button
            {
                Text = "浏览...",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 5, 5),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            _browseOutputFolderButton.FlatAppearance.BorderSize = 0;

            innerPanel.Controls.Add(_outputFolderLabel, 0, 1);
            innerPanel.Controls.Add(_outputFolderTextBox, 1, 1);
            innerPanel.Controls.Add(_browseOutputFolderButton, 2, 1);

            _configGroupBox.Controls.Add(innerPanel);
            _configPanel.Controls.Add(_configGroupBox);
        }

        private void CreateControlPanel()
        {
            _controlPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(5, 8, 5, 8)
            };

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0)
            };

            _startScanButton = new Button
            {
                Text = "▶ 开始扫描",
                Size = new Size(130, 40),
                Margin = new Padding(5, 3, 10, 3),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
            };
            _startScanButton.FlatAppearance.BorderSize = 0;

            _stopScanButton = new Button
            {
                Text = "⏹ 停止扫描",
                Size = new Size(130, 40),
                Margin = new Padding(0, 3, 10, 3),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                Enabled = false
            };
            _stopScanButton.FlatAppearance.BorderSize = 0;

            _manualProcessButton = new Button
            {
                Text = "✏ 人工审核",
                Size = new Size(130, 40),
                Margin = new Padding(0, 3, 10, 3),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
            };
            _manualProcessButton.FlatAppearance.BorderSize = 0;

            _saveConfigButton = new Button
            {
                Text = "💾 保存配置",
                Size = new Size(120, 40),
                Margin = new Padding(0, 3, 10, 3),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
            };
            _saveConfigButton.FlatAppearance.BorderSize = 0;

            flowPanel.Controls.Add(_startScanButton);
            flowPanel.Controls.Add(_stopScanButton);
            flowPanel.Controls.Add(_manualProcessButton);
            flowPanel.Controls.Add(_saveConfigButton);

            _controlPanel.Controls.Add(flowPanel);
        }

        private void CreateStatsPanel()
        {
            _statsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(5)
            };

            _statsGroupBox = new GroupBox
            {
                Text = " 📊 统计信息 ",
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 5, 10, 5)
            };

            var statsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 8,
                RowCount = 1,
                Padding = new Padding(5, 3, 5, 3)
            };

            // 设置列样式
            for (int i = 0; i < 8; i++)
            {
                statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            }

            _totalLabel = new Label { Text = "扫描总数:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray, Font = new Font("Microsoft YaHei UI", 9F) };
            _totalValueLabel = new Label { Text = "0", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(41, 128, 185), Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold) };
            
            _successLabel = new Label { Text = "成功:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray, Font = new Font("Microsoft YaHei UI", 9F) };
            _successValueLabel = new Label { Text = "0", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(46, 204, 113), Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold) };
            
            _failedLabel = new Label { Text = "失败:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray, Font = new Font("Microsoft YaHei UI", 9F) };
            _failedValueLabel = new Label { Text = "0", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(231, 76, 60), Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold) };
            
            _manualLabel = new Label { Text = "人工:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray, Font = new Font("Microsoft YaHei UI", 9F) };
            _manualValueLabel = new Label { Text = "0", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(155, 89, 182), Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold) };

            statsLayout.Controls.Add(_totalLabel, 0, 0);
            statsLayout.Controls.Add(_totalValueLabel, 1, 0);
            statsLayout.Controls.Add(_successLabel, 2, 0);
            statsLayout.Controls.Add(_successValueLabel, 3, 0);
            statsLayout.Controls.Add(_failedLabel, 4, 0);
            statsLayout.Controls.Add(_failedValueLabel, 5, 0);
            statsLayout.Controls.Add(_manualLabel, 6, 0);
            statsLayout.Controls.Add(_manualValueLabel, 7, 0);

            _statsGroupBox.Controls.Add(statsLayout);
            _statsPanel.Controls.Add(_statsGroupBox);
        }

        private void CreatePendingPanel()
        {
            _pendingGroupBox = new GroupBox
            {
                Text = " ⚠ 待处理文件 (识别失败，双击进行人工处理) ",
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 8, 8, 8),
                BackColor = Color.White
            };

            _pendingListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false,
                Font = new Font("Consolas", 9.5F),
                BackColor = Color.FromArgb(255, 253, 253),
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = SelectionMode.One
            };

            _pendingGroupBox.Controls.Add(_pendingListBox);
        }

        private void CreateLogPanel()
        {
            _logPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(5)
            };

            _logGroupBox = new GroupBox
            {
                Text = " 📝 操作日志 ",
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 8, 8, 8)
            };

            var logContainer = new Panel
            {
                Dock = DockStyle.Fill
            };

            var logHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            _clearLogButton = new Button
            {
                Text = "清空日志",
                Size = new Size(85, 26),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(logHeader.Width - 190, 4),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 8.5F)
            };
            _clearLogButton.FlatAppearance.BorderSize = 0;

            _openLogFolderButton = new Button
            {
                Text = "打开日志目录",
                Size = new Size(100, 26),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(logHeader.Width - 95, 4),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 8.5F)
            };
            _openLogFolderButton.FlatAppearance.BorderSize = 0;

            // 更新按钮位置
            logHeader.Resize += (s, e) =>
            {
                _clearLogButton.Left = logHeader.Width - 190;
                _openLogFolderButton.Left = logHeader.Width - 95;
            };

            logHeader.Controls.Add(_clearLogButton);
            logHeader.Controls.Add(_openLogFolderButton);

            _logListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false,
                Font = new Font("Consolas", 9.5F),
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            logContainer.Controls.Add(_logListBox);
            logContainer.Controls.Add(logHeader);

            _logGroupBox.Controls.Add(logContainer);
            _logPanel.Controls.Add(_logGroupBox);
        }

        private void InitializeEventHandlers()
        {
            // 按钮事件
            _browseScanFolderButton.Click += BrowseScanFolderButton_Click;
            _browseOutputFolderButton.Click += BrowseOutputFolderButton_Click;
            _startScanButton.Click += StartScanButton_Click;
            _stopScanButton.Click += StopScanButton_Click;
            _manualProcessButton.Click += ManualProcessButton_Click;
            _clearLogButton.Click += ClearLogButton_Click;
            _openLogFolderButton.Click += OpenLogFolderButton_Click;
            _saveConfigButton.Click += SaveConfigButton_Click;

            // 文件服务事件
            _fileService.FileProcessed += FileService_FileProcessed;
            _fileService.ProgressChanged += FileService_ProgressChanged;
            _fileService.ScanCompleted += FileService_ScanCompleted;

            // 待处理文件双击
            _pendingListBox.DoubleClick += PendingListBox_DoubleClick;
        }

        private void LoadSettings()
        {
            _scanFolderTextBox.Text = _settings.ScanFolder;
            _outputFolderTextBox.Text = _settings.OutputFolder;

            AddLog("应用程序启动");
            AddLog($"配置已加载: 扫描文件夹={_settings.ScanFolder}, 输出文件夹={_settings.OutputFolder}");
        }

        #region 事件处理

        private void BrowseScanFolderButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择扫描文件夹",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _scanFolderTextBox.Text = dialog.SelectedPath;
                _settings.ScanFolder = dialog.SelectedPath;
                AddLog($"扫描文件夹已设置: {dialog.SelectedPath}");
            }
        }

        private void BrowseOutputFolderButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择输出文件夹",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _outputFolderTextBox.Text = dialog.SelectedPath;
                _settings.OutputFolder = dialog.SelectedPath;
                AddLog($"输出文件夹已设置: {dialog.SelectedPath}");
            }
        }

        private async void StartScanButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_scanFolderTextBox.Text))
            {
                MessageBox.Show("请先设置扫描文件夹", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_outputFolderTextBox.Text))
            {
                MessageBox.Show("请先设置输出文件夹", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _settings.ScanFolder = _scanFolderTextBox.Text;
            _settings.OutputFolder = _outputFolderTextBox.Text;
            _settings.Save();

            _startScanButton.Enabled = false;
            _stopScanButton.Enabled = true;
            _progressBar.Value = 0;
            _pendingFiles.Clear();
            _pendingListBox.Items.Clear();

            AddLog("开始扫描...");

            await _fileService.StartScanAsync(_settings);
        }

        private void StopScanButton_Click(object? sender, EventArgs e)
        {
            _fileService.StopScan();
            AddLog("正在停止扫描...");
        }

        private void ManualProcessButton_Click(object? sender, EventArgs e)
        {
            if (_pendingListBox.SelectedItem == null)
            {
                MessageBox.Show("请先从待处理列表中选择一个文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var filePath = _pendingListBox.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                ProcessManualInput(filePath);
            }
        }

        private void PendingListBox_DoubleClick(object? sender, EventArgs e)
        {
            if (_pendingListBox.SelectedItem != null)
            {
                var filePath = _pendingListBox.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    ProcessManualInput(filePath);
                }
            }
        }

        private async void ProcessManualInput(string filePath)
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "请输入条形码内容:",
                "人工输入条形码",
                "",
                -1, -1);

            if (!string.IsNullOrEmpty(input))
            {
                var result = await _fileService.ManualProcessAsync(filePath, input, _settings);
                
                if (result.Success)
                {
                    AddLog($"人工处理成功: {Path.GetFileName(filePath)} -> {Path.GetFileName(result.NewPath)}");
                    _pendingFiles.Remove(filePath);
                    _pendingListBox.Items.Remove(filePath);
                    UpdateStatistics();
                }
                else
                {
                    AddLog($"人工处理失败: {result.ErrorMessage}");
                }
            }
        }

        private void SaveConfigButton_Click(object? sender, EventArgs e)
        {
            _settings.ScanFolder = _scanFolderTextBox.Text;
            _settings.OutputFolder = _outputFolderTextBox.Text;
            _settings.Save();
            MessageBox.Show("配置已保存", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ClearLogButton_Click(object? sender, EventArgs e)
        {
            _logListBox.Items.Clear();
            Logger.ClearLogs();
            AddLog("日志已清空");
        }

        private void OpenLogFolderButton_Click(object? sender, EventArgs e)
        {
            var logDir = Logger.GetLogDirectory();
            if (Directory.Exists(logDir))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = logDir,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }

        private void FileService_FileProcessed(object? sender, ProcessResult e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => FileService_FileProcessed(sender, e));
                return;
            }

            if (e.Success)
            {
                AddLog($"✓ {Path.GetFileName(e.OriginalPath)} -> {e.Barcode}");
            }
            else
            {
                AddLog($"✗ {Path.GetFileName(e.OriginalPath)}: {e.ErrorMessage}");
                _pendingFiles.Add(e.OriginalPath);
                _pendingListBox.Items.Add(e.OriginalPath);
            }

            UpdateStatistics();
        }

        private void FileService_ProgressChanged(object? sender, int e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => FileService_ProgressChanged(sender, e));
                return;
            }

            _progressBar.Value = Math.Min(100, e);
            _progressLabel.Text = $"处理进度: {e}%";
        }

        private void FileService_ScanCompleted(object? sender, bool completed)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => FileService_ScanCompleted(sender, completed));
                return;
            }

            _startScanButton.Enabled = true;
            _stopScanButton.Enabled = false;
            _progressLabel.Text = completed ? "扫描完成" : "扫描已取消";
        }

        #endregion

        private void UpdateStatistics()
        {
            _totalValueLabel.Text = _fileService.Statistics.TotalScanned.ToString();
            _successValueLabel.Text = _fileService.Statistics.SuccessCount.ToString();
            _failedValueLabel.Text = _fileService.Statistics.FailedCount.ToString();
            _manualValueLabel.Text = _fileService.Statistics.ManualCount.ToString();
        }

        private void AddLog(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => AddLog(message));
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            _logListBox.Items.Insert(0, $"[{timestamp}] {message}");
            Logger.Log(message);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_fileService.IsScanning)
            {
                var result = MessageBox.Show(
                    "扫描正在进行中，确定要退出吗？",
                    "确认退出",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                _fileService.StopScan();
            }

            _settings.ScanFolder = _scanFolderTextBox.Text;
            _settings.OutputFolder = _outputFolderTextBox.Text;
            _settings.Save();

            base.OnFormClosing(e);
        }
    }
}
