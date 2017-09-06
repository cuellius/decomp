using System;

namespace Decomp.Core
{
    public static class QuickStrings
    {
        public static string[] Initialize()
        {
            var fID = new Win32FileReader(Common.InputPath + @"\quick_strings.txt");
            int n = Convert.ToInt32(fID.ReadLine());
            var aQuickStrings = new string[n];
            for (int i = 0; i < n; i++)
            {
                var str = fID.ReadLine();
                if (str != null)
                    aQuickStrings[i] = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1].Replace('_', ' ');
            }
            fID.Close();

            return aQuickStrings;
        }
    }
}
