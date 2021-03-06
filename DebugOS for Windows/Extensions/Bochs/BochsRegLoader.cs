﻿using System;
using System.Linq;
using Microsoft.Win32;

namespace DebugOS.Extensions
{
    /// <summary>
    /// Extension which searches for Bochs' path in the registry.
    /// </summary>
    public sealed class BochsRegLoader : IExtension
    {
        public string Name
        {
            get { return "Bochs Registry Loader"; }
        }

        public void Initialise(string[] args)
        {
            string bochshome = System.Environment.GetEnvironmentVariable("BOCHSHOME");

            // If we already have a valid bochshome, don't bother
            if (!String.IsNullOrWhiteSpace(bochshome) && System.IO.Directory.Exists(bochshome)) 
            {
                return;
            }

            // Look for registry entries
            var programs = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node");

            var bochsKeys = from    key in programs.GetSubKeyNames() 
                            where   key.StartsWith("Bochs ") 
                            orderby key descending
                            select  programs.OpenSubKey(key);

            foreach (RegistryKey key in bochsKeys)
            {
                string path = key.GetValue(null) as string;

                if (System.IO.Directory.Exists(path))
                {
                    Environment.SetEnvironmentVariable("BOCHSHOME", path);
                    break;
                }
            }
        }
    }
}
