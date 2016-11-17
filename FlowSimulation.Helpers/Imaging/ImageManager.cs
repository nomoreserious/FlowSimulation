using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace FlowSimulation.Helpers.Imaging
{
    public class ImageManager
    {
        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bi = new BitmapImage();
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        /// <summary>
        /// Загрузка изображения в BitmapSource из байтового массива
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public static BitmapSource LoadImage(Byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                var decoder = BitmapDecoder.Create(ms,
                    BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return decoder.Frames.FirstOrDefault();
            }
        }

        public static BitmapSource LoadImage(Byte[] imageData, int targetWidth)
        {
            using (MemoryStream ms = new MemoryStream(ResizeBinary(imageData, targetWidth)))
            {
                var decoder = BitmapDecoder.Create(ms,
                    BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return decoder.Frames.FirstOrDefault();
            }
        }

        private static byte[] ResizeBinary(byte[] imageFile, int targetSize)
        {
            using (MemoryStream tempStream = new MemoryStream(imageFile))
            {
                using (Image oldImage = Image.FromStream(tempStream))
                {
                    Size newSize = new Size(targetSize, oldImage.Size.Height * targetSize / (oldImage.Size.Width));
                    using (Bitmap newImage = new Bitmap(oldImage, newSize))
                    {
                        using (MemoryStream m = new MemoryStream())
                        {
                            newImage.Save(m, ImageFormat.Jpeg);
                            return m.GetBuffer();
                        }
                    }
                }
            }
        }
    }
}
