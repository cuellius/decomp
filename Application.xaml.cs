using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace Decomp
{
    public partial class Application
    {
#if DEBUG
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool AllocConsole();
#endif    
        
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
#if DEBUG
            AllocConsole();
#endif
            CommandLineArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();

            var key = Registry.CurrentUser.OpenSubKey("Software\\WMD");
            if(key == null) return;
            var language = key.GetValue("Language") as string;
            if (language == "Russian" || language == "English") Language = language;
        }

        public static string[] CommandLineArgs;

        public static string StartupPath => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

        public static string Language
        {
            set
            {
                var newDict = new ResourceDictionary { Source = new Uri($"Languages/{value}.xaml", UriKind.Relative) };

                var oldDict = (from d in Current.Resources.MergedDictionaries where d.Source != null && d.Source.OriginalString.StartsWith("Languages/") select d).First();

                if (oldDict != null)
                {
                    var i = Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Current.Resources.MergedDictionaries.Remove(oldDict);
                    Current.Resources.MergedDictionaries.Insert(i, newDict);
                }
                else
                {
                    Current.Resources.MergedDictionaries.Add(newDict);
                }
            }
        }

        public static string GetResource(string s)
        {
            return (string)Current.FindResource(s);
        }
    }
}
