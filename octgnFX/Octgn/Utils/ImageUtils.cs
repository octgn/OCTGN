using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;

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

        public static void GetCardImage(ICard card, Action<BitmapImage> action, bool proxyOnly = false)
        {
			//var uri = new Uri(card.GetPicture());
            var uri = proxyOnly ? 
                new Uri(card.GetProxyPicture()) : new Uri(card.GetPicture());
            BitmapImage bmp = GetImageFromCache(uri);
            if (bmp != null)
            {
                action(bmp);
                return;
            }
            action(CreateFrozenBitmap(uri));
            return;
            // If the bitmap is not in cache, display the default face up picture and load the correct one async.
            //action(Program.GameEngine.CardFrontBitmap);
            //Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>{action(CreateFrozenBitmap(uri));}),DispatcherPriority.Input);
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
                imgsrc.UriSource = Program.GameEngine.GetCardFront("Default").UriSource;
                imgsrc.EndInit();              
            }
			if(imgsrc.CanFreeze)
				imgsrc.Freeze();
            return imgsrc;
        }
    }
}