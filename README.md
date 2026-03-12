# BarcodeRenamer

📷 **图片条形码识别重命名工具** - 一个基于 C# WinForms 开发的桌面应用程序，用于识别图片中的条形码并根据识别结果重命名文件。

## 功能特性

- **可视化操作界面**：简洁直观、美观的 Windows 窗体应用程序
- **配置管理**：配置扫描文件夹、输出文件夹，支持配置持久化
- **批量条形码识别**：支持识别多种常见条形码格式，多角度识别，尽量提高识别成功率
- **人工审核功能**：识别失败时可手动输入条形码内容
- **文件批量重命名**：根据条形码内容自动重命名文件并移动到输出目录
- **自动扫描**：支持开始扫描、停止扫描操作
- **进度显示**：实时显示扫描和处理进度
- **统计信息**：显示扫描总数、成功总数、失败总数、人工处理总数
- **操作日志**：记录所有操作和错误信息

## 支持的条形码格式

### 一维码
- EAN-13
- EAN-8
- UPC-A
- UPC-E
- Code 128
- Code 39
- Code 93
- Codabar
- ITF
- RSS-14

### 二维码
- QR Code
- Data Matrix
- PDF 417
- Aztec

## 系统要求

- Windows 10 或更高版本
- .NET 8.0 Runtime

## 安装与运行

### 从源码构建

1. 确保已安装 .NET 8.0 SDK
2. 克隆仓库：
   ```bash
   git clone https://github.com/sushoulin/BarcodeRenamer.git
   cd BarcodeRenamer
   ```
3. 构建项目：
   ```bash
   dotnet build
   ```
4. 运行项目：
   ```bash
   dotnet run
   ```

### 发布独立应用

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 使用说明

1. **配置文件夹**：
   - 设置"扫描文件夹"：包含待处理图片的文件夹
   - 设置"输出文件夹"：重命名后的文件将移动到此文件夹

2. **开始扫描**：
   - 点击"开始扫描"按钮开始处理
   - 程序将自动识别图片中的条形码并重命名文件

3. **人工审核**：
   - 识别失败的文件会显示在"待处理文件"列表中
   - 双击列表项或选择后点击"人工审核"按钮手动输入条形码

4. **查看日志**：
   - 所有操作记录在日志区域
   - 可点击"打开日志目录"查看详细日志文件

## 项目结构

```
BarcodeRenamer/
├── Forms/
│   ├── MainForm.cs          # 主窗体
│   └── ManualProcessForm.cs # 人工审核窗体
├── Models/
│   └── AppSettings.cs       # 配置模型和统计模型
├── Services/
│   ├── BarcodeService.cs    # 条形码识别服务
│   └── FileService.cs       # 文件处理服务
├── Helpers/
│   └── Logger.cs            # 日志记录器
├── Program.cs               # 程序入口
├── appsettings.json         # 默认配置文件
└── BarcodeRenamer.csproj    # 项目文件
```

## 技术栈

- **框架**：.NET 8.0 Windows Forms
- **条形码识别**：ZXing.Net
- **配置管理**：Newtonsoft.Json

## 许可证

MIT License

## 贡献

欢迎提交 Issue 和 Pull Request！
