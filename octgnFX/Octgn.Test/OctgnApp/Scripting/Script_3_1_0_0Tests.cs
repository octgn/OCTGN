using System;
using System.IO;
using NuGet;
using NUnit.Framework;
using Octgn.DataNew.Entities;
using Octgn.Scripting.Versions;
using Octgn.Test.Utils;

namespace Octgn.Test.OctgnApp.Scripting
{
	public class Script_3_1_0_0Tests
	{
		[Test]
		public void DoWebRequest()
		{
			var s = new Script_3_1_0_0();

			var result = s.DoWebRequest("asdf", 100);
			Assert.AreEqual(Tuple.Create("URL is in an invalid format", 0), result);

			var def = new Game() {
				Name = "Test",
				Version = new Version(1, 0, 0, 0),
				ScriptVersion = new Version(1, 0, 0, 0)
			};
#pragma warning disable CS0618 // Type or member is obsolete
            Program.GameEngine = new GameEngine();
#pragma warning restore CS0618 // Type or member is obsolete
            Program.GameEngine.Definition = def;


			result = s.DoWebRequest("http://httpstat.us/200", 0);
			Assert.AreEqual(Tuple.Create("200 OK", 200), result);

			using (var wl = new HttpEcho()) {
				// Sending proper header
				wl.Response = context => {
					using (var sw = new StreamWriter(context.Response.OutputStream)) {
						sw.Write("UserAgent:" + context.Request.Headers["UserAgent"]);
					}
					context.Response.Close();
				};
				result = s.DoWebRequest(wl.Url, 1000);
				var ua = "UserAgent:OCTGN_" + Const.OctgnVersion.ToString() + "/" + Program.GameEngine.Definition.Name + "_" + Program.GameEngine.Definition.Version.ToString();
				Assert.AreEqual(ua, result.Item1);

				// No content
				wl.Response = context => {
					context.Response.StatusCode = 200;
					context.Response.Close();
				};
				result = s.DoWebRequest(wl.Url, 1000);
				Assert.AreEqual(Tuple.Create("No Content Error", 204), result);

				// Proper Status Code
				wl.Response = context => {
					context.Response.StatusCode = 501;
					using (var sw = new StreamWriter(context.Response.OutputStream)) {
						sw.Write("asdf");
					}
					context.Response.Close();
				};
				result = s.DoWebRequest(wl.Url, 1000);
				Assert.AreEqual(Tuple.Create("Error", 501), result);

				wl.Response = context => {
					context.Response.StatusCode = 200;
					using (var sw = new StreamWriter(context.Response.OutputStream)) {
						sw.Write(context.Request.InputStream.ReadToEnd());
					}
					context.Response.Close();
				};
				result = s.DoWebRequest(wl.Url, 1000, "tacos");
				Assert.AreEqual(Tuple.Create("tacos", 200), result);
			}
		}

