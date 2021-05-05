using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using StreamLogger.Loader;

namespace StreamLogger
{
    class Program
    {
        //static DiscordIntegration di = new();
        static Task LogWriter;

        static void Main(string[] args)
        {
            LogWriter = Task.Run(Log.WriteLog);
            AppDomain.CurrentDomain.AssemblyResolve += 
                CurrentDomain_AssemblyResolve;

            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            
            IntegrationLoader.Run();

            Console.ReadLine();
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception) e.ExceptionObject;
            Log.Error($"[UNHANDLED] {exception}");
        }

        private static HashSet<string> IgnoredAssemblies = new HashSet<string>
        {
            "Serilog.dll",
            "NLog.dll",
            "log4net.dll",
            "Loupe.Agent.NETCore.dll",
            "Gibraltar.Agent.dll"
        };

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
            {
                Log.Debug($"Assembly found in CurrentDomain: {assembly.FullName}");
                return assembly;
            }

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            string filename = args.Name.Split(',')[0] + ".dll".ToLower();
            if (IgnoredAssemblies.Contains(filename)) return null;

            string asmFile = Path.Combine(@".\","Dependencies",filename);
    
            try
            {
                assembly = Assembly.LoadFrom(asmFile);
                Log.Debug($"Assembly loaded from dependencies folder: {assembly.FullName}");
                return assembly;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }
    }
}