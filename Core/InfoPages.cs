using System.IO;

namespace Decomp.Core
{
    public static class InfoPages
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "info_pages.txt"))) return new string[0];

            var fId = new Text(Path.Combine(Common.InputPath, "info_pages.txt"));
            fId.GetString();
            var iInfoPages = fId.GetInt();

            var infoPages = new string[iInfoPages];
            for (int i = 0; i < iInfoPages; i++)
            {
                infoPages[i] = fId.GetWord().Remove(0, 3);
                fId.GetWord();
                fId.GetWord();
            }

            return infoPages;
        }

        public static void Decompile()
        {
            var fInfoPages = new Text(Path.Combine(Common.InputPath, "info_pages.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_info_pages.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.InfoPages);
            fInfoPages.GetString();
            int iInfoPages = fInfoPages.GetInt();

            var infoPages = new string[iInfoPages];
            for (int i = 0; i < iInfoPages; i++)
            {
                infoPages[i] = fInfoPages.GetWord().Remove(0, 3);
                fSource.WriteLine("  (\"{0}\", \"{1}\", \"{2}\"),", infoPages[i], fInfoPages.GetWord().Replace('_', ' '), fInfoPages.GetWord().Replace('_', ' '));
            }

            fSource.Write("]");
            fSource.Close();
            fInfoPages.Close();

            Common.GenerateId("ID_info_pages.py", infoPages, "ip");
        }
    }
}
