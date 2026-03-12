using Newtonsoft.Json;
using BarcodeRenamer.Helpers;

namespace BarcodeRenamer.Models
{
    /// <summary>
    /// 应用程序配置模型
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 扫描文件夹路径
        /// </summary>
        public string ScanFolder { get; set; } = string.Empty;

        /// <summary>
        /// 输出文件夹路径
        /// </summary>
        public string OutputFolder { get; set; } = string.Empty;

        /// <summary>
        /// 支持的图片格式
        /// </summary>
        public List<string> SupportedFormats { get; set; } = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff" };

        /// <summary>
        /// 是否自动扫描
        /// </summary>
        public bool AutoScan { get; set; } = false;

        /// <summary>
        /// 重命名模式
        /// </summary>
        public string RenamePattern { get; set; } = "{barcode}";

        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static readonly string ConfigFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BarcodeRenamer",
            "config.json");

        /// <summary>
        /// 加载配置
        /// </summary>
        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"加载配置文件失败: {ex.Message}", LogLevel.Error);
            }
            return new AppSettings();
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(ConfigFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
                Logger.Log("配置已保存", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Log($"保存配置文件失败: {ex.Message}", LogLevel.Error);
            }
        }
    }

    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// 处理结果
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// 原文件路径
        /// </summary>
        public string OriginalPath { get; set; } = string.Empty;

        /// <summary>
        /// 新文件路径
        /// </summary>
        public string NewPath { get; set; } = string.Empty;

        /// <summary>
        /// 识别的条形码内容
        /// </summary>
        public string Barcode { get; set; } = string.Empty;

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 是否人工处理
        /// </summary>
        public bool ManualProcessed { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// 统计信息
    /// </summary>
    public class Statistics
    {
        /// <summary>
        /// 扫描总数
        /// </summary>
        public int TotalScanned { get; set; }

        /// <summary>
        /// 成功总数
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失败总数
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// 人工处理总数
        /// </summary>
        public int ManualCount { get; set; }

        /// <summary>
        /// 重置统计
        /// </summary>
        public void Reset()
        {
            TotalScanned = 0;
            SuccessCount = 0;
            FailedCount = 0;
            ManualCount = 0;
        }
    }
}
