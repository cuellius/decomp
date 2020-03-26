using System;
using System.Globalization;
using System.IO;
using System.Text;
using DWORD64 = System.UInt64;
using DWORD = System.UInt32;
using WORD = System.UInt16;

namespace Decomp.Core
{
    public static class PartyTemplates
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "party_templates.txt"))) return Array.Empty<string>();

            var fId = new Win32FileReader(Path.Combine(Common.InputPath, "party_templates.txt"));
            fId.ReadLine();
            var n = Convert.ToInt32(fId.ReadLine(), CultureInfo.GetCultureInfo("en-US"));
            var aPartyTemplates = new string[n];
            for (int i = 0; i < n; i++)
            {
                var str = fId.ReadLine();
                if (str != null)
                    aPartyTemplates[i] = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Remove(0, 3);
            }
            fId.Close();

            return aPartyTemplates;
        }

        public static string DecompileFlags(DWORD64 dwFlag)
        {
            var sbFlag = new StringBuilder(2048);
            var wIcon = (WORD)(dwFlag & 0xFF);
            var wCarriesGoods = (WORD)((dwFlag & 0x00FF000000000000) >> 48);
            var wCarriesGold = (WORD)((dwFlag & 0xFF00000000000000) >> 56);

            if (wIcon != 0) sbFlag.Append(wIcon < Common.MapIcons.Count ? "icon_" + Common.MapIcons[wIcon] + "|" : Convert.ToString(wIcon, CultureInfo.GetCultureInfo("en-US")) + "|"); 
            if (wCarriesGoods != 0) sbFlag.Append("carries_goods(" + wCarriesGoods + ")|");
            if (wCarriesGold != 0) sbFlag.Append("carries_gold(" + wCarriesGold + ")|");

            string[] strFlags = { "pf_disabled", "pf_is_ship", "pf_is_static", "pf_label_medium", "pf_label_large",
			"pf_always_visible", "pf_default_behavior", "pf_auto_remove_in_town", "pf_quest_party", "pf_no_label", "pf_limit_members",
			"pf_hide_defenders", "pf_show_faction", "pf_dont_attack_civilians", "pf_civilian" };
            DWORD[] dwFlags = { 0x00000100, 0x00000200, 0x00000400, 0x00001000, 0x00002000, 0x00004000, 0x00010000,
			0x00020000, 0x00040000, 0x00080000, 0x00100000, 0x00200000, 0x00400000, 0x02000000, 0x04000000 };
            for (int i = 0; i < dwFlags.Length; i++)
            {
                if (((DWORD) dwFlag & dwFlags[i]) == 0) continue;
                sbFlag.Append(strFlags[i]);
                sbFlag.Append('|');
            }

            if (sbFlag.Length == 0)
                sbFlag.Append('0');
            else
                sbFlag.Length--;

            return sbFlag.ToString();
        }

        public static string DecompilePersonality(DWORD dwPersonality)
        {
            switch (dwPersonality)
            {
                case 0x89:
                    return "soldier_personality";
                case 0x07:
                    return "merchant_personality";
                case 0x0B:
                    return "escorted_merchant_personality";
                case 0x138:
                    return "bandit_personality";
                default:
                    var sbPersonality = new StringBuilder((dwPersonality & 0x100) != 0 ? "banditness|" : "", 64);
                    
                    var wCourage = (WORD)(dwPersonality & 0xF);
                    var wAggressiveness = (WORD)((dwPersonality & 0xF0) >> 4);

                    if (wCourage >= 4 && wCourage <= 15) sbPersonality.Append($"courage_{wCourage}|"); 
                    if (wAggressiveness > 0 && wAggressiveness <= 15) sbPersonality.Append($"aggressiveness_{wAggressiveness}|");

                    if (sbPersonality.Length == 0)
                        sbPersonality.Append('0');
                    else
                        sbPersonality.Length--;
                    
                    return sbPersonality.ToString();
            }
        }

        public static void Decompile()
        {
            var fTemplates = new Text(Path.Combine(Common.InputPath, "party_templates.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, @"module_party_templates.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.PartyTemplates);
            fTemplates.GetString();
            var iTemplates = fTemplates.GetInt();
            for (int i = 0; i < iTemplates; i++)
            {
                fSource.Write("  (\"{0}\", \"{1}\"", fTemplates.GetWord().Remove(0, 3), fTemplates.GetWord());

                var dwFlag = fTemplates.GetUInt64();
                fSource.Write(", {0}, {1}", DecompileFlags(dwFlag), fTemplates.GetInt());

                var iFaction = fTemplates.GetInt();
                if (iFaction >= 0 && iFaction < Common.Factions.Count)
                    fSource.Write(", fac_{0}", Common.Factions[iFaction]);
                else
                    fSource.Write(", {0}", iFaction);

                var dwPersonality = fTemplates.GetUInt();
                fSource.Write(", {0}, [", DecompilePersonality(dwPersonality));

                
                var sbTroopList = new StringBuilder(1024);
                for (int iStack = 0; iStack < 6; iStack++)
                {
                    var iTroop = fTemplates.GetInt();
                    if (-1 == iTroop) continue;
                    var iMinTroops = fTemplates.GetInt();
                    var iMaxTroops = fTemplates.GetInt();
                    var dwMemberFlag = fTemplates.GetDWord();
                    sbTroopList.Append($"({(iTroop < Common.Troops.Count ? "trp_" + Common.Troops[iTroop] : iTroop.ToString(CultureInfo.GetCultureInfo("en-US")))}, {iMinTroops}, {iMaxTroops}{(dwMemberFlag == 1 ? ", pmf_is_prisoner" : "")}),");
                }
                if (sbTroopList.Length != 0) sbTroopList.Length--;
                fSource.WriteLine("{0}]),", sbTroopList);
            }
            fSource.Write("]");
            fSource.Close();
            fTemplates.Close();

            Common.GenerateId("ID_party_templates.py", Common.PTemps, "pt");
        }
    }
}
