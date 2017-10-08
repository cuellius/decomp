using System;
using System.Globalization;
using System.IO;

namespace Decomp.Core
{
    public static class Meshes
    {
        public static string[] Initialize()
        {
            var fID = new Win32FileReader(Common.InputPath + @"\meshes.txt");
            int n = Convert.ToInt32(fID.ReadLine());
            var aMeshes = new string[n];
            for (int i = 0; i < n; i++)
            {
                var str = fID.ReadLine();
                if (str != null)
                    aMeshes[i] = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Remove(0, 5);
            }
            fID.Close();

            return aMeshes;
        }

        public static void Decompile()
        {
            var fMeshes = new Text(Common.InputPath + @"\meshes.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_meshes.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Meshes);
            int iMeshes = fMeshes.GetInt();
            for (int m = 0; m < iMeshes; m++)
            {
                fSource.Write("  (\"{0}\", ", fMeshes.GetWord().Remove(0, 5));

                int iFlag = fMeshes.GetInt();
                if (iFlag == 1)
                    fSource.Write("render_order_plus_1,");
                else
                    fSource.Write("{0},", iFlag);

                fSource.Write(" \"{0}\"", fMeshes.GetWord());
                
                for (int i = 0; i < 9; i++)
                    fSource.Write(", {0}", fMeshes.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US")));
                fSource.WriteLine("),");
            }
            fSource.Write("]");
            fSource.Close();
            fMeshes.Close();

            Common.GenerateId("ID_meshes.py", Common.Meshes, "mesh");
        }
    }
}