		public class Script_3_1_0_1Tests
		{
			[Test]
			public void DoWebRequest()
			{
				var s = new Script_3_1_0_1();

				var result = s.DoWebRequest("asdf", 100);
				Assert.AreEqual(Tuple.Create("URL is in an invalid format", 0), result);

				var def = new Game() {
					Name = "Test",
					Version = new Version(1, 0, 0, 0),
					ScriptVersion = new Version(1, 0, 0, 0)
				};
#pragma warning disable CS0618 // Type or member is obsolete
                Program.GameEngine = new GameEngine();
#pragma warning restore CS0618 // Type or member is obsolete
                Program.GameEngine.Definition = def;


				result = s.DoWebRequest("http://httpstat.us/200", 0);
				Assert.AreEqual(Tuple.Create("200 OK", 200), result);

				using (var wl = new HttpEcho()) {
					// Sending proper header
					wl.Response = context => {
						using (var sw = new StreamWriter(context.Response.OutputStream)) {
							sw.Write("UserAgent:" + context.Request.Headers["UserAgent"]);
						}
						context.Response.Close();
					};
					result = s.DoWebRequest(wl.Url, 1000);
					var ua = "UserAgent:OCTGN_" + Const.OctgnVersion.ToString() + "/" + Program.GameEngine.Definition.Name + "_" + Program.GameEngine.Definition.Version.ToString();
					Assert.AreEqual(ua, result.Item1);

					// No content
					wl.Response = context => {
						context.Response.StatusCode = 200;
						context.Response.Close();
					};
					result = s.DoWebRequest(wl.Url, 1000);
					Assert.AreEqual(Tuple.Create("No Content Error", 204), result);

					// Proper Status Code
					wl.Response = context => {
						context.Response.StatusCode = 501;
						using (var sw = new StreamWriter(context.Response.OutputStream)) {
							sw.Write("asdf");
						}
						context.Response.Close();
					};
					result = s.DoWebRequest(wl.Url, 1000);
					Assert.AreEqual(Tuple.Create("Error", 501), result);

					wl.Response = context => {
						context.Response.StatusCode = 200;
						using (var sw = new StreamWriter(context.Response.OutputStream)) {
							sw.Write(context.Request.InputStream.ReadToEnd());
						}
						context.Response.Close();
					};
					result = s.DoWebRequest(wl.Url, 1000, "tacos");
					Assert.AreEqual(Tuple.Create("tacos", 200), result);
				}
			}
		}

		public class Script_3_1_0_2Tests
		{
			[Test]
			public void DoWebRequest()
			{
				var s = new Script_3_1_0_2();

				var result = s.DoWebRequest("asdf", 100);
				Assert.AreEqual(Tuple.Create("URL is in an invalid format", 0), result);

				var def = new Game() {
					Name = "Test",
					Version = new Version(1, 0, 0, 0),
					ScriptVersion = new Version(1, 0, 0, 0)
				};
#pragma warning disable CS0618 // Type or member is obsolete
                Program.GameEngine = new GameEngine();
#pragma warning restore CS0618 // Type or member is obsolete
                Program.GameEngine.Definition = def;


				result = s.DoWebRequest("http://httpstat.us/200", 0);
				Assert.AreEqual(Tuple.Create("200 OK", 200), result);

				using (var wl = new HttpEcho()) {
					// Sending proper header
					wl.Response = context => {
						using (var sw = new StreamWriter(context.Response.OutputStream)) {
							sw.Write("UserAgent:" + context.Request.Headers["UserAgent"]);
						}
						context.Response.Close();
					};
					result = s.DoWebRequest(wl.Url, 1000);
					var ua = "UserAgent:OCTGN_" + Const.OctgnVersion.ToString() + "/" + Program.GameEngine.Definition.Name + "_" + Program.GameEngine.Definition.Version.ToString();
					Assert.AreEqual(ua, result.Item1);

					// No content
					wl.Response = context => {
						context.Response.StatusCode = 200;
						context.Response.Close();
					};
					result = s.DoWebRequest(wl.Url, 1000);
					Assert.AreEqual(Tuple.Create("No Content Error", 204), result);

					// Proper Status Code
					wl.Response = context => {
						context.Response.StatusCode = 501;
						using (var sw = new StreamWriter(context.Response.OutputStream)) {
							sw.Write("asdf");
						}
						context.Response.Close();
					};
					result = s.DoWebRequest(wl.Url, 1000);
					Assert.AreEqual(Tuple.Create("Error", 501), result);

					wl.Response = context => {
						context.Response.StatusCode = 200;
						using (var sw = new StreamWriter(context.Response.OutputStream)) {
							sw.Write(context.Request.InputStream.ReadToEnd());
						}
						context.Response.Close();
					};
					result = s.DoWebRequest(wl.Url, 1000, "tacos");
					Assert.AreEqual(Tuple.Create("tacos", 200), result);
				}
			}
		}
	}
}
