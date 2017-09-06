using System;

namespace Decomp.Core
{
    public static class TableauMaterials
    {
        public static string[] Initialize()
        {
            var fID = new Win32FileReader(Common.InputPath + @"\tableau_materials.txt");
            int n = Convert.ToInt32(fID.ReadLine());
            var aTableauMateriales = new string[n];
            for (int i = 0; i < n; i++)
            {
                var str = fID.ReadLine();
                if (str != null)
                    aTableauMateriales[i] = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Remove(0, 4);
            }
            fID.Close();

            return aTableauMateriales;
        }

        public static void Decompile()
        {
            var fTableaus = new Text(Common.InputPath + @"\tableau_materials.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_tableau_materials.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.TableauMaterials);
            int iCount = fTableaus.GetInt();

            for (int i = 0; i < iCount; i++)
            {
                fSource.Write("  (\"{0}\", {1}, \"{2}\", ", fTableaus.GetWord().Remove(0, 4), fTableaus.GetDWord(), fTableaus.GetWord());
                for (int j = 0; j < 6; j++)
                    fSource.Write(" {0},", fTableaus.GetInt());
                fSource.WriteLine("\r\n  [");
                Common.PrintStatement(ref fTableaus, ref fSource, fTableaus.GetInt(), "    ");
                fSource.WriteLine("  ]),\r\n");
            }
            fSource.Write("]");
            fSource.Close();
            fTableaus.Close();
        }
    }
}
