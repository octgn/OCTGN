/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2012 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */ 
#if !CF
using System;
using System.IO;
#if !SL
using System.Drawing;
using System.Drawing.Imaging;
#endif
using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.iq.vcard
{
	/// <summary>
	/// Vcard Photo
	/// When you dont want System.Drawing in the Lib just remove the photo stuff
	/// </summary>
	public class Photo : Element
	{
		// <!-- Photograph property. Value is either a BASE64 encoded
		// binary value or a URI to the external content. -->
		// <!ELEMENT PHOTO ((TYPE, BINVAL) | EXTVAL)>	
		#region << Constructors >>
		public Photo()
		{
			this.TagName	= "PHOTO";
			this.Namespace	= Uri.VCARD;
		}
#if !SL
		public Photo(Image image, ImageFormat format) : this()
		{
			SetImage(image, format);
		}
#endif	
		public Photo(string url) : this()
		{
			SetImage(url);
		}
		#endregion
				
		/// <summary>
		/// The Media Type, Only available when BINVAL
		/// </summary>
		public string Type
		{
			//<TYPE>image</TYPE>
			get { return GetTag("TYPE"); }
			set { SetTag("TYPE", value); }
		}

		/// <summary>
		/// Sets the URL of an external image
		/// </summary>
		/// <param name="url"></param>
		public void SetImage(string url)
		{
			SetTag("EXTVAL", url);
		}

        /*
		/// <summary>
		/// Sets a internal Image
		/// </summary>
		/// <param name="image"></param>
		public void SetImage(Image image)
		{
			Image = image;
		}
        */

#if !SL
        public void SetImage(Image image, ImageFormat format)
        {
            // if we have no FOrmatprovider then we save the image as PNG
            if (format == null)
                format = ImageFormat.Png;

            // 17.05.2006 
            // fixed GDI+ bug see also http://www.bobpowell.net/imagefileconvert.htm
            string sType = "image";
            
            if (format == ImageFormat.Jpeg)
                sType = "image/jpeg";
            else if (format == ImageFormat.Png)
                sType = "image/png";
            else if (format == ImageFormat.Gif)
                sType = "image/gif";
#if!CF_2
            else if (format == ImageFormat.Tiff)
                sType = "image/tiff";
#endif

            SetTag("TYPE", sType);            
            //create temporary
            Image temp = new Bitmap(image.Width, image.Height);            
            //get graphics
            Graphics g = Graphics.FromImage(temp);
            
            //copy image
            // i hope this overload of DrawImage works now on all Frameworks, also CF2
            g.DrawImage(image, new Rectangle(0, 0, temp.Width, temp.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            //g.DrawImage(image, 0, 0, image.Width, image.Height);
            g.Dispose();

            MemoryStream ms = new MemoryStream();
            temp.Save(ms, format);
            byte[] buf = ms.GetBuffer();
            SetTagBase64("BINVAL", buf);
        }

        /// <summary>
        /// returns the image format or null for unknown formats or TYPES
        /// </summary>
        public ImageFormat ImageFormat
        {
            get
            {
                string sType = GetTag("TYPE");

                if (sType == "image/jpeg")
                    return ImageFormat.Jpeg;
                else if (sType == "image/png")
                    return ImageFormat.Png;
                else if (sType == "image/gif")
                    return ImageFormat.Gif;
#if!CF_2
                else if (sType == "image/tiff")
                    return ImageFormat.Tiff;
#endif
                else
                    return null;
            }
        }

		/// <summary>
		/// gets or sets the from internal (binary) or external source
		/// When external then it trys to get the image with a Webrequest
		/// </summary>
		public System.Drawing.Image Image
		{
			get
			{
				try
				{
					if (HasTag("BINVAL"))
					{
						byte[] pic = Convert.FromBase64String(GetTag("BINVAL"));
						System.IO.MemoryStream ms = new System.IO.MemoryStream(pic, 0, pic.Length);
						return new System.Drawing.Bitmap(ms);
					}
					else if (HasTag("EXTVAL"))
					{
						System.Net.WebRequest req = System.Net.WebRequest.Create(GetTag("EXTVAL"));
						System.Net.WebResponse response = req.GetResponse();
						return new System.Drawing.Bitmap(response.GetResponseStream());
					}
					else
						return null;
				}
				catch
				{
					return null;
				}
			}
			/*
            set
			{
				SetTag("TYPE", "image");
				MemoryStream ms = new MemoryStream();
				// Save the Image as PNG to the Memorystream
				value.Save(ms, ImageFormat.Png);
				byte[] buf = ms.GetBuffer();				
				SetTagBase64("BINVAL", buf);
			}
            */
		}
#endif
    }
}
#endif