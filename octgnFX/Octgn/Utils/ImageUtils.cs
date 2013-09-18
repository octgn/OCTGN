using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Octgn.Utils
{
    internal static class ImageUtils
    {
        private static readonly BitmapImage ReflectionBitmapImage;
        private static readonly Func<Uri, BitmapImage> GetImageFromCache;

        static ImageUtils()
        {
            ReflectionBitmapImage = new BitmapImage();
            MethodInfo methodInfo = typeof (BitmapImage).GetMethod("CheckCache",
                                                                   BindingFlags.NonPublic | BindingFlags.Instance);
            GetImageFromCache =
                (Func<Uri, BitmapImage>)
                Delegate.CreateDelegate(typeof (Func<Uri, BitmapImage>), ReflectionBitmapImage, methodInfo);
        }

        public static void GetCardImage(Uri uri, Action<BitmapImage> action)
        {
            BitmapImage bmp = GetImageFromCache(uri);
            if (bmp != null)
            {
                action(bmp);
                return;
            }
            // If the bitmap is not in cache, display the default face up picture and load the correct one async.
            action(Program.GameEngine.CardFrontBitmap);
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>{action(CreateFrozenBitmap(uri));}), DispatcherPriority.ContextIdle);
        }

        public static BitmapImage CreateFrozenBitmap(string source)
        {
            return CreateFrozenBitmap(new Uri(source));
        }

        public static BitmapImage CreateFrozenBitmap(Uri uri)
        {
            var imgsrc = new BitmapImage();
            imgsrc.BeginInit();
            imgsrc.CacheOption = BitmapCacheOption.OnLoad;
            // catch bad Uri's and load Front Bitmap "?"
            try
            {                                               
                imgsrc.UriSource = uri;
                imgsrc.EndInit();                
            }
            catch (Exception)
            {
                imgsrc = new BitmapImage();
                imgsrc.BeginInit();
                imgsrc.CacheOption = BitmapCacheOption.None;
                imgsrc.UriSource = Program.GameEngine.CardFrontBitmap.UriSource;
                imgsrc.EndInit();              
            }
            imgsrc.Freeze();
            return imgsrc;
        }
    }
}