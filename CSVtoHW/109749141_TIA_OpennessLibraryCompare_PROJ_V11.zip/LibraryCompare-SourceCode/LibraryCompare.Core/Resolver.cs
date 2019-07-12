using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace LibraryCompare.Core
{
    public static class Resolver
    {
        private static Assembly OpennessAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var index = args.Name.IndexOf(',');
            if (index == -1) return null;

            var name = args.Name.Substring(0, index) + ".dll";
            // Check for 64bit installation
            var filePathReg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Siemens\\Automation\\_InstalledSW\\TIAP14\\TIA_Opns") ??
                                                      Registry.LocalMachine.OpenSubKey("SOFTWARE\\Siemens\\Automation\\_InstalledSW\\TIAP14\\TIA_Opns");
            if (filePathReg == null) return null;

            var filePath = filePathReg.GetValue("Path") + "PublicAPI\\V14 SP1";

            var path = Path.Combine(filePath, name);
            // User must provide the correct path
            var fullPath = Path.GetFullPath(path);
            return File.Exists(fullPath) ? Assembly.LoadFrom(fullPath) : null;
        }

        public static void AssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OpennessAssemblyResolve;
        }
    }
}
