﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageProcess
    {
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
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public async Task ResizeImagesAsync(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);

            //非同步處理圖片資訊
            var task = new Task[allFiles.Count];
            for (int i = 0; i < allFiles.Count; i++)
            {
                var file = allFiles[i];
                task[i] = Task.Run(async () =>
                {
                    await ExecuteResize(destPath, scale, file);
                });
            }

            await Task.WhenAll(task);
        }

        private async Task<bool> ExecuteResize(string destPath, double scale, string filePath)
        {
            //var tid = String.Format("{0:D2}", Thread.CurrentThread.ManagedThreadId);
            //Console.WriteLine($" 測試 (TID: {tid}) >>>> {DateTime.Now}");

            Image imgPhoto = Image.FromFile(filePath);
            string imgName = Path.GetFileNameWithoutExtension(filePath);

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;

            int destionatonWidth = (int)(sourceWidth * scale);
            int destionatonHeight = (int)(sourceHeight * scale);

            var processedImage = await processBitmap((Bitmap)imgPhoto,
                sourceWidth, sourceHeight,
                destionatonWidth, destionatonHeight);

            //Console.WriteLine($" 測試後 (TID: {tid}) >>>> {DateTime.Now}");

            string destFile = Path.Combine(destPath, imgName + ".jpg");
            processedImage.Save(destFile, ImageFormat.Jpeg);
            return true;
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories));
            return files;
        }


        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        async Task<Bitmap> processBitmap(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            var task = await Task.Run(() =>
            {
                Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
                Graphics g = Graphics.FromImage(resizedbitmap);
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.Clear(Color.Transparent);
                g.DrawImage(img,
                    new Rectangle(0, 0, newWidth, newHeight),
                    new Rectangle(0, 0, srcWidth, srcHeight),
                    GraphicsUnit.Pixel);

                return resizedbitmap;
            });
            return task;
        }
    }
}
