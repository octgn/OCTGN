using System;

namespace Skylabs.ConsoleHelper
{
    public class ConsoleWriter
    {
        #region EnConsoleEvent enum

        public enum EnConsoleEvent
        {
            Read,
            Wrote,
            ComText
        };

        #endregion

        //public static ConsoleColor CommandTextColor { get { return _CommandTextColor;} set { _CommandTextColor = value;} }
        //public static ConsoleColor OutputColor { get { return _OutputColor; } set { _OutputColor = value; } }

        //private static ConsoleColor _CommandTextColor = ConsoleColor.White;
        //private static ConsoleColor _OutputColor = ConsoleColor.White;
        private static String _commandText = ": ";

        private static EnConsoleEvent _lastEvent = EnConsoleEvent.Wrote;

        public static String CommandText
        {
            get { return _commandText; }
            set { _commandText = value; }
        }

        public static void WriteCt()
        {
            if (_lastEvent == EnConsoleEvent.ComText) return;
            //Console.ForegroundColor = CommandTextColor;
            Console.Out.Write(CommandText);
            _lastEvent = EnConsoleEvent.ComText;
            //Console.ForegroundColor = ConsoleReader.InputColor;
        }

        public static void WriteLine(String st, Boolean writeComText)
        {
            if (_lastEvent == EnConsoleEvent.ComText)
                Console.Out.WriteLine();
            Console.Out.WriteLine(st);
            _lastEvent = EnConsoleEvent.Wrote;
            if (writeComText)
                WriteCt();
        }
    }
}