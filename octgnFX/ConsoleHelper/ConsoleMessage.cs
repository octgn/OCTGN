using System;
using System.Collections.Generic;
using System.Linq;

namespace Skylabs.ConsoleHelper
{
    public struct ConsoleArgument
    {
        public String Argument { get; set; }

        public String Value { get; set; }
    }

    public class ConsoleMessage
    {
        private String _rawData;

        public ConsoleMessage()
        {
            RawData = "";
        }

        public ConsoleMessage(String rawData)
        {
            RawData = rawData;
        }

        public String RawData
        {
            get { return _rawData; }
            set
            {
                _rawData = value.TrimStart(new[] {' '});
                ParseMessage();
            }
        }

        public String Header { get; set; }

        public List<ConsoleArgument> Args { get; set; }

        public void ParseMessage(String data)
        {
            RawData = data;
        }

        private void ParseMessage()
        {
            //TODO better handling of jibberish. Probubly be best to use regex. It'd be a lot cleaner and sexier.
            RawData = RawData.TrimStart(new[] {' '});
            int ws = RawData.IndexOf(' ');
            Header = "";
            Args = new List<ConsoleArgument>();
            if (ws == -1)
            {
                Header = RawData;
            }
            else
            {
                Header = RawData.Substring(0, ws);
                try
                {
                    string args = RawData.Substring(ws + 1);
                    args = args.TrimStart(new[] {' '});
                    if (!args.Equals(""))
                    {
                        string[] araw = args.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
                        if (araw.Length != 0)
                        {
                            foreach (string temp in araw.Select(a => a.Trim()))
                            {
                                ws = temp.IndexOf(' ');
                                if (ws == -1)
                                {
                                    var ca = new ConsoleArgument {Argument = temp};
                                    Args.Add(ca);
                                }
                                else
                                {
                                    var ca = new ConsoleArgument
                                                 {
                                                     Argument = temp.Substring(0, ws),
                                                     Value = temp.Substring(ws + 1)
                                                 };
                                    Args.Add(ca);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    new ConsoleEventError("Error parsing arguments. ", e).WriteEvent(true);
                }
            }
        }
    }
}