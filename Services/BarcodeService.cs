using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using System.Drawing;
using System.Drawing.Imaging;
using BarcodeRenamer.Models;
using BarcodeRenamer.Helpers;

namespace BarcodeRenamer.Services
{
    /// <summary>
    /// 条形码识别服务
    /// </summary>
    public class BarcodeService
    {
        private readonly BarcodeReader _barcodeReader;
        private readonly List<BarcodeFormat> _supportedFormats;

        // 空白区域检测阈值
        private const int WhiteThreshold = 240;  // 认为是白色的亮度阈值
        private const double NonWhitePixelThreshold = 0.02;  // 允许的非白像素比例 (2%)
        private const double CropHeightPercent = 0.005;  // 每次裁剪高度百分比 (0.5%)
        private const double MaxCropPercent = 0.3;  // 最大裁剪比例 (30%)

        public BarcodeService()
        {
            _supportedFormats = new List<BarcodeFormat>
            {
                // 一维码
                BarcodeFormat.EAN_13,
                BarcodeFormat.EAN_8,
                BarcodeFormat.UPC_A,
                BarcodeFormat.UPC_E,
                BarcodeFormat.CODE_128,
                BarcodeFormat.CODE_39,
                BarcodeFormat.CODE_93,
                BarcodeFormat.CODABAR,
                BarcodeFormat.ITF,
                BarcodeFormat.RSS_14,
                // 二维码
                BarcodeFormat.QR_CODE,
                BarcodeFormat.DATA_MATRIX,
                BarcodeFormat.PDF_417,
                BarcodeFormat.AZTEC
            };

            _barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                TryInverted = true,
                Options = new DecodingOptions
                {
                    PossibleFormats = _supportedFormats,
                    TryHarder = true,
                    ReturnCodabarStartEnd = true,
                    PureBarcode = false
                }
            };
        }

        /// <summary>
        /// 识别图片中的条形码
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        /// <returns>识别结果</returns>
        public string RecognizeBarcode(string imagePath)
        {
            try
            {
                if (!File.Exists(imagePath))
                {
                    Logger.Log($"文件不存在: {imagePath}", LogLevel.Warning);
                    return string.Empty;
                }

                // 1. 直接识别
                var result = TryRecognize(imagePath);
                if (!string.IsNullOrEmpty(result))
                {
                    Logger.Log($"直接识别成功: {imagePath} -> {result}", LogLevel.Info);
                    return result;
                }

                // 2. 尝试裁剪顶部空白区域后识别
                result = TryRecognizeWithCropTop(imagePath);
                if (!string.IsNullOrEmpty(result))
                {
                    Logger.Log($"裁剪顶部空白后识别成功: {imagePath} -> {result}", LogLevel.Info);
                    return result;
                }

                // 3. 尝试其他图像预处理
                result = TryRecognizeWithPreprocessing(imagePath);
                if (!string.IsNullOrEmpty(result))
                {
                    Logger.Log($"预处理后识别成功: {imagePath} -> {result}", LogLevel.Info);
                    return result;
                }

                // 4. 尝试裁剪 + 预处理组合
                result = TryRecognizeWithCropAndPreprocessing(imagePath);
                if (!string.IsNullOrEmpty(result))
                {
                    Logger.Log($"裁剪+预处理后识别成功: {imagePath} -> {result}", LogLevel.Info);
                    return result;
                }

                Logger.Log($"无法识别条形码: {imagePath}", LogLevel.Warning);
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Log($"识别条形码异常: {imagePath} - {ex.Message}", LogLevel.Error);
                return string.Empty;
            }
        }

