﻿using Octgn.Library.Localization;
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
        public static Image GenerateProxy(BlockManager manager, TemplateDefinition template, Dictionary<string, string> values, string specialPath)
        {
            if (template == null || template.src == null)
                throw new UserMessageException(L.D.Exception__InvalidProxyDefinition);
            Bitmap temp = GraphicUtils.LoadImage(template.src, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(temp))
            {
                foreach (LinkDefinition overlay in template.GetOverLayBlocks(values))
                {
                    BlockDefinition block = null;
                    if (overlay.SpecialBlock != null)
                    {
                        block = overlay.SpecialBlock;
                        if (specialPath == null)
                        {
                            continue;
                        }
                        block.src = specialPath;
                        GraphicUtils.MergeArtOverlay(graphics, block);
                    }
                    else
                    {
                        block = manager.GetBlock(overlay.Block);
                        if (block.type != "overlay")
                        {
                            continue;
                        }
                        GraphicUtils.MergeOverlay(graphics, block);
                    }
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
