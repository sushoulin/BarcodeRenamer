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

                // 尝试多种角度识别
                var result = TryRecognize(imagePath);
                if (!string.IsNullOrEmpty(result))
                {
                    Logger.Log($"成功识别条形码: {imagePath} -> {result}", LogLevel.Info);
                    return result;
                }

                // 尝试图像预处理后识别
                result = TryRecognizeWithPreprocessing(imagePath);
                if (!string.IsNullOrEmpty(result))
                {
                    Logger.Log($"预处理后成功识别: {imagePath} -> {result}", LogLevel.Info);
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
                    var result = _barcodeReader.Decode(scaledBitmap);
                    if (!string.IsNullOrEmpty(result?.Text))
                    {
                        return result.Text;
                    }
                }

                // 尝试灰度图
                using var grayBitmap = ConvertToGrayscale(bitmap);
                var grayResult = _barcodeReader.Decode(grayBitmap);
                if (!string.IsNullOrEmpty(grayResult?.Text))
                {
                    return grayResult.Text;
                }

                // 尝试增强对比度
                using var enhancedBitmap = EnhanceContrast(bitmap);
                var enhancedResult = _barcodeReader.Decode(enhancedBitmap);
                if (!string.IsNullOrEmpty(enhancedResult?.Text))
                {
                    return enhancedResult.Text;
                }
            }
            catch
            {
                // 忽略预处理失败
            }

            return string.Empty;
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
        /// 获取支持的格式列表
        /// </summary>
        public List<string> GetSupportedFormats()
        {
            return _supportedFormats.Select(f => f.ToString()).ToList();
        }
    }
}
