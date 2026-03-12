using BarcodeRenamer.Models;
using System.Text;

namespace BarcodeRenamer.Helpers
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    public static class Logger
    {
        private static readonly string LogDirectory;
        private static readonly string LogFilePath;
        private static readonly object LockObject = new object();

        static Logger()
        {
            LogDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BarcodeRenamer",
                "logs");

            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            LogFilePath = Path.Combine(LogDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            try
            {
                lock (LockObject)
                {
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // 忽略日志写入失败
            }
        }

        /// <summary>
        /// 获取日志目录
        /// </summary>
        public static string GetLogDirectory() => LogDirectory;

        /// <summary>
        /// 读取今日日志
        /// </summary>
        public static string ReadTodayLogs()
        {
            try
            {
                if (File.Exists(LogFilePath))
                {
                    return File.ReadAllText(LogFilePath, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Log($"读取日志失败: {ex.Message}", LogLevel.Error);
            }
            return string.Empty;
        }

        /// <summary>
        /// 清空日志
        /// </summary>
        public static void ClearLogs()
        {
            try
            {
                lock (LockObject)
                {
                    if (File.Exists(LogFilePath))
                    {
                        File.Delete(LogFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"清空日志失败: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
