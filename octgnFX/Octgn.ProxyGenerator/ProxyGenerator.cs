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
    using System.Drawing.Imaging;
    using System.IO;

    public class ProxyGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Image GenerateProxy(BlockManager manager, string rootPath, TemplateDefinition template, Dictionary<string,string> values)
        {
            var path = Path.Combine(rootPath, template.src);
            Image temp = Image.FromFile(path);
            Bitmap b = ((Bitmap)temp).Clone(new Rectangle(0, 0, temp.Width, temp.Height), PixelFormat.Format32bppArgb);
            //b.MakeTransparent();
                //ret.PixelFormat = PixelFormat.Format32bppArgb;

            using (Graphics graphics = Graphics.FromImage(b))
            {
                foreach (LinkDefinition overlay in template.GetOverLayBlocks(values))
                {
                    BlockDefinition block = manager.GetBlock(overlay.Block);
                    if (block.type != "overlay")
                    {
                        continue;
                    }
                    block.src = Path.Combine(rootPath, block.src);
                    GraphicUtils.MergeOverlay(graphics, block);
                }
                List<Property> clonedProps = new List<Property>();
                foreach (LinkDefinition section in template.GetTextBlocks(values))
                {
                    BlockDefinition block = manager.GetBlock(section.Block);
                    if (block.type != "text")
                    {
                        continue;
                    }
                    clonedProps.AddRange(section.NestedProperties);
                    
                    foreach (Property prop in section.NestedProperties)
                    {
                        if (!values.ContainsKey(prop.Name))
                        {
                            clonedProps.Remove(prop);
                        }
                        if (clonedProps.Contains(prop) && (values[prop.Name] == null || values[prop.Name].Length == 0))
                        {
                            clonedProps.Remove(prop);
                        }
                    }

                    StringBuilder toWrite = new StringBuilder();
                    if (clonedProps.Count > 1)
                    {
                        for (int i = 0; i < clonedProps.Count; i++)
                        {
                            string propertyName = clonedProps[i].Name;
                            if (i < (clonedProps.Count - 1))
                            {
                                toWrite.Append(string.Format("{0} {1}", values[propertyName], section.Separator));
                            }
                            else
                            {
                                toWrite.Append(values[propertyName]);
                            }
                        }

                    }
                    else
                    {
                        if (clonedProps.Count > 0)
                        {
                            string propertyName = clonedProps[0].Name;
                            toWrite.Append(values[propertyName]);
                        }
                    }
                    GraphicUtils.WriteString(graphics, block, toWrite.ToString());
                }
            }

            return (b);
        }




    }
}
