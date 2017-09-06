using System;
using System.Globalization;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class MapIcons
    {
        public static string[] Initialize()
        {
            var fID = new Text(Common.InputPath + @"\map_icons.txt");
            fID.GetString();
            int n = Convert.ToInt32(fID.GetString());
            var aMapIcons = new string[n];
            for (int i = 0; i < n; i++)
            {
                aMapIcons[i] = fID.GetWord();
                fID.GetWord();
                fID.GetWord();

                fID.GetWord();
                fID.GetWord();
                fID.GetWord();
                fID.GetWord();
                fID.GetWord();

                int iTriggers = fID.GetInt();
                for (int t = 0; t < iTriggers; t++)
                {
                    //idFile.GetString();
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
                }
            }
            fID.Close();

            return aMapIcons;
        }

        public static void Decompile()
        {
            var fIcons = new Text(Common.InputPath + @"\map_icons.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_map_icons.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Icons);
            fIcons.GetString();
            int iMapIcons = fIcons.GetInt();
            for (int iMIcon = 0; iMIcon < iMapIcons; iMIcon++)
            {
                string strName = fIcons.GetWord();
                fSource.Write("  (\"{0}\",", strName);

                DWORD dwFlags = fIcons.GetDWord();
                fSource.Write(" {0},", dwFlags == 1 ? "mcn_no_shadow" : "0");

                string strMeshName = fIcons.GetWord();
                fSource.Write(" \"{0}\",", strMeshName);

                double dScale = fIcons.GetDouble();
                int iSound = fIcons.GetInt();
                double dX = fIcons.GetDouble(), dY = fIcons.GetDouble(), dZ = fIcons.GetDouble();

                fSource.Write(" {0}, {1}, {2}, {3}, {4}", dScale.ToString(CultureInfo.GetCultureInfo("en-US")), iSound != 0 ? "snd_" + Common.Sounds[iSound] : "0", 
                    dX.ToString(CultureInfo.GetCultureInfo("en-US")), dY.ToString(CultureInfo.GetCultureInfo("en-US")), dZ.ToString(CultureInfo.GetCultureInfo("en-US")));

                int iTriggers = fIcons.GetInt();
                if (iTriggers > 0)
                {
                    fSource.Write(",\r\n  [\r\n");
                    for (int t = 0; t < iTriggers; t++)
                    {
                        double dInterval = fIcons.GetDouble();
                        fSource.WriteLine("    ({0},[", Common.GetTriggerParam(dInterval));

                        int iRecords = fIcons.GetInt();
                        Common.PrintStatement(ref fIcons, ref fSource, iRecords, "      ");

                        fSource.WriteLine("    ]),");
                    }
                    fSource.Write("  ]");
                }
                fSource.WriteLine("),");
            }
            fSource.Write("]");
            fSource.Close();
            fIcons.Close();
        }
    }
}
