using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Troops
    {
        public class Upgrade
        {
            private readonly int _upgradeFrom;
            private readonly int _upgradeTo;

            public Upgrade(int from, int to)
            {
                _upgradeFrom = from;
                _upgradeTo = to;
            }

            public override string ToString()
            {
                return Math.Max(_upgradeFrom, _upgradeTo) >= Common.Troops.Length ? "" : $"upgrade(troops,\"{Common.Troops[_upgradeFrom]}\",\"{Common.Troops[_upgradeTo]}\")";
            }
        }

        public class Upgrade2
        {
            private readonly int _upgradeFrom;
            private readonly int _upgradeTo;
            private readonly int _upgradeTo2;

            public Upgrade2(int from, int to, int to2)
            {
                _upgradeFrom = from;
                _upgradeTo = to;
                _upgradeTo2 = to2;
            }

            public override string ToString()
            {
                return Math.Max(Math.Max(_upgradeFrom, _upgradeTo), _upgradeTo2) >= Common.Troops.Length ? "" :
                    $"upgrade2(troops,\"{Common.Troops[_upgradeFrom]}\",\"{Common.Troops[_upgradeTo]}\",\"{Common.Troops[_upgradeTo2]}\")";
            }
        }

        public static string[] Initialize()
        {
            var fID = new Text(Common.InputPath + @"\troops.txt");
            fID.GetString();
            int n = fID.GetInt();
            var aTroops = new string[n];
            for (int i = 0; i < n; i++)
            {
                string strTroopID = fID.GetWord();
                aTroops[i] = strTroopID.Remove(0, 4);

                /*for (int j = 0; j < 9; j++)
                {
                    fID.GetWord();
                }

                for (int j = 0; j < 64; j++)
                {
                    fID.GetWord();
                    fID.GetWord();
                }

                for (int j = 0; j < 26; j++)
                    fID.GetWord();*/

                for (int j = 0; j < 163; j++)
                    fID.GetWord();
                if (Common.SelectedMode == Mode.Caribbean) fID.GetWord();
            }
            fID.Close();

            return aTroops;
        }

        public static string GetScene(DWORD dwScene)
        {
            DWORD dwEntry = (dwScene & 0xFFFF0000) >> 16;
            DWORD dwID = dwScene & 0xFFFF;

            return dwID < Common.Scenes.Length ? 
                "scn_" + Common.Scenes[dwID] + "|entry(" + dwEntry + ")" :
                $"{dwID}|entry({dwEntry})";
        }

        public static string DecompileFlags(DWORD dwFlag)
        {
            var sbFlag = new StringBuilder(1024);

            DWORD dwSkin = dwFlag & 0xF;
            if(dwSkin > 0)
                sbFlag.Append(dwSkin < Common.Skins.Length ? "tf_" + Common.Skins[dwSkin] + "|" : $"{dwSkin}|");

            if ((dwFlag & 0x7F00000) - 0x7F00000 == 0)
            {
                sbFlag.Append("tf_guarantee_all|");
                dwFlag ^= 0x7F00000;
            }
            else if ((dwFlag & 0x3F00000) - 0x3F00000 == 0)
            {
                sbFlag.Append("tf_guarantee_all_wo_ranged|");
                dwFlag ^= 0x3F00000;
            }
            string[] strFlags = { "tf_hero", "tf_inactive", "tf_unkillable", "tf_allways_fall_dead", "tf_no_capture_alive", "tf_mounted", 
            "tf_is_merchant", "tf_randomize_face", "tf_guarantee_boots", "tf_guarantee_armor", "tf_guarantee_helmet", "tf_guarantee_gloves", 
            "tf_guarantee_horse", "tf_guarantee_shield", "tf_guarantee_ranged", "tf_unmoveable_in_party_window" };
            DWORD[] dwFlags = { 0x00000010, 0x00000020, 0x00000040, 0x00000080, 0x00000100, 0x00000400, 0x00001000, 0x00008000, 0x00100000, 
            0x00200000, 0x00400000, 0x00800000, 0x01000000, 0x02000000, 0x04000000, 0x10000000 };
            for (int i = 0; i < dwFlags.Length; i++)
            {
                if ((dwFlag & dwFlags[i]) != 0)
                {
                    sbFlag.Append(strFlags[i]);
                    sbFlag.Append('|');
                    dwFlag ^= dwFlags[i];
                }
            }

            if (sbFlag.Length == 0)
                sbFlag.Append('0');
            else
                sbFlag.Length--;

            return sbFlag.ToString();
        }

        public static void Decompile()
        {
            var fTroops = new Text(Common.InputPath + @"\troops.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_troops.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Troops);

            for (int s = 0; s < Common.Skins.Length; s++) 
                fSource.WriteLine("tf_" + Common.Skins[s] + " = " + s); 

            fSource.WriteLine("\r\ntroops = [");
            
            fTroops.GetString();
            int iTroops = fTroops.GetInt();

            var aUpList = new List<object>();

            for (int t = 0; t < iTroops; t++)
            {
                fSource.Write("  [\"{0}\", \"{1}\", \"{2}\",", fTroops.GetWord().Remove(0, 4), fTroops.GetWord().Replace('_', ' '), fTroops.GetWord().Replace('_', ' '));
                fTroops.GetWord();

                DWORD dwFlag = fTroops.GetDWord();
                fSource.Write(" {0},", DecompileFlags(dwFlag));

                DWORD dwScene = fTroops.GetDWord();
                fSource.Write(" {0},", dwScene == 0 ? "0" : GetScene(dwScene));

                fSource.Write(" {0},", fTroops.GetWord()); // reserved "0"

                int iFaction = fTroops.GetInt();
                if (iFaction > 0 && iFaction < Common.Factions.Length)
                    fSource.WriteLine(" fac_{0},", Common.Factions[iFaction]);
                else
                    fSource.WriteLine(" {0},", iFaction);

                int iUp1 = fTroops.GetInt();
                int iUp2 = fTroops.GetInt();

                /*if (iUp1 != 0 && iUp2 != 0)
                    strUpList.Add(
                        $"upgrade2(troops,\"{Common.Troops[t]}\",\"{Common.Troops[iUp1]}\",\"{Common.Troops[iUp2]}\")");
                else if (iUp1 != 0 && iUp2 == 0)
                    strUpList.Add($"upgrade(troops,\"{Common.Troops[t]}\",\"{Common.Troops[iUp1]}\")");
                */
                if(iUp1 != 0 && iUp2 != 0)
                    aUpList.Add(new Upgrade2(t, iUp1, iUp2));
                else if (iUp1 != 0 && iUp2 == 0)
                    aUpList.Add(new Upgrade(t, iUp1));

                string strItemList = "";
                for (int i = 0; i < 64; i++)
                {
                    int iItem = fTroops.GetInt();
                    fTroops.GetInt(); //skip 0
                    if (-1 == iItem)
                        continue;
                    strItemList += iItem < Common.Items.Length ? $"itm_{Common.Items[iItem]}," : $"{iItem},";
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

                var strKnow = new StringBuilder("");
                for (int x = 0; x < 6; x++)
                {
                    DWORD dword = fTroops.GetDWord();
                    if (dword == 0)
                        continue;
                    for (int q = 0; q < 8; q++)
                    {
                        DWORD dwKnow = 0xF & (dword >> (q * 4));
                        /*if (dwKnow != 0 && dwKnow <= 8)
                            strKnow = strKnow + String.Format("knows_{0}_{1}|", Common.Skills[x * 8 + q], dwKnow);
                        else*/
                        if (dwKnow != 0 && (x << 3) + q < Common.Skills.Length)
                            strKnow.Append($"knows_{Common.Skills[(x << 3) + q]}_{dwKnow}|");
                    }
                }
                if (strKnow.Length == 0) strKnow.Append('0'); else strKnow.Length--;
                //strKnow = strKnow.Length == 0 ? "0" : strKnow.Remove(strKnow.Length - 1, 1);
                fSource.Write(" {0},", strKnow);

                string strFase =
                    $"0x{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}, 0x{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}{fTroops.GetUInt64():x16}";
                if (Common.SelectedMode == Mode.Caribbean) fTroops.GetWord();
                fSource.WriteLine("{0}],", strFase);
            }

            fSource.WriteLine("]");
            foreach (var t in aUpList.Select(up => up.ToString()).Where(t => !String.IsNullOrEmpty(t)))
            {
                fSource.WriteLine(t);
            }
            fSource.Close();
            fTroops.Close();
        }
    }
}
