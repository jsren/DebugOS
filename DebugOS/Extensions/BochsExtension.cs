using DebugOS.Loaders;
using System;

namespace DebugOS.Extensions
{
    public class BochsExtension : IDebugExtension
    {
        public void SetupUI(IDebugUI UI, IDebugger debugger) {
            return;
        }

        public string Name { get { return "Bochs Debugger"; } }

        public void Initialise(string[] args)
        {
            // Ignore if debugger already initialised
            if (App.Debugger != null) return; 

            // Check for bochs args
            if (args.Length < 2 || args[1] != "bochs") return;

            string cfgPath   = null;
            string bochsPath = null;
            string objPath   = args[0];

            // Get config file path
            for (int i = 2; i < args.Length - 1; i++)
            {
                if (args[i] == "-bxrc") {
                    cfgPath = args[i + 1];
                }
                if (args[i] == "-bxpath") {
                    bochsPath = args[i + 1];
                }
            }

            if (cfgPath == null)
                throw new Exception("Config file not specified.");

            // If not given, try to get bochs path from env. var
            bochsPath = bochsPath ?? 
                System.Environment.GetEnvironmentVariable("BOCHSHOME");

            if (bochsPath == null) 
                throw new Exception("Bochs installation path not specified");

            bochsPath = System.IO.Path.Combine(bochsPath, "bochsdbg.exe");

            App.Debugger = new Bochs.BochsDebugger(bochsPath, cfgPath);
            var objFile = ObjectFileLoader.Load(objPath, AssemblySyntax.Intel);
            App.Debugger.IncludeObjectFile(objFile);
        }
    }
}
