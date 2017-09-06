using System.Collections.Generic;
using DWORD = System.UInt32;

namespace Decomp.Core.Vanilla
{
    public static class Troops
    {
        public static string[] GetIdFromFile(string strFileName)
        {
            var fID = new Text(strFileName);
            fID.GetString();
            int n = fID.GetInt();
            var aTroops = new string[n];
            for (int i = 0; i < n; i++)
            {
                aTroops[i] = fID.GetWord().Remove(0, 4);
                for (int j = 0; j < 162; j++)
                    fID.GetWord();
            }
            fID.Close();

            return aTroops;
        }

        public static void Decompile()
        {
            var fTroops = new Text(Common.InputPath + @"\troops.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_troops.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Troops);

            for (int s = 0; s < Common.Skins.Length; s++)
            {
                fSource.WriteLine("tf_" + Common.Skins[s] + " = " + s);
            }
            fSource.WriteLine("\r\ntroops = [");

            fTroops.GetString();
            int iTroops = fTroops.GetInt();

            var strUpList = new List<string>();

            for (int t = 0; t < iTroops; t++)
            {
                fSource.Write("  [\"{0}\", \"{1}\", \"{2}\",", fTroops.GetWord().Remove(0, 4), fTroops.GetWord().Replace('_', ' '), fTroops.GetWord().Replace('_', ' '));

                DWORD dwFlag = fTroops.GetDWord();
                fSource.Write(" {0},", Core.Troops.DecompileFlags(dwFlag));

                DWORD dwScene = fTroops.GetDWord();
                fSource.Write(" {0},", dwScene == 0 ? "0" : Core.Troops.GetScene(dwScene));

                fSource.Write(" {0},", fTroops.GetWord()); // reserved "0"

                int iFaction = fTroops.GetInt();
                if (iFaction > 0 && iFaction < Common.Factions.Length)
                    fSource.WriteLine(" fac_{0},", Common.Factions[iFaction]);
                else
                    fSource.WriteLine(" {0},", iFaction);

                int iUp1 = fTroops.GetInt();
                int iUp2 = fTroops.GetInt();

                if (iUp1 != 0 && iUp2 != 0)
                    strUpList.Add(
                        $"upgrade2(troops,\"{Common.Troops[t]}\",\"{Common.Troops[iUp1]}\",\"{Common.Troops[iUp2]}\")");
                else if (iUp1 != 0 && iUp2 == 0)
                    strUpList.Add($"upgrade(troops,\"{Common.Troops[t]}\",\"{Common.Troops[iUp1]}\")");

                string strItemList = "";
                for (int i = 0; i < 64; i++)
                {
                    int iItem = fTroops.GetInt();
                    fTroops.GetInt(); //skip 0
                    if (-1 == iItem)
                        continue;
                    strItemList += $"itm_{Common.Items[iItem]},";
                }
                if (strItemList.Length > 0)
                    strItemList = strItemList.Remove(strItemList.Length - 1, 1);
                fSource.WriteLine("  [{0}],", strItemList);

                int iStregth = fTroops.GetInt(),
                    iAgility = fTroops.GetInt(),
                    iIntelligence = fTroops.GetInt(),
                    iCharisma = fTroops.GetInt(),
                    iLevel = fTroops.GetInt();

                fSource.Write("  strength({0})|agility({1})|intellect({2})|charisma({3})|level({4}), ", iStregth, iAgility, iIntelligence, iCharisma, iLevel);

                var iWP = new int[7];
                for (int i = 0; i < 7; i++)
                    iWP[i] = fTroops.GetInt();

                if (iWP[0] == iWP[1] && iWP[1] == iWP[2] && iWP[2] == iWP[3] && iWP[3] == iWP[4] && iWP[4] == iWP[5])
                    fSource.Write("wp({0}){1},", iWP[0], iWP[6] == 0 ? "" : "|wp_firearm(" + iWP[6] + ")");
                else if (iWP[0] == iWP[1] && iWP[1] == iWP[2])
                    fSource.Write("wpe({0},{1},{2},{3}){4},", iWP[0], iWP[3], iWP[4], iWP[5], iWP[6] == 0 ? "" : "|wp_firearm(" + iWP[6] + ")");
                else
                    fSource.Write("wpex({0},{1},{2},{3},{4},{5}){6},", iWP[0], iWP[1], iWP[2], iWP[3], iWP[4], iWP[5], iWP[6] == 0 ? "" : "|wp_firearm(" + iWP[6] + ")");

                string strKnow = "";
                for (int x = 0; x < 6; x++)
                {
                    DWORD dword = fTroops.GetDWord();
                    if (dword == 0)
                        continue;
                    for (int q = 0; q < 8; q++)
                    {
                        DWORD dwKnow = (0xF & (dword >> (q * 4)));
                        /*if (dwKnow != 0 && dwKnow <= 8)
                            strKnow = strKnow + String.Format("knows_{0}_{1}|", Common.Skills[x * 8 + q], dwKnow);
                        else*/
                        if (dwKnow != 0)
                            strKnow += $"knows_{Common.Skills[x*8 + q]}_{dwKnow}|";
                    }
                }
                strKnow = strKnow == "" ? "0" : strKnow.Remove(strKnow.Length - 1, 1);
                fSource.Write(" {0},", strKnow);

                string strFase =
                    $"0x{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}, 0x{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}";
                fSource.WriteLine("{0}],", strFase);
            }

            fSource.WriteLine("]");
            foreach (var strUp in strUpList)
            {
                fSource.WriteLine(strUp);
            }
            fSource.Close();
            fTroops.Close();
        }
    }
}
