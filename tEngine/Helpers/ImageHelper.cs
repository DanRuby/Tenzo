using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Size = System.Windows.Size;

namespace tEngine.Helpers
{
    /// <summary>
    /// Расширения класса BitmapImage
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// Сохранить картинку в файл
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filepath"></param>
        public static void Save(this BitmapSource image, string filepath)
        {
            if (image == null) return;
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            try
            {
                using (FileStream filestream = new FileStream(filepath, FileMode.Create))
                    encoder.Save(filestream);
            }
            catch (IOException)
            {
                MessageBox.Show("Ошибка создания файла", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Может понадобиться позже

        //public static BitmapImage Uri2BI(Uri uri, Size size)
        //{
        //    BitmapImage imSize = new BitmapImage(uri);
        //    BitmapImage image = new BitmapImage();
        //    image.BeginInit();
        //    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
        //    image.CacheOption = BitmapCacheOption.OnLoad;
        //    image.UriSource = uri;

        //    int wOld = imSize.PixelWidth;
        //    int hOld = imSize.PixelHeight;

        //    int wNew = (int)size.Width;
        //    int hNew = (int)size.Height;

        //    double k = wOld / (double)hOld;
        //    if (k <= wNew / (double)hNew)
        //    {
        //        wNew = (int)(k * hNew);
        //    }
        //    else
        //    {
        //        hNew = (int)(wNew / k);
        //    }

        //    image.DecodePixelWidth = wNew;
        //    image.DecodePixelHeight = hNew;
        //    image.EndInit();
        //    image.Freeze();
        //    return image;
        //}

        //public static BitmapImage Uri2BI(Uri uri)
        //{
        //    return Uri2BI(uri, new Size(0, 0));
        //}

        //public static BitmapImage Array2BI(byte[] array, Size size)
        //{
        //    BitmapImage image = new BitmapImage();
        //    using (MemoryStream stream = new MemoryStream(array))
        //    {
        //        image.BeginInit();
        //        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
        //        image.CacheOption = BitmapCacheOption.OnLoad;
        //        image.UriSource = null;
        //        image.StreamSource = stream;
        //        image.DecodePixelWidth = (int)size.Width;
        //        image.DecodePixelHeight = (int)size.Height;
        //        image.EndInit();
        //    }
        //    return image;
        //}

        //public static BitmapImage Array2BI(byte[] array)
        //{
        //    return Array2BI(array, new Size(0, 0));
        //}

        //public static Int32 Array2Int(byte[] array)
        //{
        //    if (BitConverter.IsLittleEndian)
        //        Array.Reverse(array, 0, sizeof(Int32));
        //    return BitConverter.ToInt32(array, 0);
        //}

        //public static byte[] BI2Array(BitmapImage image)
        //{
        //    PngBitmapEncoder encoder = new PngBitmapEncoder();
        //    encoder.Frames.Add(BitmapFrame.Create(image));
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        encoder.Save(stream);
        //        return stream.ToArray();
        //    }
        //}

        //public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        //{
        //    if (bitmap == null) return null;
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        bitmap.Save(ms, ImageFormat.Png);
        //        ms.Position = 0;
        //        BitmapImage bi = new BitmapImage();
        //        bi.BeginInit();
        //        bi.StreamSource = ms;
        //        bi.EndInit();

        //        return bi;
        //    }
        //}

        //public static BitmapImage GetSimilarImage(BitmapImage image, Size size)
        //{
        //    // не работает =( можно сделать через BI2Array
        //    throw new NotImplementedException();
        //    /*if( image == null ) 
        //        return null;
        //    var orignBi = image;
        //    var bi = new BitmapImage();

        //    // клонировать нельзя, т.к. BeginInit будет второй раз
        //    bi.BeginInit();
        //    bi.CreateOptions = image.CreateOptions;
        //    bi.CacheOption = image.CacheOption;
        //    bi.UriSource = image.UriSource;
        //    // вероятно здесь
        //    bi.StreamSource = image.StreamSource;

        //    var w = size.Width/orignBi.PixelWidth;
        //    var h = size.Height/orignBi.PixelHeight;
        //    var k = w > h ? h : w;
        //    bi.DecodePixelHeight = (int) (k*orignBi.PixelHeight);
        //    bi.DecodePixelWidth = (int) (k*orignBi.PixelWidth);
        //    bi.EndInit();

        //    return bi;*/
        //}

        //public static byte[] Int2Array(Int32 value)
        //{
        //    byte[] intBytes = BitConverter.GetBytes(value);
        //    if (BitConverter.IsLittleEndian)
        //        Array.Reverse(intBytes);
        //    return intBytes;
        //}
        #endregion
    }
}