using System.Globalization;

namespace Decomp.Core
{
    public static class Presentations
    {
        public static string[] Initialize()
        {
            var fID = new Text(Common.InputPath + @"\presentations.txt");
            fID.GetString();
            int n = fID.GetInt();
            var aPresentations = new string[n];
            for (int i = 0; i < n; i++)
            {
                aPresentations[i] = fID.GetWord().Remove(0, 6);
                //idPresentations[i - 1] = presentation.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Remove(0, 6);
                //var numEvents = Convert.ToInt32(presentation.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[3]);
                fID.GetWord();
                fID.GetWord();

                var iEvents = fID.GetInt();

                while (iEvents != 0)
                {
                    fID.GetWord();

                    int iRecords = fID.GetInt();
                    if (iRecords != 0)
                    {
                        for (int r = 0; r < iRecords; r++)
                        {
                            fID.GetWord();
                            int iParams = fID.GetInt();
                            for (int p = 0; p < iParams; p++)
                            {
                                fID.GetWord();
                            }
                        }
                    }
                    iEvents--;
                }

                //idFile.ReadLine();
                //idFile.ReadLine();
            }
            fID.Close();

            return aPresentations;
        }

        public static string DecompileFlags(int iFlag)
        {
            switch (iFlag)
            {
                case 3:
                    return "prsntf_read_only|prsntf_manual_end_only";
                case 2:
                    return "prsntf_manual_end_only";
                case 1:
                    return "prsntf_read_only";
                default:
                    return iFlag.ToString(CultureInfo.GetCultureInfo("en-US"));
            }
        }

        public static void Decompile()
        {
            var fPresentations = new Text(Common.InputPath + @"\presentations.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_presentations.py");
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
                if (iMesh >= 0 && iMesh < Common.Meshes.Length)
                    fSource.Write(", mesh_{0}", Common.Meshes[iMesh]);
                else
                    fSource.Write(", {0}", iMesh);
                fSource.Write(",\r\n  [\r\n");

                int iTriggers = fPresentations.GetInt();
                for (int t = 0; t < iTriggers; t++)
                {
                    double dInterval = fPresentations.GetDouble();
                    fSource.Write("    ({0},\r\n    [\r\n", Common.GetTriggerParam(dInterval));
                    int iRecords = fPresentations.GetInt();
                    if (iRecords != 0)
                    {
                        //memcpy(indention, "      ", 7);
                        Common.PrintStatement(ref fPresentations, ref fSource, iRecords, "      ");
                    }
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
