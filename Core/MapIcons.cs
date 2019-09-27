using System;
using System.Globalization;
using System.IO;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class MapIcons
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "map_icons.txt"))) return new string[0];

            var fId = new Text(Path.Combine(Common.InputPath, "map_icons.txt"));
            fId.GetString();
            var n = Convert.ToInt32(fId.GetString());
            var aMapIcons = new string[n];
            for (int i = 0; i < n; i++)
            {
                aMapIcons[i] = fId.GetWord();
                fId.GetWord();
                fId.GetWord();

                fId.GetWord();
                fId.GetWord();
                fId.GetWord();
                fId.GetWord();
                fId.GetWord();

                var iTriggers = fId.GetInt();
                for (int t = 0; t < iTriggers; t++)
                {
                    fId.GetWord();

                    var iRecords = fId.GetInt();
                    if (iRecords != 0)
                    {
                        for (int r = 0; r < iRecords; r++)
                        {
                            fId.GetWord();
                            var iParams = fId.GetInt();
                            for (int p = 0; p < iParams; p++) fId.GetWord();
                        }
                    }
                }
            }
            fId.Close();

            return aMapIcons;
        }

        public static void Decompile()
        {
            var fIcons = new Text(Path.Combine(Common.InputPath, "map_icons.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_map_icons.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Icons);
            fIcons.GetString();
            int iMapIcons = fIcons.GetInt();
            for (int iMIcon = 0; iMIcon < iMapIcons; iMIcon++)
            {
                var strName = fIcons.GetWord();
                fSource.Write("  (\"{0}\",", strName);

                DWORD dwFlags = fIcons.GetDWord();
                fSource.Write(" {0},", dwFlags == 1 ? "mcn_no_shadow" : "0");

                var strMeshName = fIcons.GetWord();
                fSource.Write(" \"{0}\",", strMeshName);

                var dScale = fIcons.GetDouble();
                int iSound = fIcons.GetInt();
                double dX = fIcons.GetDouble(), dY = fIcons.GetDouble(), dZ = fIcons.GetDouble();

                fSource.Write(" {0}, {1}, {2}, {3}, {4}", dScale.ToString(CultureInfo.GetCultureInfo("en-US")), iSound != 0 ? (iSound < Common.Sounds.Length ? "snd_" + Common.Sounds[iSound] : iSound.ToString()) : "0", 
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

            Common.GenerateId("ID_map_icons.py", Common.MapIcons, "icon");
        }
    }
}
