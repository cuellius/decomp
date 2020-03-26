using System;
using System.Globalization;
using System.IO;

namespace Decomp.Core
{
    public static class TableauMaterials
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "tableau_materials.txt"))) return Array.Empty<string>();

            var fId = new Win32FileReader(Path.Combine(Common.InputPath, "tableau_materials.txt"));
            int n = Convert.ToInt32(fId.ReadLine(), CultureInfo.GetCultureInfo("en-US"));
            var aTableauMateriales = new string[n];
            for (int i = 0; i < n; i++)
            {
                var str = fId.ReadLine();
                if (str != null) aTableauMateriales[i] = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Remove(0, 4);
            }
            fId.Close();

            return aTableauMateriales;
        }

        public static void Decompile()
        {
            var fTableaus = new Text(Path.Combine(Common.InputPath, "tableau_materials.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_tableau_materials.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.TableauMaterials);
            int iCount = fTableaus.GetInt();

            for (int i = 0; i < iCount; i++)
            {
                fSource.Write("  (\"{0}\", {1}, \"{2}\", ", fTableaus.GetWord().Remove(0, 4), fTableaus.GetDWord(), fTableaus.GetWord());
                for (int j = 0; j < 6; j++) fSource.Write(" {0},", fTableaus.GetInt());
                fSource.WriteLine("\r\n  [");
                Common.PrintStatement(ref fTableaus, ref fSource, fTableaus.GetInt(), "    ");
                fSource.WriteLine("  ]),\r\n");
            }
            fSource.Write("]");
            fSource.Close();
            fTableaus.Close();
            Common.GenerateId("ID_tableau_materials.py", Common.Tableaus, "tableau");
        }
    }
}
