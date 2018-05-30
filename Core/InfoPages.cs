using System.IO;

namespace Decomp.Core
{
    public static class InfoPages
    {
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
