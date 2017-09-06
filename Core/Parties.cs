using System;
using System.Globalization;
using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Parties
    {
        public static string GetAiBehavior(ulong t)
        {
            string[] aiBehaviours = { "ai_bhvr_hold", "ai_bhvr_travel_to_party", "ai_bhvr_patrol_location", "ai_bhvr_patrol_party",
                "ai_bhvr_attack_party", "ai_bhvr_avoid_party", "ai_bhvr_travel_to_point", "ai_bhvr_negotiate_party", "ai_bhvr_in_town",
                "ai_bhvr_travel_to_ship", "ai_bhvr_escort_party", "ai_bhvr_driven_by_party" };
            return t < (ulong)aiBehaviours.Length ? aiBehaviours[t] : t.ToString(CultureInfo.GetCultureInfo("en-US"));
        }

        public static string[] Initialize()
        {
            var fID = new Text(Common.InputPath + @"\parties.txt");
            fID.GetString();
            int n = fID.GetInt();
            fID.GetInt();

            var idParties = new string[n];
            for (int i = 0; i < n; i++)
            {
                fID.GetWord(); fID.GetWord(); fID.GetWord();
                idParties[i] = fID.GetWord().Remove(0, 2);

                for (int j = 0; j < 17; j++)
                    fID.GetWord();

                int iRecords = fID.GetInt();

                for (int j = 0; j < iRecords; j++)
                {
                    fID.GetWord();
                    fID.GetWord();
                    fID.GetWord();
                    fID.GetWord();
                }

                fID.GetWord();
            }
            fID.Close();

            return idParties;
        }

        public static string DecompileFlags(DWORD dwFlag)
        {
            var sbFlag = new StringBuilder(512);

            DWORD dwIcon = dwFlag & 0xFF;
            if(dwIcon != 0)
                sbFlag.Append(dwIcon < Common.MapIcons.Length ? "icon_" + Common.MapIcons[dwIcon] + "|" : Convert.ToString(dwIcon) + "|");
            
            string[] strFlags = { "pf_town", "pf_castle", "pf_village", "pf_disabled", "pf_is_ship", "pf_is_static", "pf_label_medium", 
            "pf_label_large", "pf_always_visible", "pf_default_behavior", "pf_auto_remove_in_town", "pf_quest_party", "pf_no_label", "pf_limit_members", 
            "pf_hide_defenders", "pf_show_faction", "pf_dont_attack_civilians", "pf_civilian" };
            DWORD[] dwFlags = { 0x406400, 0x405400, 0x204400, 0x00000100, 0x00000200, 0x00000400, 0x00001000, 0x00002000, 0x00004000, 0x00010000, 
            0x00020000, 0x00040000, 0x00080000, 0x00100000, 0x00200000, 0x00400000, 0x02000000, 0x04000000 };
            for (int i = 0; i < dwFlags.Length; i++)
            {
                DWORD temp = dwFlag & dwFlags[i];
                if (temp - dwFlags[i] == 0)
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
            var fParties = new Text(Common.InputPath + @"\parties.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_parties.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Parties);
            fParties.GetString();
            int iParties = fParties.GetInt();
            fParties.GetInt();
            for (int i = 0; i < iParties; i++)
            {
                fParties.GetInt(); fParties.GetInt(); fParties.GetInt();
                fSource.Write(" (\"{0}\", \"{1}\", {2}", fParties.GetWord().Remove(0, 2), fParties.GetWord().Replace('_', ' '), DecompileFlags(fParties.GetDWord()));

                int iMenu = fParties.GetInt();
                fSource.Write(", {0}", iMenu == 0 ? "no_menu" : "mnu_" + Common.Menus[iMenu]);

                int iParty = fParties.GetInt();
                fSource.Write(", {0}", iParty == 0 ? "pt_none" : "pt_" + Common.PTemps[iParty]);

                int iFaction = fParties.GetInt();
                fSource.Write(", {0}", "fac_" + Common.Factions[iFaction]);

                int iPersonality = fParties.GetInt(); fParties.GetInt(); 
                fSource.Write(", {0}", iPersonality);

                int iAIbehaviour = fParties.GetInt(); fParties.GetInt(); 
                string[] strAIbehaviours = { "ai_bhvr_hold", "ai_bhvr_travel_to_party", "ai_bhvr_patrol_location", "ai_bhvr_patrol_party",
			    "ai_bhvr_attack_party", "ai_bhvr_avoid_party", "ai_bhvr_travel_to_point", "ai_bhvr_negotiate_party", "ai_bhvr_in_town",
			    "ai_bhvr_travel_to_ship", "ai_bhvr_escort_party", "ai_bhvr_driven_by_party" };
                fSource.Write(", {0}", iAIbehaviour <= 11 ? strAIbehaviours[iAIbehaviour] : iAIbehaviour.ToString(CultureInfo.GetCultureInfo("en-US")));

                int iAITargetParty = fParties.GetInt(); 
                fSource.Write(", {0}", iAITargetParty);

                double dX = fParties.GetDouble(), dY = fParties.GetDouble(); 
                fSource.Write(", ({0}, {1}), [", dX.ToString(CultureInfo.GetCultureInfo("en-US")), dY.ToString(CultureInfo.GetCultureInfo("en-US")));
                fParties.GetDouble(); fParties.GetDouble(); fParties.GetDouble(); fParties.GetDouble(); fParties.GetDouble();
                
                int iRecords = fParties.GetInt();
                for (int j = 0; j < iRecords; j++)
                {
                    int iTroop = fParties.GetInt();
                    int iNumTroops = fParties.GetInt(); fParties.GetInt();
                    int iFlag = fParties.GetInt();
                    fSource.Write("(trp_{0}, {1}, {2}){3}", Common.Troops[iTroop], iNumTroops, iFlag == 1 ? "pmf_is_prisoner" : "0", j == (iRecords - 1) ? "" : ",");
                }
                fSource.Write("]");
                double dAngle = fParties.GetDouble();
                if (Math.Abs(dAngle) > 0.0000001)
                {
                    fSource.Write(", {0}", (Math.Round(dAngle * (180 / Math.PI))).ToString(CultureInfo.GetCultureInfo("en-US")));
                }

                fSource.WriteLine("),");
            }
            fSource.Write("]");
            fSource.Close();
            fParties.Close();
        }
    }
}
