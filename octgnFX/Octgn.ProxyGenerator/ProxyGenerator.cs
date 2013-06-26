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

    using Octgn.Library.Exceptions;

    public class ProxyGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Image GenerateProxy(BlockManager manager, string rootPath, TemplateDefinition template, Dictionary<string,string> values)
        {
            if (rootPath == null || template == null || template.src == null)
                throw new UserMessageException(
                    "There is an invalid proxy definition. Please contact the game developer and let them know.");
            var path = Path.Combine(rootPath, template.src);
            Bitmap temp = GraphicUtils.LoadImage(path,PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(temp))
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
                
                foreach (LinkDefinition section in template.GetTextBlocks(values))
                {
                    List<Property> clonedProps = new List<Property>();
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
                            string format = clonedProps[i].Format.Replace("{}", values[propertyName]);
                            if (i < (clonedProps.Count - 1))
                            {
                                toWrite.Append(string.Format("{0} {1}", format, section.Separator));
                            }
                            else
                            {
                                toWrite.Append(format);
                            }
                        }

                    }
                    else
                    {
                        if (clonedProps.Count > 0)
                        {
                            string propertyName = clonedProps[0].Name;
                            string format = clonedProps[0].Format.Replace("{}", values[propertyName]);
                            toWrite.Append(format);
                        }
                    }
                    GraphicUtils.WriteString(graphics, block, toWrite.ToString());
                }
            }

            return (temp);
        }




    }
}
