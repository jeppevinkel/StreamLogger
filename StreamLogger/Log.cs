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
        
        private static readonly string InfoStr = "[INFO]".Pastel("eee8d5");
        private static readonly string WarnStr = "[WARN]".Pastel("b58900");
        private static readonly string ErrorStr = "[ERROR]".Pastel("dc322f");
        private static readonly string DebugStr = "[DEBUG]".Pastel("2aa198");

        private const string InfoStrP = "[INFO]";
        private const string WarnStrP = "[WARN]";
        private const string ErrorStrP = "[ERROR]";
        private const string DebugStrP = "[DEBUG]";

        private static string _logToWrite;

        private static string _logFolder;
        public static string LogFolder
        {
            get
            {
                if (_logFolder == null)
                {
                    _logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                }
                return _logFolder;
            }
        }
        public static string LogFile => Path.Combine(LogFolder, $"Log_{DateTime.Today:yyyy-MM-dd}.txt");

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
            string _str = $"({GetTime()}) {InfoStr}:  {str}";
            string _strP = $"({GetTime(true)}) {InfoStrP}:  {str}";
            Console.WriteLine(_str);
            // ConsoleManager.WriteOut(_str, true);
            _logToWrite += _strP + Environment.NewLine;
        }

        public static void Warn(object str)
        {
            string _str = $"({GetTime()}) {WarnStr}:  {str}";
            string _strP = $"({GetTime(true)}) {WarnStrP}:  {str}";
            Console.WriteLine(_str);
            // ConsoleManager.WriteOut(_str, true);
            _logToWrite += _strP + Environment.NewLine;
        }

        public static void Error(object str)
        {
            string _str = $"({GetTime()}) {ErrorStr}: {str}";
            string _strP = $"({GetTime(true)}) {ErrorStrP}: {str}";
            Console.WriteLine(_str);
            // ConsoleManager.WriteOut(_str, true);
            _logToWrite += _strP + Environment.NewLine;
        }

        public static void Debug(object str)
        {
            if (DebugMode)
            {
                string _str = $"({GetTime()}) {DebugStr}: {str}";
                string _strP = $"({GetTime(true)}) {DebugStrP}: {str}";
                Console.WriteLine(_str);
                // ConsoleManager.WriteOut(_str, true);
                _logToWrite += _strP + Environment.NewLine;
            }
        }

        public static void Animate(string[] strs, int delay, int repeat = 1)
        {
            Task.Run(() => _Animate(strs, delay, repeat)).Wait();
        }

        private static void _Animate(string[] strs, int delay, int repeat = 1)
        {
            Console.Write($"({GetTime()}) {InfoStr}:  ");
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
                if (!string.IsNullOrEmpty(_logToWrite))
                {
                    if (!Directory.Exists(LogFolder))
                    {
                        Directory.CreateDirectory(LogFolder);
                    }
                    File.AppendAllText(LogFile, _logToWrite);
                    _logToWrite = null;
                }
            }
        }
    }
}
