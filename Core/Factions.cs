using System;
using System.Globalization;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Factions
    {
        public static string[] Initialize()
        {
            var fID = new Text(Common.InputPath + @"\factions.txt");
            fID.GetString();
            int n = Convert.ToInt32(fID.GetString());
            var aFactions = new string[n];
            for (int i = 0; i < n; i++)
            {
                string strFacID = fID.GetWord();
                if (strFacID == "0")
                    strFacID = fID.GetWord();
                aFactions[i] = strFacID.Remove(0, 4);

                fID.GetWord();
                fID.GetWord();
                fID.GetWord();

                for (int r = 0; r < n; r++)
                {
                    fID.GetDouble();
                }
            }
            fID.Close();

            return aFactions;
        }

        public static void Decompile()
        {
            var fFactions = new Text(Common.InputPath + @"\factions.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_factions.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Factions);
            fFactions.GetString();
            int iFactions = fFactions.GetInt();
            for (int f = 0; f < iFactions; f++)
            {
                string strFacID = fFactions.GetWord();
                if (strFacID == "0")
                    strFacID = fFactions.GetWord();
                string strFacName = fFactions.GetWord();
                fSource.Write("  (\"{0}\", \"{1}\",", strFacID.Remove(0, 4), strFacName);

                string strFlags = "";
                DWORD dwFlags = fFactions.GetUInt();
                int iRating = ((int)(dwFlags & 0xFF00)) >> 8;
                if (iRating != 0)
                    strFlags = $"max_player_rating({100 - iRating})";

                if ((dwFlags & 1) != 0)
                {
                    if (strFlags != "")
                        strFlags = strFlags + "|";
                    strFlags += "ff_always_hide_label";
                }
                if (strFlags == "")
                    strFlags = "0";

                fSource.Write(" {0}, 0.0, [", strFlags);

                DWORD dwColor = fFactions.GetUInt();

                string strRelations = "";
                for (int r = 0; r < iFactions; r++)
                {
                    double rRelation = fFactions.GetDouble();
                    if (Math.Abs(rRelation) > 0.000001)
                        strRelations +=
                            $"(\"{Common.Factions[r]}\", {rRelation.ToString(CultureInfo.GetCultureInfo("en-US"))}),";
                }
                if (strRelations != "") strRelations = strRelations.Remove(strRelations.Length - 1, 1);

                fSource.Write("{0}], []", strRelations);

                if (dwColor != 0xAAAAAA)
                    fSource.Write(", 0x{0:X}", dwColor);

                fSource.WriteLine("),");
            }
            fSource.Write("]");
            fSource.Close();
            fFactions.Close();
        }
    }
}
