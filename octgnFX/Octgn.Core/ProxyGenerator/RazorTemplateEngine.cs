//// /* This Source Code Form is subject to the terms of the Mozilla Public
////  * License, v. 2.0. If a copy of the MPL was not distributed with this
////  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

//using System;
//using System.IO;
//using Awesomium.Core;
//using Octgn.DataNew.Entities;
//using RazorEngine;
//using RazorEngine.Configuration;
//using RazorEngine.Templating;

//namespace Octgn.Core.ProxyGenerator
//{
//    public class RazorProxyGenerator
//    {
//        private const string TestTemplateString = @"
//<html>
//    <head>
//        <script type='text/javascript'>
//            window.onload = function() {
//                setTimeout(function()
//                { 
//                    var elem = document.getElementById('taco');
//                    elem.style.color = 'red'; 
//                }
//                , 2000);
//            }
//        </script>
//    </head>
//    <body  style='background-color: black'>
//        <h1 id='taco' style='color:White'>@(Model.Name)</h1>
//    </body>
//</html>
//";

//        public RazorProxyGenerator()
//        {
//            var config = new FluentTemplateServiceConfiguration(c => c.WithEncoding(RazorEngine.Encoding.Raw));
//            // create a new TemplateService and pass in the configuration to the constructor
//            var myConfiguredTemplateService = new TemplateService(config);
//            // set the template service to our configured one
//            Razor.SetTemplateService(myConfiguredTemplateService);
//            Razor.Compile(TestTemplateString, typeof(ICard), "ICard");
//        }

//        public string GenerateHtml(ICard card)
//        {
//            // Making sure to compile the templates reduces time from like 500ms to 40ms
//            // Thinking during o8build we should compile out all of these templates into pure html

//            // start parsing templates
//            //string template = "Hello \"@(Model.Name)\"";

//            string result= Razor.Run("ICard", card);
//            //string result = Razor.Parse(template, card);
//            return result;
//        }

//        public ISurface GenerateImage(ICard card)
//        {
//            var view = WebCore.CreateWebView(640, 480, WebViewType.Offscreen);
//            var bs = new BitmapSurface(640, 480);
//            view.Surface = bs;
//            view.LoadingFrameComplete += (s, e) =>
//            {
//                if (e.IsMainFrame)
//                {
//                    WebCore.QueueWork(() => { 
//                        Console.WriteLine("Saving...");
//                        bs.SaveToPNG(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "image.png"));
//                        WebCore.Shutdown();
//                    });
//                }
//            };

//            var html = GenerateHtml(card);

//            if(view.LoadHTML(html))
//            {
//                WebCore.Run();
//            }
//            return view.Surface;
//        }
//    }
//}