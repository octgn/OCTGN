// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CassiniDev
{


    public abstract class CassiniDevBrowserTestFixture<T> where T : BrowserTestResultItem, new()
    {

        private string url;
        private TimeSpan _timeOut = TimeSpan.FromMinutes(1);
        private Dictionary<string, BrowserTestResultItem> _results;
        private string _postKey = "log.axd";

        public abstract WebBrowser Browser { get; }
        public abstract string Path { get; }
        public abstract string Url { get; }

        public TimeSpan TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }


        public Dictionary<string, BrowserTestResultItem> Results
        {
            get { return _results; }
        }



        public string PostKey
        {
            get { return _postKey; }
            set { _postKey = value; }
        }


        
        public void RunTest()
        {
            //ContentLocator locator = new ContentLocator(@"RESTWebServices\RESTWebServices");

            var test = new CassiniDevBrowserTest(PostKey);
            test.StartServer(Path);
            url = test.NormalizeUrl(Url);

            var testResults = new BrowserTestResults(test.RunTest(url, Browser, TimeOut));
            var results = new T();
            results.Parse(testResults.Log);
            _results = results.Items;
            test.StopServer();
        }
    }

    /// <summary>
    ///   NOTE: there seems to be a 7k limit on data posted from the test so
    ///   be concious of the data you log
    /// </summary>
    [Serializable]
    public class QUnitExBrowserTestResultItem : BrowserTestResultItem
    {
        private static readonly Regex rx = new Regex(
            @"failures\s*=\s*(?<failures>\d+)\s*;\s*total\s*=\s*(?<total>\d+)",
            RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        public override void Parse(string log)
        {
            // parse it line by line
            var lines = log.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            Log.AddRange(lines);
            BrowserTestResultItem currentItem = this;
            BrowserTestResultItem lastItem = null;
            int index = 0;
            while (index < lines.Length - 1)
            {
                var line = lines[index];
                if (line.StartsWith("Module Started:") || line.StartsWith("  Test Started:"))
                {
                    lastItem = currentItem;
                    currentItem = new BrowserTestResultItem
                        {
                            Name = line.Substring("Module Started:".Length + 1)
                        };
                    if (lastItem == null)
                    {
                        throw new Exception("lst item is null?");
                    }
                    lastItem.Items.Add(currentItem.Name, currentItem);
                }
                else if (line.StartsWith("Module Done:") || line.StartsWith("  Test Done:"))
                {
                    SetCount(currentItem, line);
                    currentItem = lastItem;
                }
                else
                {
                    if (currentItem == null)
                    {
                        throw new Exception("log parse exception");
                    }
                    currentItem.Log.Add(line);
                }

                index++;
            }
            SetCount(this, lines[lines.Length - 1]);
        }

        private static void SetCount(BrowserTestResultItem item, string value)
        {
            int total, failures;
            ParseCount(value, out total, out failures);
            item.Total = total;
            item.Failures = failures;
            item.Success = failures == 0;
        }

        private static void ParseCount(string value, out int total, out int failures)
        {
            var match = rx.Match(value);
            total = Convert.ToInt32(match.Groups["total"].Value);
            failures = Convert.ToInt32(match.Groups["failures"].Value);
        }
    }
}
