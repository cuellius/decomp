using System;

namespace Decomp.Core
{
    public static class Strings
    {
        public static string[] Initialize()
        {
            var fID = new Win32FileReader(Common.InputPath + @"\strings.txt");
            fID.ReadLine();
            int n = Convert.ToInt32(fID.ReadLine());
            var aStrings = new string[n];
            for (int i = 0; i < n; i++)
            {
                var str = fID.ReadLine();
                if (str != null)
                    aStrings[i] = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Remove(0, 4);
            }
            fID.Close();

            return aStrings;
        }
        
        public static void Decompile()
        {
            var fStrings = new Text(Common.InputPath + @"\strings.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_strings.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Strings);
            fStrings.GetString();
            int iStrings = Convert.ToInt32(fStrings.GetString()); //fStrings.GetInt();

            for (int s = 0; s < iStrings; s++)
            {
                string[] strDataArray = fStrings.GetString().Split(new []{ ' ' }, StringSplitOptions.RemoveEmptyEntries); //.st
                //string strID = fStrings.GetWord();
                //string strText = fStrings.GetWord();
                string strID = strDataArray[0];
                string strText = strDataArray[1];
                fSource.WriteLine("  (\"{0}\", \"{1}\"),", strID.Remove(0, 4), strText.Replace('_', ' '));
                //fSource.Close();
            }

            fSource.Write("]");
            fSource.Close();
            fStrings.Close();
        }
    }
}
