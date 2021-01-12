using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkImageResizer
{
    public class SKImageProcess
    {
        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImages(string sourcePath, string destPath, double scale)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            var allFiles = FindImages(sourcePath);
            foreach (var filePath in allFiles)
            {
                var bitmap = SKBitmap.Decode(filePath);
                var imgPhoto = SKImage.FromBitmap(bitmap);
                var imgName = Path.GetFileNameWithoutExtension(filePath);

                var sourceWidth = imgPhoto.Width;
                var sourceHeight = imgPhoto.Height;

                var destinationWidth = (int)(sourceWidth * scale);
                var destinationHeight = (int)(sourceHeight * scale);

                using var scaledBitmap = bitmap.Resize(
                    new SKImageInfo(destinationWidth, destinationHeight),
                    SKFilterQuality.High);
                using var scaledImage = SKImage.FromBitmap(scaledBitmap);
                using var data = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);
                using var s = File.OpenWrite(Path.Combine(destPath, imgName + ".jpg"));
                data.SaveTo(s);
            }
        }

        public async Task ResizeImagesAsync(string sourcePath, string destPath, double scale)
        {
            if (!Directory.Exists(destPath)) { Directory.CreateDirectory(destPath); }

            var ls = new List<Task>();
            var allFiles = FindImages(sourcePath);
            foreach (var filePath in allFiles)
            {
                ls.Add(Task.Run(() =>
                {
                    var bitmap = SKBitmap.Decode(filePath);
                    var imgPhoto = SKImage.FromBitmap(bitmap);
                    var imgName = Path.GetFileNameWithoutExtension(filePath);

                    var sourceWidth = imgPhoto.Width;
                    var sourceHeight = imgPhoto.Height;

                    var destinationWidth = (int)(sourceWidth * scale);
                    var destinationHeight = (int)(sourceHeight * scale);

                    Task.Run(() =>
                    {
                        using var scaledBitmap = bitmap.Resize(
                            new SKImageInfo(destinationWidth, destinationHeight),
                            SKFilterQuality.High);
                        using var scaledImage = SKImage.FromBitmap(scaledBitmap);
                        using var data = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);
                        using var s = File.OpenWrite(Path.Combine(destPath, imgName + ".jpg"));
                        data.SaveTo(s);
                    });
                }));
            }

            await Task.WhenAll(ls);
        }

        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            var exts = new string[] { ".png", ".jpg", ".jpeg" };
            //List<string> files = new List<string>();
            //var times = MeasureTimeSpend(() =>
            //{
            //    files.AddRange(Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories));
            //    files.AddRange(Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories));
            //    files.AddRange(Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories));
            //});
            //Console.WriteLine($"1. {times:#,0.00} ms");

            //files.Clear();
            //times = MeasureTimeSpend(() =>
            //{
            //    files = Directory.EnumerateFiles(srcPath, "*.*", SearchOption.AllDirectories)
            //                     .Where(file => exts.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
            //                     .ToList();
            //});
            //Console.WriteLine($"2. {times:#,0.00} ms");

            //return files;

            return Directory.EnumerateFiles(srcPath, "*.*", SearchOption.AllDirectories)
                            .Where(file => exts.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                            .ToList();
        }

        private double MeasureTimeSpend(Action func)
        {
            if (func == null) { return -1d; }
            var sw = new Stopwatch();
            sw.Start();
            func.Invoke();
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }
    }
}