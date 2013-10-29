namespace Octgn.Core.Appenders
{
    using System;

    using global::log4net.Appender;

    public class RollingFileAppenderCustom : RollingFileAppender
    {
        public override string File
        {
            get
            {
                return base.File;
            }
            set
            {
                if (value == null)
                {
                    base.File = null;
                }
                else
                {
                    var nv = value.Replace("$TIMESTAMP$", DateTime.Now.ToFileTime().ToString());
                    base.File = nv;
                }
            }
        }
    }
}