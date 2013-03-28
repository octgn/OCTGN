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
        public static Image GenerateProxy(string rootPath, TemplateDefinition template, Dictionary<string,string> values)
        {
            var path = Path.Combine(rootPath, template.src);
            Image temp = Image.FromFile(path);
            Bitmap b = ((Bitmap)temp).Clone(new Rectangle(0, 0, temp.Width, temp.Height), PixelFormat.Format32bppArgb);
            //b.MakeTransparent();
                //ret.PixelFormat = PixelFormat.Format32bppArgb;

            using (Graphics graphics = Graphics.FromImage(b))
            {
                foreach (LinkDefinition overlay in template.OverlayBlocks)
                {
                    BlockDefinition block = BlockManager.GetInstance().GetBlock(overlay.Block);
                    if (block.type != "overlay")
                    {
                        continue;
                    }
                    block.src = Path.Combine(rootPath, block.src);
                    GraphicUtils.MergeOverlay(graphics, block);
                }
                List<Property> removedProps = new List<Property>();
                foreach (LinkDefinition section in template.TextBlocks)
                {
                    BlockDefinition block = BlockManager.GetInstance().GetBlock(section.Block);
                    if (block.type != "text")
                    {
                        continue;
                    }
                    
                    
                    foreach (Property prop in section.NestedProperties)
                    {
                        if (!values.ContainsKey(prop.Name))
                        {
                            removedProps.Add(prop);
                        }
                    }
                    foreach (Property prop in removedProps)
                    {
                        section.NestedProperties.Remove(prop);
                    }

                    StringBuilder toWrite = new StringBuilder();
                    if (section.NestedProperties.Count > 1)
                    {
                        for (int i = 0; i < section.NestedProperties.Count; i++)
                        {
                            string propertyName = section.NestedProperties[i].Name;
                            if (i < (section.NestedProperties.Count - 1))
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
                        if (section.NestedProperties.Count > 0)
                        {
                            string propertyName = section.NestedProperties[0].Name;
                            toWrite.Append(values[propertyName]);
                        }
                    }
                    GraphicUtils.WriteString(graphics, block, toWrite.ToString());
                    foreach (Property prop in removedProps)
                    {
                        section.NestedProperties.Add(prop);
                    }
                    removedProps.Clear();
                }
            }

            return (b);
        }




    }
}