        /// <summary>
        /// 尝试识别
        /// </summary>
        private string TryRecognize(string imagePath)
        {
            try
            {
                using var bitmap = new Bitmap(imagePath);
                var result = _barcodeReader.Decode(bitmap);
                return result?.Text ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 尝试识别（Bitmap版本）
        /// </summary>
        private string TryRecognizeBitmap(Bitmap bitmap)
        {
            try
            {
                var result = _barcodeReader.Decode(bitmap);
                return result?.Text ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 尝试裁剪顶部空白区域后识别
        /// </summary>
        private string TryRecognizeWithCropTop(string imagePath)
        {
            try
            {
                using var originalBitmap = new Bitmap(imagePath);
                var height = originalBitmap.Height;
                var maxCropHeight = (int)(height * MaxCropPercent);
                var cropStep = Math.Max(1, (int)(height * CropHeightPercent));

                // 逐行扫描，找到第一个非空白行
                int cropHeight = 0;
                for (int y = 0; y < maxCropHeight; y += cropStep)
                {
                    if (!IsBlankRow(originalBitmap, y))
                    {
                        cropHeight = y;
                        break;
                    }
                    cropHeight = y + cropStep;
                }

                // 如果有空白区域需要裁剪
                if (cropHeight > 0 && cropHeight < maxCropHeight)
                {
                    Logger.Log($"检测到顶部空白区域，裁剪高度: {cropHeight}px", LogLevel.Debug);

                    // 逐步尝试不同裁剪高度
                    for (int ch = cropStep; ch <= cropHeight; ch += cropStep)
                    {
                        using var croppedBitmap = CropTop(originalBitmap, ch);
                        var result = TryRecognizeBitmap(croppedBitmap);
                        if (!string.IsNullOrEmpty(result))
                        {
                            return result;
                        }
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 尝试图像预处理后识别
        /// </summary>
        private string TryRecognizeWithPreprocessing(string imagePath)
        {
            try
            {
                using var bitmap = new Bitmap(imagePath);

                // 尝试不同分辨率
                foreach (var scale in new[] { 2, 3, 4 })
                {
                    using var scaledBitmap = ScaleBitmap(bitmap, scale);
                    var result = TryRecognizeBitmap(scaledBitmap);
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }

                // 尝试灰度图
                using var grayBitmap = ConvertToGrayscale(bitmap);
                var resultGray = TryRecognizeBitmap(grayBitmap);
                if (!string.IsNullOrEmpty(resultGray))
                {
                    return resultGray;
                }

                // 尝试增强对比度
                using var enhancedBitmap = EnhanceContrast(bitmap);
                var resultEnhanced = TryRecognizeBitmap(enhancedBitmap);
                if (!string.IsNullOrEmpty(resultEnhanced))
                {
                    return resultEnhanced;
                }

                // 尝试二值化
                using var binaryBitmap = Binarize(bitmap);
                var resultBinary = TryRecognizeBitmap(binaryBitmap);
                if (!string.IsNullOrEmpty(resultBinary))
                {
                    return resultBinary;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 尝试裁剪 + 预处理组合
        /// </summary>
        private string TryRecognizeWithCropAndPreprocessing(string imagePath)
        {
            try
            {
                using var originalBitmap = new Bitmap(imagePath);
                var height = originalBitmap.Height;
                var maxCropHeight = (int)(height * MaxCropPercent);
                var cropStep = Math.Max(1, (int)(height * CropHeightPercent));

                // 逐步裁剪并尝试各种预处理
                for (int ch = 0; ch <= maxCropHeight; ch += cropStep * 5)  // 步长加大
                {
                    using var croppedBitmap = ch > 0 ? CropTop(originalBitmap, ch) : originalBitmap;

                    // 尝试缩放
                    foreach (var scale in new[] { 2, 3 })
                    {
                        using var scaledBitmap = ScaleBitmap(croppedBitmap, scale);
                        var result = TryRecognizeBitmap(scaledBitmap);
                        if (!string.IsNullOrEmpty(result))
                        {
                            return result;
                        }
                    }

                    // 尝试二值化
                    using var binaryBitmap = Binarize(croppedBitmap);
                    var resultBinary = TryRecognizeBitmap(binaryBitmap);
                    if (!string.IsNullOrEmpty(resultBinary))
                    {
                        return resultBinary;
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 检测一行是否为空白行（或接近空白）
        /// </summary>
        private bool IsBlankRow(Bitmap bitmap, int y)
        {
            if (y < 0 || y >= bitmap.Height) return true;

            int width = bitmap.Width;
            int nonWhiteCount = 0;
            int threshold = (int)(width * NonWhitePixelThreshold);

            // 使用快速方式获取像素数据
            for (int x = 0; x < width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                var brightness = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);

                // 如果像素不够白，计数
                if (brightness < WhiteThreshold)
                {
                    nonWhiteCount++;
                    // 如果非白像素超过阈值，则不是空白行
                    if (nonWhiteCount > threshold)
                    {
                        return false;
                    }
                }
            }

            return true;  // 是空白行
        }

        /// <summary>
        /// 裁剪顶部指定高度
        /// </summary>
        private Bitmap CropTop(Bitmap source, int cropHeight)
        {
            if (cropHeight <= 0) return new Bitmap(source);
            if (cropHeight >= source.Height) return new Bitmap(1, 1);

            int newHeight = source.Height - cropHeight;
            var result = new Bitmap(source.Width, newHeight);

            using var graphics = Graphics.FromImage(result);
            graphics.DrawImage(source, 
                new Rectangle(0, 0, source.Width, newHeight),
                new Rectangle(0, cropHeight, source.Width, newHeight),
                GraphicsUnit.Pixel);

            return result;
        }

        /// <summary>
        /// 缩放Bitmap
        /// </summary>
        private Bitmap ScaleBitmap(Bitmap source, int scale)
        {
            var width = source.Width * scale;
            var height = source.Height * scale;
            var result = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(result);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(source, 0, 0, width, height);
            return result;
        }

        /// <summary>
        /// 转换为灰度图
        /// </summary>
        private Bitmap ConvertToGrayscale(Bitmap source)
        {
            var result = new Bitmap(source.Width, source.Height);
            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    var pixel = source.GetPixel(x, y);
                    var gray = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                    result.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
            return result;
        }

        /// <summary>
        /// 增强对比度
        /// </summary>
        private Bitmap EnhanceContrast(Bitmap source)
        {
            var result = new Bitmap(source.Width, source.Height);
            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    var pixel = source.GetPixel(x, y);
                    var r = Math.Min(255, Math.Max(0, (pixel.R - 128) * 1.5 + 128));
                    var g = Math.Min(255, Math.Max(0, (pixel.G - 128) * 1.5 + 128));
                    var b = Math.Min(255, Math.Max(0, (pixel.B - 128) * 1.5 + 128));
                    result.SetPixel(x, y, Color.FromArgb((int)r, (int)g, (int)b));
                }
            }
            return result;
        }

        /// <summary>
        /// 二值化处理
        /// </summary>
        private Bitmap Binarize(Bitmap source, int threshold = 128)
        {
            var result = new Bitmap(source.Width, source.Height);
            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    var pixel = source.GetPixel(x, y);
                    var gray = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                    var binary = gray > threshold ? 255 : 0;
                    result.SetPixel(x, y, Color.FromArgb(binary, binary, binary));
                }
            }
            return result;
        }

        /// <summary>
        /// 获取支持的格式列表
        /// </summary>
        public List<string> GetSupportedFormats()
        {
            return _supportedFormats.Select(f => f.ToString()).ToList();
        }
    }
}
