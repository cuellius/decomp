using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Decomp.Core.Vanilla
{
    public static class Troops
    {
        public static string[] GetIdFromFile(string strFileName)
        {
            var fId = new Text(strFileName);
            fId.GetString();
            var n = fId.GetInt();
            var aTroops = new string[n];
            for (int i = 0; i < n; i++)
            {
                aTroops[i] = fId.GetWord().Remove(0, 4);
                for (int j = 0; j < 162; j++) fId.GetWord();
            }
            fId.Close();

            return aTroops;
        }

        public static void Decompile()
        {
            var fTroops = new Text(Path.Combine(Common.InputPath, "troops.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_troops.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Troops);

            for (int s = 0; s < Common.Skins.Length; s++) fSource.WriteLine("tf_" + Common.Skins[s] + " = " + s);
            fSource.WriteLine("\r\ntroops = [");

            fTroops.GetString();
            var iTroops = fTroops.GetInt();

            var strUpList = new List<string>();

            for (int t = 0; t < iTroops; t++)
            {
                fSource.Write("  [\"{0}\", \"{1}\", \"{2}\",", fTroops.GetWord().Remove(0, 4), fTroops.GetWord().Replace('_', ' '), fTroops.GetWord().Replace('_', ' '));

                var dwFlag = fTroops.GetDWord();
                fSource.Write(" {0},", Core.Troops.DecompileFlags(dwFlag));

                var dwScene = fTroops.GetDWord();
                fSource.Write(" {0},", dwScene == 0 ? "0" : Core.Troops.GetScene(dwScene));

                fSource.Write(" {0},", fTroops.GetWord()); // reserved "0"

                var iFaction = fTroops.GetInt();
                if (iFaction > 0 && iFaction < Common.Factions.Length)
                    fSource.WriteLine(" fac_{0},", Common.Factions[iFaction]);
                else
                    fSource.WriteLine(" {0},", iFaction);

                var iUp1 = fTroops.GetInt();
                var iUp2 = fTroops.GetInt();

                // ReSharper disable once InconsistentNaming
                string fnGetTroopForUpgrade(int id) => id >= 0 && id < Common.Troops.Length ? '"' + Common.Troops[id] + '"' : id.ToString();

                if (iUp1 != 0 && iUp2 != 0)
                    strUpList.Add(
                        $"upgrade2(troops,{fnGetTroopForUpgrade(t)},{fnGetTroopForUpgrade(iUp1)},{fnGetTroopForUpgrade(iUp2)})");
                else if (iUp1 != 0 && iUp2 == 0)
                    strUpList.Add($"upgrade(troops,{fnGetTroopForUpgrade(t)},{fnGetTroopForUpgrade(iUp1)})");

                var itemList = new List<int>(64);
                for (int i = 0; i < 64; i++)
                {
                    var iItem = fTroops.GetInt();
                    fTroops.GetInt(); //skip 0
                    if (-1 == iItem) continue;
                    itemList.Add(iItem);
                }
                fSource.WriteLine("  [{0}],", String.Join(",", itemList.Select(item => item < Common.Items.Length ? $"itm_{Common.Items[item]}" : $"{item}")));

                int iStregth = fTroops.GetInt(),
                    iAgility = fTroops.GetInt(),
                    iIntelligence = fTroops.GetInt(),
                    iCharisma = fTroops.GetInt(),
                    iLevel = fTroops.GetInt();

                fSource.Write("  strength({0})|agility({1})|intellect({2})|charisma({3})|level({4}), ", iStregth, iAgility, iIntelligence, iCharisma, iLevel);

                var iWP = new int[7];
                for (int i = 0; i < 7; i++) iWP[i] = fTroops.GetInt();

                if (iWP[0] == iWP[1] && iWP[1] == iWP[2] && iWP[2] == iWP[3] && iWP[3] == iWP[4] && iWP[4] == iWP[5])
                    fSource.Write("wp({0}){1},", iWP[0], iWP[6] == 0 ? "" : "|wp_firearm(" + iWP[6] + ")");
                else if (iWP[0] == iWP[1] && iWP[1] == iWP[2])
                    fSource.Write("wpe({0},{1},{2},{3}){4},", iWP[0], iWP[3], iWP[4], iWP[5], iWP[6] == 0 ? "" : "|wp_firearm(" + iWP[6] + ")");
                else
                    fSource.Write("wpex({0},{1},{2},{3},{4},{5}){6},", iWP[0], iWP[1], iWP[2], iWP[3], iWP[4], iWP[5], iWP[6] == 0 ? "" : "|wp_firearm(" + iWP[6] + ")");

                var sbKnow = new StringBuilder(512);
                for (int x = 0; x < 6; x++)
                {
                    var dword = fTroops.GetDWord();
                    if (dword == 0) continue;
                    for (int q = 0; q < 8; q++)
                    {
                        var dwKnow = 0xF & (dword >> (q << 2));
                        if (dwKnow != 0 && (x << 3) + q < Common.Skills.Length) sbKnow.Append($"knows_{Common.Skills[(x << 3) + q]}_{dwKnow}|");
                    }
                }

                if (sbKnow.Length == 0)
                    sbKnow.Append('0');
                else
                    sbKnow.Length--;

                fSource.Write(" {0},", sbKnow);

                var strFace =
                    $"0x{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}, 0x{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}";
                fSource.WriteLine("{0}],", strFace);
            }

            fSource.WriteLine("]");
            foreach (var strUp in strUpList) fSource.WriteLine(strUp);
            fSource.Close();
            fTroops.Close();

            Common.GenerateId("ID_troops.py", Common.Troops, "trp");
        }
    }
}
