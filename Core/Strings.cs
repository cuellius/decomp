using System;
using System.Globalization;
using System.IO;

namespace Decomp.Core
{
    public static class Strings
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "strings.txt"))) return Array.Empty<string>();

            var fId = new Win32FileReader(Path.Combine(Common.InputPath, "strings.txt"));
            fId.ReadLine();
            int n = Convert.ToInt32(fId.ReadLine(), CultureInfo.GetCultureInfo("en-US"));
            var aStrings = new string[n];
            for (int i = 0; i < n; i++)
            {
                var str = fId.ReadLine();
                if (str != null)
                    aStrings[i] = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Remove(0, 4);
            }
            fId.Close();

            return aStrings;
        }
        
        public static void Decompile()
        {
            var fStrings = new Text(Path.Combine(Common.InputPath, "strings.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_strings.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Strings);
            fStrings.GetString();
            int iStrings = Convert.ToInt32(fStrings.GetString(), CultureInfo.GetCultureInfo("en-US")); //fStrings.GetInt();

            for (int s = 0; s < iStrings; s++)
            {
                var str = fStrings.GetString();
                if(str == null) continue;
                var strDataArray = str.Split(new []{ ' ' }, StringSplitOptions.RemoveEmptyEntries); //.st
                //string strID = fStrings.GetWord();
                //string strText = fStrings.GetWord();
                var strId = strDataArray[0];
                var strText = strDataArray[1];
                //Console.WriteLine($@"process {s} = ""{strID}""");
                fSource.WriteLine("  (\"{0}\", \"{1}\"),", strId.Remove(0, 4), strText.Replace('_', ' '));
                //fSource.Close();
            }

            fSource.Write("]");
            fSource.Close();
            fStrings.Close();

            Common.GenerateId("ID_strings.py", Common.Strings, "str");
        }
    }
}
