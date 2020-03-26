using System;
using System.Globalization;
using System.IO;

namespace Decomp.Core
{
    public static class QuickStrings
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "quick_strings.txt"))) return Array.Empty<string>();

            var fId = new Win32FileReader(Path.Combine(Common.InputPath, "quick_strings.txt"));
            int n = Convert.ToInt32(fId.ReadLine(), CultureInfo.GetCultureInfo("en-US"));
            var aQuickStrings = new string[n];
            for (int i = 0; i < n; i++)
            {
                var str = fId.ReadLine();
                if (str != null)
                    aQuickStrings[i] = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1].Replace('_', ' ');
            }
            fId.Close();

            return aQuickStrings;
        }
    }
}
