using System;
using System.Diagnostics;

namespace COM3D2.MaidFiddler.Core.Utils
{
    public struct LogLevel
    {
        public static LogLevel Info = new LogLevel("INFO", ConsoleColor.Blue);

        public static LogLevel Warning = new LogLevel("WARNING", ConsoleColor.Yellow);

        public static LogLevel Error = new LogLevel("ERROR", ConsoleColor.Red);

        public LogLevel(string tag, ConsoleColor col)
        {
            Tag = tag;
            Color = col;
        }

        public ConsoleColor Color { get; }
        public string Tag { get; }
    }

    public static class Debugger
    {
        public delegate void ErrorCallback(Exception e, string errorMessage);

        private const string TAG = "MaidFiddler";
        private const ConsoleColor TAG_COLOR = ConsoleColor.Green;

        public static event ErrorCallback ErrorOccured;

        public static void Assert(Action action, string errMsg)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                ErrorOccured?.Invoke(e, errMsg);
            }
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string s)
        {
            Console.ForegroundColor = TAG_COLOR;
            Console.Write(TAG + ": ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(s);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(LogLevel level, string s)
        {
            Console.ForegroundColor = level.Color;
            Console.Write($"{TAG}[{level.Tag}]: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(s);
        }

        [Conditional("DEBUG")]
        public static void WriteLine()
        {
            Console.WriteLine();
        }
    }
}