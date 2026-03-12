using BarcodeRenamer.Models;

namespace BarcodeRenamer.Services
{
    /// <summary>
    /// 文件处理服务
    /// </summary>
    public class FileService
    {
        private readonly BarcodeService _barcodeService;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isScanning;

        public event EventHandler<ProcessResult>? FileProcessed;
        public event EventHandler<int>? ProgressChanged;
        public event EventHandler<bool>? ScanCompleted;

        public Statistics Statistics { get; private set; } = new Statistics();
        public bool IsScanning => _isScanning;

        public FileService(BarcodeService barcodeService)
        {
            _barcodeService = barcodeService;
        }

        /// <summary>
        /// 开始扫描
        /// </summary>
        public async Task StartScanAsync(AppSettings settings)
        {
            if (_isScanning)
            {
                Logger.Log("扫描已在进行中", LogLevel.Warning);
                return;
            }

            if (string.IsNullOrEmpty(settings.ScanFolder) || !Directory.Exists(settings.ScanFolder))
            {
                Logger.Log($"扫描文件夹无效: {settings.ScanFolder}", LogLevel.Error);
                return;
            }

            if (string.IsNullOrEmpty(settings.OutputFolder))
            {
                Logger.Log("输出文件夹未设置", LogLevel.Error);
                return;
            }

            if (!Directory.Exists(settings.OutputFolder))
            {
                try
                {
                    Directory.CreateDirectory(settings.OutputFolder);
                }
                catch (Exception ex)
                {
                    Logger.Log($"创建输出文件夹失败: {ex.Message}", LogLevel.Error);
                    return;
                }
            }

            _isScanning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            Statistics.Reset();

            Logger.Log($"开始扫描: {settings.ScanFolder}", LogLevel.Info);

            try
            {
                var files = GetImageFiles(settings.ScanFolder, settings.SupportedFormats);
                var totalFiles = files.Count;
                var processedFiles = 0;

                foreach (var file in files)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        Logger.Log("扫描已取消", LogLevel.Info);
                        break;
                    }

                    var result = await ProcessFileAsync(file, settings);
                    Statistics.TotalScanned++;

                    if (result.Success)
                    {
                        Statistics.SuccessCount++;
                    }
                    else if (result.ManualProcessed)
                    {
                        Statistics.ManualCount++;
                    }
                    else
                    {
                        Statistics.FailedCount++;
                    }

                    processedFiles++;
                    var progress = (int)((double)processedFiles / totalFiles * 100);
                    ProgressChanged?.Invoke(this, progress);

                    FileProcessed?.Invoke(this, result);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"扫描异常: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                _isScanning = false;
                ScanCompleted?.Invoke(this, !_cancellationTokenSource.Token.IsCancellationRequested);
                Logger.Log($"扫描完成: 总数={Statistics.TotalScanned}, 成功={Statistics.SuccessCount}, 失败={Statistics.FailedCount}, 人工={Statistics.ManualCount}", LogLevel.Info);
            }
        }

        /// <summary>
        /// 停止扫描
        /// </summary>
        public void StopScan()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                Logger.Log("正在停止扫描...", LogLevel.Info);
            }
        }

        /// <summary>
        /// 处理单个文件
        /// </summary>
        public async Task<ProcessResult> ProcessFileAsync(string filePath, AppSettings settings)
        {
            var result = new ProcessResult
            {
                OriginalPath = filePath
            };

            try
            {
                // 识别条形码
                var barcode = await Task.Run(() => _barcodeService.RecognizeBarcode(filePath));

                if (!string.IsNullOrEmpty(barcode))
                {
                    result.Barcode = barcode;
                    result.Success = true;

                    // 生成新文件名并移动文件
                    var newFileName = GenerateFileName(barcode, Path.GetExtension(filePath));
                    var newPath = Path.Combine(settings.OutputFolder, newFileName);

                    // 处理文件名冲突
                    newPath = GetUniqueFilePath(newPath);

                    await Task.Run(() => File.Move(filePath, newPath));
                    result.NewPath = newPath;

                    Logger.Log($"文件重命名成功: {Path.GetFileName(filePath)} -> {newFileName}", LogLevel.Info);
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "无法识别条形码";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                Logger.Log($"处理文件失败: {filePath} - {ex.Message}", LogLevel.Error);
            }

            return result;
        }

        /// <summary>
        /// 手动处理文件
        /// </summary>
        public async Task<ProcessResult> ManualProcessAsync(string filePath, string barcode, AppSettings settings)
        {
            var result = new ProcessResult
            {
                OriginalPath = filePath,
                Barcode = barcode,
                Success = true,
                ManualProcessed = true
            };

            try
            {
                var newFileName = GenerateFileName(barcode, Path.GetExtension(filePath));
                var newPath = Path.Combine(settings.OutputFolder, newFileName);
                newPath = GetUniqueFilePath(newPath);

                await Task.Run(() => File.Move(filePath, newPath));
                result.NewPath = newPath;

                Logger.Log($"人工处理成功: {Path.GetFileName(filePath)} -> {newFileName}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取图片文件列表
        /// </summary>
        private List<string> GetImageFiles(string folder, List<string> supportedFormats)
        {
            var files = new List<string>();

            try
            {
                var allFiles = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly);
                files = allFiles
                    .Where(f => supportedFormats.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Log($"获取文件列表失败: {ex.Message}", LogLevel.Error);
            }

            return files;
        }

        /// <summary>
        /// 生成文件名
        /// </summary>
        private string GenerateFileName(string barcode, string extension)
        {
            var pattern = AppSettings.Load().RenamePattern;
            var fileName = pattern.Replace("{barcode}", barcode);
            return fileName + extension.ToLowerInvariant();
        }

        /// <summary>
        /// 获取唯一的文件路径
        /// </summary>
        private string GetUniqueFilePath(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return filePath;
            }

            var directory = Path.GetDirectoryName(filePath)!;
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);

            var counter = 1;
            string newPath;

            do
            {
                newPath = Path.Combine(directory, $"{fileNameWithoutExt}_{counter}{extension}");
                counter++;
            } while (File.Exists(newPath));

            return newPath;
        }
    }
}
