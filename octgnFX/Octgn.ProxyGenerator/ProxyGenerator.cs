using Octgn.ProxyGenerator.Definitions;
using Octgn.ProxyGenerator.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator
{
    using System.IO;

    public class ProxyGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Image GenerateProxy(string rootPath, CardDefinition template, Dictionary<string,string> values)
        {
            var path = Path.Combine(rootPath, template.filename);
            Image ret = Image.FromFile(path);

            using (Graphics graphics = Graphics.FromImage(ret))
            {
                foreach (OverlayDefinition overlay in template.Overlays)
                {
                    overlay.filename = Path.Combine(rootPath, overlay.filename);
                   GraphicUtils.MergeOverlay(graphics, overlay);
                }

                foreach (SectionDefinition section in template.Sections)
                {
                    if (values.ContainsKey(section.id))
                    {
                        GraphicUtils.WriteString(graphics, section, values[section.id]);
                    }
                }
            }

            return (ret);
        }




    }
}
