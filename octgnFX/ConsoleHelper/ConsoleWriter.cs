using System;

namespace Skylabs.ConsoleHelper
{
    public class ConsoleWriter
    {
        public enum enConsoleEvent { Read, Wrote, ComText };

        //public static ConsoleColor CommandTextColor { get { return _CommandTextColor;} set { _CommandTextColor = value;} }
        //public static ConsoleColor OutputColor { get { return _OutputColor; } set { _OutputColor = value; } }
        public static String CommandText { get { return _CommandText; } set { _CommandText = value; } }

        //private static ConsoleColor _CommandTextColor = ConsoleColor.White;
        //private static ConsoleColor _OutputColor = ConsoleColor.White;
        private static String _CommandText = ": ";

        private static enConsoleEvent lastEvent = enConsoleEvent.Wrote;

        public static void writeCT()
        {
            if(lastEvent != enConsoleEvent.ComText)
            {
                //Console.ForegroundColor = CommandTextColor;
                Console.Out.Write(CommandText);
                lastEvent = enConsoleEvent.ComText;
                //Console.ForegroundColor = ConsoleReader.InputColor;
            }
        }

        public static void writeLine(String st, Boolean writeComText)
        {
            if(lastEvent == enConsoleEvent.ComText)
                Console.Out.WriteLine();
            Console.Out.WriteLine(st);
            lastEvent = enConsoleEvent.Wrote;
            if(writeComText)
                writeCT();
        }
    }
}