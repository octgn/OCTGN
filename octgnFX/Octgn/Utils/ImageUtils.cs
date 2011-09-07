using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows.Threading;

namespace Octgn
{
  static class ImageUtils
  {
    static ImageUtils()
    {
      reflectionBitmapImage = new BitmapImage();
      var methodInfo = typeof(BitmapImage).GetMethod("CheckCache", BindingFlags.NonPublic | BindingFlags.Instance);
      GetImageFromCache = (Func<Uri, BitmapImage>)Delegate.CreateDelegate(typeof(Func<Uri, BitmapImage>), reflectionBitmapImage, methodInfo);
    }

    private static BitmapImage reflectionBitmapImage;
    private static Func<Uri, BitmapImage> GetImageFromCache;

    public static void GetCardImage(Uri uri, Action<BitmapImage> action)
    {
      BitmapImage bmp = GetImageFromCache(uri);
      if (bmp != null)
      { action(bmp); return; }

      // If the bitmap is not in cache, display the default face up picture and load the correct one async.
      action(Program.Game.CardFrontBitmap);
      Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
      {
        var imgsrc = new BitmapImage();
        imgsrc.BeginInit();
        imgsrc.CacheOption = BitmapCacheOption.OnLoad;
        imgsrc.UriSource = uri;
        imgsrc.EndInit();
        imgsrc.Freeze();
        action(imgsrc);
      }), DispatcherPriority.ContextIdle);
    }

    public static BitmapImage CreateFrozenBitmap(string source)
    { return CreateFrozenBitmap(new Uri(source)); }

    public static BitmapImage CreateFrozenBitmap(Uri uri)
    {
      var imgsrc = new BitmapImage();
      imgsrc.BeginInit();
      imgsrc.CacheOption = BitmapCacheOption.OnLoad;
      imgsrc.UriSource = uri;
      imgsrc.EndInit();
      imgsrc.Freeze();
      return imgsrc;
    }
  }
}
