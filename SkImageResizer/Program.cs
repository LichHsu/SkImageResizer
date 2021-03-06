﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkImageResizer
{
    class Program
    {
        static readonly Stopwatch sw = new Stopwatch();
        static CancellationTokenSource cts = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            Progress<string> progress = new Progress<string>((msg) =>
            {
                Console.WriteLine(msg);
            });

            Console.CancelKeyPress += Console_CancelKeyPress;

            var imageProcess = new SKImageProcess();
            var sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            var destinationPath1 = Path.Combine(Environment.CurrentDirectory, "output1");
            var destinationPath2 = Path.Combine(Environment.CurrentDirectory, "output2");

            // Sync
            Console.WriteLine($"同步壓縮 => 清空舊檔案.");
            imageProcess.Clean(destinationPath1);

            Console.WriteLine($"同步壓縮 => 開始.");
            sw.Start();
            imageProcess.ResizeImages(sourcePath, destinationPath1, 2.0);
            sw.Stop();

            //decimal result1 = 12_000;//sw.ElapsedMilliseconds;
            var result1 = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"同步壓縮 => 結束 (耗時 {result1 / 1000:#,0.00} 秒).");

            Console.WriteLine("");
            Console.WriteLine("========");
            Console.WriteLine("");

            // Async
            Console.WriteLine($"非同步壓縮 => 清空舊檔案.");
            imageProcess.Clean(destinationPath2);

            Console.WriteLine($"非同步壓縮 => 開始.");
            sw.Restart();
            try
            {
                await imageProcess.ResizeImagesAsync(sourcePath, destinationPath2, 2.0, progress, cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"Canceled: {ex}");
                Console.WriteLine($"作業取消 => 清空舊檔案.");
                imageProcess.Clean(destinationPath2);
                Console.WriteLine($"清空舊檔案 => 完成.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception:{ex}");
            }
            sw.Stop();

            var result2 = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"非同步壓縮 => 結束 (耗時 {result2 / 1000:#,0.00} 秒).");

            // Result
            // 效能提升比例公式：((Orig - New) / Orig) * 100%
            //var result = ((result1 - result2) / result1) * 100;
            //Console.WriteLine($"效能提升 => {result:f2}%");
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            cts.Cancel();

            e.Cancel = true;
        }
    }
}
