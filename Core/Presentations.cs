using System;
using System.Globalization;
using System.IO;

namespace Decomp.Core
{
    public static class Presentations
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "presentations.txt"))) return Array.Empty<string>();

            var fId = new Text(Path.Combine(Common.InputPath, "presentations.txt"));
            fId.GetString();
            int n = fId.GetInt();
            var aPresentations = new string[n];
            for (int i = 0; i < n; i++)
            {
                aPresentations[i] = fId.GetWord().Remove(0, 6);
                fId.GetWord();
                fId.GetWord();

                var iEvents = fId.GetInt();

                while (iEvents != 0)
                {
                    fId.GetWord();

                    int iRecords = fId.GetInt();
                    if (iRecords != 0)
                    {
                        for (int r = 0; r < iRecords; r++)
                        {
                            fId.GetWord();
                            int iParams = fId.GetInt();
                            for (int p = 0; p < iParams; p++) fId.GetWord();
                        }
                    }
                    iEvents--;
                }
            }
            fId.Close();

            return aPresentations;
        }

        public static string DecompileFlags(int iFlag) => iFlag switch
        {
            3 => "prsntf_read_only|prsntf_manual_end_only",
            2 => "prsntf_manual_end_only",
            1 => "prsntf_read_only",
            _ => iFlag.ToString(CultureInfo.GetCultureInfo("en-US"))
        };

        public static void Decompile()
        {
            var fPresentations = new Text(Path.Combine(Common.InputPath, "presentations.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_presentations.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Presentations);
            fPresentations.GetString();
            int iPresentations = fPresentations.GetInt();
            for (int i = 0; i < iPresentations; i++)
            {
                fSource.Write("  (\"{0}\"", fPresentations.GetWord().Remove(0, 6));

                int iFlag = fPresentations.GetInt();
                fSource.Write(", {0}", DecompileFlags(iFlag));

                int iMesh = fPresentations.GetInt();
                if (iMesh >= 0 && iMesh < Common.Meshes.Count)
                    fSource.Write(", mesh_{0}", Common.Meshes[iMesh]);
                else
                    fSource.Write(", {0}", iMesh);
                fSource.Write(",\r\n  [\r\n");

                int iTriggers = fPresentations.GetInt();
                for (int t = 0; t < iTriggers; t++)
                {
                    var dInterval = fPresentations.GetDouble();
                    fSource.Write("    ({0},\r\n    [\r\n", Common.GetTriggerParam(dInterval));
                    int iRecords = fPresentations.GetInt();
                    if (iRecords != 0) Common.PrintStatement(ref fPresentations, ref fSource, iRecords, "      ");
                    fSource.Write("    ]),\r\n");
                }
                fSource.Write("  ]),\r\n\r\n");
            }
            fSource.Write("]");
            fSource.Close();
            fPresentations.Close();

            Common.GenerateId("ID_presentations.py", Common.Presentations, "prsnt");
        }
    }
}
