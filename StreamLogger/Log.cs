using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Pastel;

namespace StreamLogger
{
    public static class Log
    {
        public static bool DebugMode = false;
        
        private static readonly string infoStr = "[INFO]".Pastel("eee8d5");
        private static readonly string warnStr = "[WARN]".Pastel("b58900");
        private static readonly string errorStr = "[ERROR]".Pastel("dc322f");
        private static readonly string debugStr = "[DEBUG]".Pastel("2aa198");

        private static readonly string infoStrP = "[INFO]";
        private static readonly string warnStrP = "[WARN]";
        private static readonly string errorStrP = "[ERROR]";
        private static readonly string debugStrP = "[DEBUG]";

        private static string logToWrite;

        private static string logFolder;
        public static string LogFolder
        {
            get
            {
                if (logFolder == null)
                {
                    logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                }
                return logFolder;
            }
        }
        public static string LogFile
        {
            get
            {
                return Path.Combine(LogFolder, $"serverLog_{DateTime.Today.ToString("yyyy-MM-dd")}.txt");
            }
        }

        private static string GetTime(bool plain = false)
        {
            string timeStr;
            if (plain)
            {
                timeStr = $"{DateTime.Now.ToString("HH")}:{ DateTime.Now.ToString("mm")}:{ DateTime.Now.ToString("ss")}";
            }
            else
            {
                timeStr = $"{DateTime.Now.ToString("HH").Pastel("586e75")}:{ DateTime.Now.ToString("mm").Pastel("586e75")}:{ DateTime.Now.ToString("ss").Pastel("586e75")}";
            }
            return timeStr;
        }

        public static void Info(object str)
        {
            string _str = $"({GetTime()}) {infoStr}:  {str}";
            string _strP = $"({GetTime(true)}) {infoStrP}:  {str}";
            Console.WriteLine(_str);
            // ConsoleManager.WriteOut(_str, true);
            logToWrite += _strP + Environment.NewLine;
        }

        public static void Warn(object str)
        {
            string _str = $"({GetTime()}) {warnStr}:  {str}";
            string _strP = $"({GetTime(true)}) {warnStrP}:  {str}";
            Console.WriteLine(_str);
            // ConsoleManager.WriteOut(_str, true);
            logToWrite += _strP + Environment.NewLine;
        }

        public static void Error(object str)
        {
            string _str = $"({GetTime()}) {errorStr}: {str}";
            string _strP = $"({GetTime(true)}) {errorStrP}: {str}";
            Console.WriteLine(_str);
            // ConsoleManager.WriteOut(_str, true);
            logToWrite += _strP + Environment.NewLine;
        }

        public static void Debug(object str)
        {
            if (DebugMode)
            {
                string _str = $"({GetTime()}) {debugStr}: {str}";
                string _strP = $"({GetTime(true)}) {debugStrP}: {str}";
                Console.WriteLine(_str);
                // ConsoleManager.WriteOut(_str, true);
                logToWrite += _strP + Environment.NewLine;
            }
        }

        public static void Animate(string[] strs, int delay, int repeat = 1)
        {
            Task.Run(() => _Animate(strs, delay, repeat)).Wait();
        }

        private static void _Animate(string[] strs, int delay, int repeat = 1)
        {
            Console.Write($"({GetTime()}) {infoStr}:  ");
            // ConsoleManager.WriteOut($"({GetTime()}) {infoStr}:  ", false);

            int cursorLeft = Console.CursorLeft;
            int cursorTop = Console.CursorTop;

            int strsL = strs.Length;

            int maxLineLen = 0;

            for (int i = 0; i < strs.Length; i++)
            {
                if (strs[i].Length > maxLineLen)
                {
                    maxLineLen = strs[i].Length;
                }
            }

            for (int i = 0; i < (strsL * repeat) - 1; i++)
            {
                Console.SetCursorPosition(cursorLeft, cursorTop);
                for (int j = 0; j < maxLineLen; j++)
                {
                    Console.Write(" ");
                    // ConsoleManager.WriteOut(" ", false);
                }
                Console.SetCursorPosition(cursorLeft, cursorTop);
                string l = strs[i%strsL];
                Console.Write(l);
                // ConsoleManager.WriteOut(l, false);
                Task.Delay(delay).Wait();
            }
            Console.SetCursorPosition(cursorLeft, cursorTop);
            Console.WriteLine(strs[strsL - 1]);
            // ConsoleManager.WriteOut(strs[strsL - 1], true);
        }

        public static Task WriteLog()
        {
            while (true)
            {
                Thread.Sleep(2000);
                if (!string.IsNullOrEmpty(logToWrite))
                {
                    if (!Directory.Exists(LogFolder))
                    {
                        Directory.CreateDirectory(LogFolder);
                    }
                    File.AppendAllText(LogFile, logToWrite);
                    logToWrite = null;
                }
            }
        }
    }
}
