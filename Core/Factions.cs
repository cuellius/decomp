using System;
using System.Globalization;
using System.IO;
using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Factions
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "factions.txt"))) return new string[0];

            var fId = new Text(Path.Combine(Common.InputPath, "factions.txt"));
            fId.GetString();
            int n = Convert.ToInt32(fId.GetString());
            var aFactions = new string[n];
            for (int i = 0; i < n; i++)
            {
                string strFacId = fId.GetWord();
                if (strFacId == "0") strFacId = fId.GetWord();
                aFactions[i] = strFacId.Remove(0, 4);

                fId.GetWord();
                fId.GetWord();
                fId.GetWord();

                for (int r = 0; r < n; r++) fId.GetDouble();
            }
            fId.Close();

            return aFactions;
        }

        public static void Decompile()
        {
            var fFactions = new Text(Path.Combine(Common.InputPath, "factions.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_factions.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Factions);
            fFactions.GetString();
            int iFactions = fFactions.GetInt();
            for (int f = 0; f < iFactions; f++)
            {
                string strFacId = fFactions.GetWord();
                if (strFacId == "0") strFacId = fFactions.GetWord();
                string strFacName = fFactions.GetWord();
                fSource.Write("  (\"{0}\", \"{1}\",", strFacId.Remove(0, 4), strFacName);

                var sbFlags = new StringBuilder(64);
                DWORD dwFlags = fFactions.GetUInt();
                int iRating = (int)(dwFlags & 0xFF00) >> 8;
                if (iRating != 0) sbFlags.Append($"max_player_rating({100 - iRating})");

                if ((dwFlags & 1) != 0)
                {
                    if (sbFlags.Length > 0) sbFlags.Append('|');
                    sbFlags.Append("ff_always_hide_label");
                }
                if (sbFlags.Length == 0) sbFlags.Append('0');

                fSource.Write(" {0}, 0.0, [", sbFlags);

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

            Common.GenerateId("ID_factions.py", Common.Factions, "fac");
        }
    }
}
