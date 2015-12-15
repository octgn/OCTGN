using Nancy.ViewEngines.Razor;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Octgn.Client
{
    public abstract class WebViewBase<T> : NancyRazorViewBase<T>
    {
        public IHtmlString RT(string name)
        {
            var val = this.Text["Text." + name];
            return new NonEncodedHtmlString(val as string);
        }
    }
}
