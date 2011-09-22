using System;
using System.Collections.Generic;

namespace Skylabs.ConsoleHelper
{
    public struct ConsoleArgument
    {
        public String Argument { get; set; }

        public String Value { get; set; }
    }

    public class ConsoleMessage
    {
        public String RawData
        {
            get
            {
                return _RawData;
            }
            set
            {
                _RawData = value.TrimStart(new char[1] { ' ' });
                parseMessage();
            }
        }

        public String Header { get; set; }

        public List<ConsoleArgument> Args { get; set; }

        private String _RawData;

        public ConsoleMessage()
        {
            RawData = "";
        }

        public ConsoleMessage(String rawData)
        {
            RawData = rawData;
        }

        public void parseMessage(String data)
        {
            RawData = data;
        }

        private void parseMessage()
        {
            //TODO better handling of jibberish. Probubly be best to use regex. It'd be a lot cleaner and sexier.
            RawData.TrimStart(new char[1] { ' ' });
            int ws = RawData.IndexOf(' ');
            Header = "";
            Args = new List<ConsoleArgument>();
            if(ws == -1)
            {
                Header = RawData;
            }
            else
            {
                Header = RawData.Substring(0, ws);
                try
                {
                    String args = RawData.Substring(ws + 1);
                    args.TrimStart(new char[1] { ' ' });
                    if(!args.Equals(""))
                    {
                        String[] araw = args.Split(new char[1] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        if(araw.Length != 0)
                        {
                            foreach(String a in araw)
                            {
                                a.Trim();
                                ws = a.IndexOf(' ');
                                if(ws == -1)
                                {
                                    ConsoleArgument ca = new ConsoleArgument();
                                    ca.Argument = a;
                                    Args.Add(ca);
                                }
                                else
                                {
                                    ConsoleArgument ca = new ConsoleArgument();
                                    ca.Argument = a.Substring(0, ws);
                                    ca.Value = a.Substring(ws + 1);
                                    Args.Add(ca);
                                }
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    new ConsoleEventError("Error parsing arguments. ", e).writeEvent(true);
                }
            }
        }
    }
}