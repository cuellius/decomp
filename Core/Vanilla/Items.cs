using System;
using System.Globalization;
using BYTE = System.Byte;
using WORD = System.UInt16;
using DWORD64 = System.UInt64;

namespace Decomp.Core.Vanilla
{
    public static class Items
    {
        private const BYTE HORSE_TYPE = 0x1;
        private const BYTE GOODS_TYPE = 0xb;
        private const BYTE BOW_TYPE = 0x8;
        private const BYTE CROSSBOW_TYPE = 0x9;
        private const BYTE PISTOL_TYPE = 0x10;
        private const BYTE MUSKET_TYPE = 0x11;

        public static string[] GetIdFromFile(string strFileName)
        {
            var fID = new Text(strFileName);
            fID.GetString();
            int n = Convert.ToInt32(fID.GetString());
            var aItems = new string[n];
            for (int i = 0; i < n; i++)
            {
                string strID = fID.GetWord();
                aItems[i] = strID.Remove(0, 4);
                fID.GetWord();
                fID.GetWord();

                int iMeshes = fID.GetInt();

                for (int m = 0; m < iMeshes; m++)
                {
                    fID.GetWord();
                    fID.GetWord();
                }

                for (int v = 0; v < 17; v++)
                {
                    fID.GetWord();
                }

                //int iFactions = fID.GetInt();
                //for (int j = 0; j < iFactions; j++)
                //    fID.GetInt();

                int iTriggers = fID.GetInt();
                for (int t = 0; t < iTriggers; t++)
                {
                    fID.GetWord();
                    int iRecords = fID.GetInt();
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
            fID.Close();

            return aItems;
        }

        public static string DecompileFlags(DWORD64 dwFlag, ref BYTE bType)
        {
            string strFlag = "";
            DWORD64 dwType = dwFlag & 0xFF;

            bType = (BYTE)dwType;

            string[] strItemTypes = { "itp_type_zero", "itp_type_horse", "itp_type_one_handed_wpn", "itp_type_two_handed_wpn", "itp_type_polearm", 
            "itp_type_arrows", "itp_type_bolts", "itp_type_shield", "itp_type_bow", "itp_type_crossbow", "itp_type_thrown", "itp_type_goods", 
            "itp_type_head_armor", "itp_type_body_armor", "itp_type_foot_armor", "itp_type_hand_armor", "itp_type_pistol", "itp_type_musket", 
            "itp_type_bullets", "itp_type_animal", "itp_type_book" };

            string[] strItemTypeFlags = { "itp_unique", "itp_always_loot", "itp_no_parry", "itp_spear", "itp_merchandise", "itp_wooden_attack", 
            "itp_wooden_parry", "itp_food", "itp_cant_reload_on_horseback", "itp_two_handed", "itp_primary", "itp_secondary", "itp_covers_legs", 
            "itp_doesnt_cover_hair", "itp_consumable", "itp_bonus_against_shield", "itp_penalty_with_shield", "itp_cant_use_on_horseback", 
            "itp_civilian", "itp_fit_to_head", "itp_covers_head" };
            DWORD64[] dwItemFlags = { 0x00001000, 0x00002000, 0x00004000, 0x00008000, 0x00010000, 0x00020000, 0x00040000, 0x00080000, 0x00100000, 
            0x00200000, 0x00400000, 0x00800000, 0x01000000, 0x01000000, 0x02000000, 0x04000000, 0x08000000, 0x10000000, 0x20000000, 0x40000000, 0x80000000 };

            //uint dwItemType = uItemFlags & 0xFF;
            if ((dwType > 0) && (dwType < 0x15))
                strFlag = strItemTypes[dwType] + "|";

            var wAttach = (WORD)(dwFlag & 0xF00);

            switch (wAttach)
            {
                case 0x0100:
                    strFlag += "itp_force_attach_left_hand|";
                    break;
                case 0x0200:
                    strFlag += "itp_force_attach_right_hand|";
                    break;
                case 0x0300:
                    strFlag += "itp_force_attach_left_forearm|";
                    break;
                case 0x0F00:
                    strFlag += "itp_attach_armature|";
                    break;
            }

            for (int i = 0; i < dwItemFlags.Length; i++)
            {
                if ((dwFlag & dwItemFlags[i]) != 0)
                {
                    strFlag += strItemTypeFlags[i] + "|";
                }
            }

            strFlag = strFlag == "" ? "0" : strFlag.Remove(strFlag.Length - 1, 1);

            return strFlag;
        }

        public static void Decompile()
        {
            var fItems = new Text(Common.InputPath + @"\item_kinds1.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_items.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Items);
            fItems.GetString();
            int iItems = fItems.GetInt();

            for (int i = 0; i < iItems; i++)
            {
                string strItemID = fItems.GetWord().Remove(0, 4);
                fSource.Write("  [\"{0}\"", strItemID);
                string strItemName = fItems.GetWord();

                fItems.GetWord(); // skip second name
                fSource.Write(",\"{0}\", [", strItemName);

                int iMeshes = fItems.GetInt();

                string strMeshes = "";
                for (int m = 0; m < iMeshes; m++)
                {
                    string strMeshName = fItems.GetWord();
                    DWORD64 dwMeshBits = fItems.GetUInt64();
                    strMeshes = strMeshes + $"(\"{strMeshName}\", {Core.Items.DecompileMeshesImodBits(dwMeshBits)}),";
                }
                if (strMeshes.Length > 0)
                    strMeshes = strMeshes.Remove(strMeshes.Length - 1, 1);

                fSource.Write("{0}]", strMeshes);

                DWORD64 dwItemFlags = fItems.GetUInt64();
                ulong lItemCaps = fItems.GetUInt64();

                BYTE bType = 0;

                fSource.Write(", {0}, {1},", DecompileFlags(dwItemFlags, ref bType), Core.Items.DecompileCapabilities(lItemCaps));

                int iCost = fItems.GetInt();
                DWORD64 dwImodBits = fItems.GetUInt64();

                string strItemStats = "weight(" + fItems.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US")) + ")";
                string[] strStats = { "abundance", "head_armor", "body_armor", "leg_armor", "difficulty", "hit_points",
			    "spd_rtng", "shoot_speed", "weapon_length", "max_ammo", "thrust_damage", "swing_damage" };
                for (int v = 0; v < 12; v++)
                {
                    int iValue = fItems.GetInt();

                    string strState = strStats[v];

                    if (bType == HORSE_TYPE && strState == "shoot_speed")
                        strState = "horse_speed";
                    else if (bType == HORSE_TYPE && strState == "spd_rtng")
                        strState = "horse_maneuver";
                    else if (bType == GOODS_TYPE && strState == "head_armor")
                        strState = "food_quality";
                    else if ((bType == BOW_TYPE || bType == CROSSBOW_TYPE || bType == MUSKET_TYPE || bType == PISTOL_TYPE) && strState == "leg_armor")
                        strState = "accuracy";

                    if (iValue != 0)
                    {
                        if (v >= 10)
                        {
                            int iDamage = iValue & 0xFF;
                            int iDamageType = (iValue - iDamage) >> 8;
                            string strDamageType = "";
                            switch (iDamageType)
                            {
                                case 0:
                                    strDamageType = "cut";
                                    break;
                                case 1:
                                    strDamageType = "pierce";
                                    break;
                                case 2:
                                    strDamageType = "blunt";
                                    break;
                            }
                            if (bType == HORSE_TYPE && strState == "thrust_damage" && iDamageType == 0)
                                strItemStats = strItemStats + $"|horse_charge({iDamage})";
                            else
                                strItemStats = strItemStats + $"|{strState}({iDamage}, {strDamageType})";
                        }
                        else
                            strItemStats = strItemStats + $"|{strState}({iValue})";
                    }
                }
                fSource.Write("{0}, {1}, {2}", iCost, strItemStats, Core.Items.DecompileImodBits(dwImodBits));

                int iTriggers = fItems.GetInt();
                if (iTriggers != 0)
                {
                    fSource.Write(", [\r\n    ");
                    for (int t = 0; t < iTriggers; t++)
                    {
                        double dInterval = fItems.GetDouble();
                        fSource.WriteLine("({0}, [", Common.GetTriggerParam(dInterval));

                        int iRecords = fItems.GetInt();
                        //memcpy(indention, "      ", 7);
                        Common.PrintStatement(ref fItems, ref fSource, iRecords, "      ");

                        fSource.Write("    ]),\r\n   ");
                    }
                    fSource.Write("]");
                }
                else
                    fSource.Write(", []");

                fSource.WriteLine("],");
            }

            fSource.Write("]");
            fSource.Close();
            fItems.Close();

            Common.GenerateId("ID_items.py", Common.Items, "itm");
        }
    }
}
