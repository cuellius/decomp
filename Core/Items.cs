using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BYTE = System.Byte;
using WORD = System.UInt16;
using DWORD64 = System.UInt64;

namespace Decomp.Core
{
    public static class Items
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "item_kinds1.txt"))) return Array.Empty<string>();

            var fId = new Text(Path.Combine(Common.InputPath, "item_kinds1.txt"));
            fId.GetString();
            var n = Convert.ToInt32(fId.GetString(), CultureInfo.GetCultureInfo("en-US"));
            var aItems = new string[n];
            for (int i = 0; i < n; i++)
            {
                var strId = fId.GetWord();
                aItems[i] = strId.Remove(0, 4);
                fId.GetWord();
                fId.GetWord();

                var iMeshes = fId.GetInt();

                for (int m = 0; m < iMeshes; m++)
                {
                    fId.GetWord();
                    fId.GetWord();
                }

                for (int v = 0; v < 17; v++) fId.GetWord();

                var iFactions = fId.GetInt();
                for (int j = 0; j < iFactions; j++) fId.GetInt();

                var iTriggers = fId.GetInt();
                for (int t = 0; t < iTriggers; t++)
                {
                    fId.GetWord();
                    var iRecords = fId.GetInt();
                    for (int r = 0; r < iRecords; r++)
                    {
                        fId.GetWord();
                        var iParams = fId.GetInt();
                        for (int p = 0; p < iParams; p++) fId.GetWord();
                    }
                }
            }
            fId.Close();

            return aItems;
        }

        public static string DecompileModifier(uint t)
        {
            if (t == 0xFFFFFFFF) return "-1";

            string[] strImods = { "imod_plain", "imod_cracked", "imod_rusty", "imod_bent", "imod_chipped", "imod_battered", "imod_poor", "imod_crude", "imod_old",
                "imod_cheap", "imod_fine", "imod_well_made", "imod_sharp", "imod_balanced", "imod_tempered", "imod_deadly", "imod_exquisite", "imod_masterwork",
                "imod_heavy", "imod_strong", "imod_powerful", "imod_tattered", "imod_ragged", "imod_rough", "imod_sturdy", "imod_thick", "imod_hardened",
                "imod_reinforced", "imod_superb", "imod_lordly", "imod_lame", "imod_swaybacked", "imod_stubborn", "imod_timid", "imod_meek", "imod_spirited",
                "imod_champion", "imod_fresh", "imod_day_old", "imod_two_day_old", "imod_smelling", "imod_rotten", "imod_large_bag" };

            return t >= strImods.Length ? t.ToString(CultureInfo.GetCultureInfo(1033)) : strImods[t];
        }

        public static string DecompileType(ulong t)
        {
            string[] strItemTypes = { "itp_type_zero", "itp_type_horse", "itp_type_one_handed_wpn", "itp_type_two_handed_wpn", "itp_type_polearm",
                "itp_type_arrows", "itp_type_bolts", "itp_type_shield", "itp_type_bow", "itp_type_crossbow", "itp_type_thrown", "itp_type_goods",
                "itp_type_head_armor", "itp_type_body_armor", "itp_type_foot_armor", "itp_type_hand_armor", "itp_type_pistol", "itp_type_musket",
                "itp_type_bullets", "itp_type_animal", "itp_type_book" };
            return t >= (ulong)strItemTypes.Length ? t.ToString(CultureInfo.GetCultureInfo(1033)) : strItemTypes[t];
        }

        // ReSharper disable InconsistentNaming
        private const BYTE HORSE_TYPE = 0x01;
        private const BYTE GOODS_TYPE = 0x0B;
        private const BYTE BOW_TYPE = 0x08;
        private const BYTE CROSSBOW_TYPE = 0x09;
        private const BYTE PISTOL_TYPE = 0x10;
        private const BYTE MUSKET_TYPE = 0x11;
        private const BYTE SHIELD_TYPE = 0x07;
        private const BYTE HEAD_ARMOR_TYPE = 0x0C;
        private const BYTE BODY_ARMOR_TYPE = 0x0D;
        private const BYTE FOOT_ARMOR_TYPE = 0x0E;
        private const BYTE HAND_ARMOR_TYPE = 0x0F;
        // ReSharper restore InconsistentNaming
        
        public static string DecompileImodBits(DWORD64 dwImodBit)
        {
            var sbFlag = new StringBuilder(2048);
		    string[] strImodConstants = { "imodbits_horse_basic", "imodbits_cloth", "imodbits_armor", "imodbits_plate", "imodbits_polearm",
			"imodbits_shield", "imodbits_sword", "imodbits_sword_high", "imodbits_axe", "imodbits_pick", "imodbits_bow", "imodbits_crossbow",
			"imodbits_missile", "imodbits_thrown", "imodbits_thrown_minus_heavy", "imodbits_horse_good", "imodbits_good", "imodbits_bad" };
		    DWORD64[] dwImodConstans = { 41876193280, 123731968, 704643236, 704643238, 8202, 167772194, 24596, 155668, 262164, 270356, 655370,
			131082, 4398046511112, 4398046781448, 4398046519304, 34360000512, 251658240, 6291486 };
            string[] strImodBits = { "imodbit_plain", "imodbit_cracked", "imodbit_rusty", "imodbit_bent", "imodbit_chipped", "imodbit_battered", "imodbit_poor", "imodbit_crude", "imodbit_old",
			"imodbit_cheap", "imodbit_fine", "imodbit_well_made", "imodbit_sharp", "imodbit_balanced", "imodbit_tempered", "imodbit_deadly", "imodbit_exquisite", "imodbit_masterwork",
			"imodbit_heavy", "imodbit_strong", "imodbit_powerful", "imodbit_tattered", "imodbit_ragged", "imodbit_rough", "imodbit_sturdy", "imodbit_thick", "imodbit_hardened",
			"imodbit_reinforced", "imodbit_superb", "imodbit_lordly", "imodbit_lame", "imodbit_swaybacked", "imodbit_stubborn", "imodbit_timid", "imodbit_meek", "imodbit_spirited",
			"imodbit_champion", "imodbit_fresh", "imodbit_day_old", "imodbit_two_day_old", "imodbit_smelling", "imodbit_rotten", "imodbit_large_bag" };
            DWORD64[] dwImodBits = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576,
			2097152, 4194304, 8388608, 16777216, 33554432, 67108864, 134217728, 268435456, 536870912, 1073741824, 2147483648, 4294967296, 8589934592,
			17179869184, 34359738368, 68719476736, 137438953472, 274877906944, 549755813888, 1099511627776, 2199023255552, 4398046511104 };

            for (int i = 0; i < dwImodConstans.Length; i++)
            {
                DWORD64 temp = dwImodBit & dwImodConstans[i];
                if (temp - dwImodConstans[i] != 0) continue;
                sbFlag.Append(strImodConstants[i]);
                sbFlag.Append('|');
                dwImodBit ^= dwImodConstans[i];
                break;
            }

            if (dwImodBit != 0)
                for (int i = 0; i < dwImodBits.Length; i++)
                {
                    if ((dwImodBit & dwImodBits[i]) == 0) continue;
                    sbFlag.Append(strImodBits[i]);
                    sbFlag.Append('|');
                }

            if (sbFlag.Length == 0)
                sbFlag.Append("imodbits_none");
            else
                sbFlag.Length--;
            
            return sbFlag.ToString();
        }

        public static string DecompileMeshesImodBits(DWORD64 dwMeshBits)
        {
            var sbFlag = new StringBuilder(2048);
            DWORD64 dwMeshExtraBits = (dwMeshBits & 0xFF00000000000000) >> 56;
            switch (dwMeshExtraBits)
            {
                case 0x10:
                    sbFlag.Append("ixmesh_inventory|");
                    break;
                case 0x20:
                    sbFlag.Append("ixmesh_flying_ammo|");
                    break;
                case 0x30:
                    sbFlag.Append("ixmesh_carry|");
                    break;
            }
            DWORD64 dwMeshImodBits = dwMeshBits & 0x00FFFFFFFFFFFFFF;
            string[] strImodBits = { "imodbit_plain", "imodbit_cracked", "imodbit_rusty", "imodbit_bent", "imodbit_chipped", "imodbit_battered", "imodbit_poor", "imodbit_crude", "imodbit_old",
			"imodbit_cheap", "imodbit_fine", "imodbit_well_made", "imodbit_sharp", "imodbit_balanced", "imodbit_tempered", "imodbit_deadly", "imodbit_exquisite", "imodbit_masterwork",
			"imodbit_heavy", "imodbit_strong", "imodbit_powerful", "imodbit_tattered", "imodbit_ragged", "imodbit_rough", "imodbit_sturdy", "imodbit_thick", "imodbit_hardened",
			"imodbit_reinforced", "imodbit_superb", "imodbit_lordly", "imodbit_lame", "imodbit_swaybacked", "imodbit_stubborn", "imodbit_timid", "imodbit_meek", "imodbit_spirited",
			"imodbit_champion", "imodbit_fresh", "imodbit_day_old", "imodbit_two_day_old", "imodbit_smelling", "imodbit_rotten", "imodbit_large_bag" };
            DWORD64[] dwImodBits = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576,
			2097152, 4194304, 8388608, 16777216, 33554432, 67108864, 134217728, 268435456, 536870912, 1073741824, 2147483648, 4294967296, 8589934592,
			17179869184, 34359738368, 68719476736, 137438953472, 274877906944, 549755813888, 1099511627776, 2199023255552, 4398046511104 };

            for (int i = 0; i < dwImodBits.Length; i++)
            {
                if ((dwMeshImodBits & dwImodBits[i]) == 0) continue;
                sbFlag.Append(strImodBits[i]);
                sbFlag.Append('|');
            }

            if (sbFlag.Length == 0)
                sbFlag.Append('0');
            else
                sbFlag.Length--;

            return sbFlag.ToString();
        }

        public static string DecompileFlags(DWORD64 dwFlag)
        {
            return DecompileFlags(dwFlag, out var _);
        }

        public static string DecompileFlags(DWORD64 dwFlag, out BYTE bType)
        {
            //DWORD64 dwType = dwFlag & 0xFF;
            var sbFlag = new StringBuilder(256);
            bType = (BYTE)dwFlag;
            
            string[] strItemTypes = { "itp_type_zero", "itp_type_horse", "itp_type_one_handed_wpn", "itp_type_two_handed_wpn", "itp_type_polearm", 
            "itp_type_arrows", "itp_type_bolts", "itp_type_shield", "itp_type_bow", "itp_type_crossbow", "itp_type_thrown", "itp_type_goods", 
            "itp_type_head_armor", "itp_type_body_armor", "itp_type_foot_armor", "itp_type_hand_armor", "itp_type_pistol", "itp_type_musket", 
            "itp_type_bullets", "itp_type_animal", "itp_type_book" };
            
            string[] strItemTypeFlags = { "itp_unique", "itp_always_loot"," itp_no_parry", "itp_default_ammo", "itp_merchandise", "itp_wooden_attack",
            "itp_wooden_parry", "itp_food", "itp_cant_reload_on_horseback", "itp_two_handed", "itp_primary", "itp_secondary", "itp_covers_legs",
            "itp_doesnt_cover_hair", "itp_consumable", "itp_bonus_against_shield", "itp_penalty_with_shield", "itp_cant_use_on_horseback",
            "itp_next_item_as_melee", "itp_offset_lance", "itp_covers_head", "itp_crush_through", "itp_remove_item_on_use", "itp_unbalanced",
            "itp_covers_beard", "itp_no_pick_up_from_ground", "itp_can_knock_down", "itp_extra_penetration", "itp_has_bayonet", "itp_cant_reload_while_moving",
            "itp_ignore_gravity", "itp_ignore_friction", "itp_is_pike", "itp_offset_musket", "itp_no_blur", "itp_cant_reload_while_moving_mounted",
            "itp_has_upper_stab", "itp_offset_mortschlag", "itp_offset_flip" };
            DWORD64[] dwItemFlags = { 0x0000000000001000, 0x0000000000002000, 0x0000000000004000, 0x0000000000008000, 0x0000000000010000, 0x0000000000020000,
            0x0000000000040000, 0x0000000000080000, 0x0000000000100000, 0x0000000000200000, 0x0000000000400000, 0x0000000000800000, 0x0000000001000000,
            0x0000000001000000, 0x0000000002000000, 0x0000000004000000, 0x0000000008000000, 0x0000000010000000, 0x0000000020000000, 0x0000000040000000,
            0x0000000080000000, 0x0000000100000000, 0x0000000400000000, 0x0000000800000000, 0x0000001000000000, 0x0000002000000000, 0x0000004000000000,
            0x0000100000000000, 0x0000200000000000, 0x0000400000000000, 0x0000800000000000, 0x0001000000000000, 0x0002000000000000, 0x0004000000000000,
            0x0008000000000000, 0x0010000000000000, 0x0020000000000000, 0x1000000000000000, 0x4000000000000000 };

            DWORD64 dwCustomKillInfo = (dwFlag & 0x0700000000000000U) >> 56;
            if (dwCustomKillInfo != 0) sbFlag.Append("custom_kill_info(" + dwCustomKillInfo + ")|");                                 

            //uint dwItemType = uItemFlags & 0xFF;
            if (bType > 0 && bType < 0x15)
            {
                sbFlag.Append(strItemTypes[bType]);
                sbFlag.Append('|');
            }
            
            var wAttach = (WORD)(dwFlag & 0xF00);

            switch (wAttach)
            {
                case 0x0100:
                    sbFlag.Append("itp_force_attach_left_hand|");
                    break;
                case 0x0200:
                    sbFlag.Append("itp_force_attach_right_hand|");
                    break;
                case 0x0300:
                    sbFlag.Append("itp_force_attach_left_forearm|");
                    break;
                case 0x0F00:
                    sbFlag.Append("itp_attach_armature|");
                    break;
            }

            for (int i = 0; i < dwItemFlags.Length; i++)
            {
                if ((dwFlag & dwItemFlags[i]) == 0) continue;
                sbFlag.Append(strItemTypeFlags[i]);
                sbFlag.Append('|');
            }

            if (sbFlag.Length == 0)
                sbFlag.Append('0');
            else
                sbFlag.Length--;

            var strFlag = sbFlag.ToString();
            switch (bType)
            {
                case HEAD_ARMOR_TYPE: strFlag = strFlag.Replace("itp_next_item_as_melee", "itp_civilian").Replace("itp_offset_lance", "itp_fit_to_head").Replace("itp_couchable", "itp_covers_head").Replace("itp_can_penetrate_shield", "itp_doesnt_cover_hair"); break; //head armor
                case BODY_ARMOR_TYPE: strFlag = strFlag.Replace("itp_next_item_as_melee", "itp_civilian").Replace("itp_can_penetrate_shield", "itp_covers_legs").Replace("itp_offset_lance", "itp_show_body").Replace("itp_offset_mortschlag", "itp_covers_hands"); break; //body armor
                case FOOT_ARMOR_TYPE: strFlag = strFlag.Replace("itp_next_item_as_melee", "itp_civilian"); break; //legs armor
                case HAND_ARMOR_TYPE: strFlag = strFlag.Replace("itp_next_item_as_melee", "itp_civilian").Replace("itp_offset_lance", "itp_show_left_hand").Replace("itp_couchable", "itp_show_right_hand"); break; //hand armor
            }
            
            return strFlag;
        }
        
        public static string DecompileCapabilities(DWORD64 dwCapacity)
        {
            var sbCapacity = new StringBuilder(2048);

            /*string[] strItemCapsFlags = { "itcf_thrust_onehanded", "itcf_overswing_onehanded", "itcf_slashright_onehanded", "itcf_slashleft_onehanded", 
            "itcf_thrust_twohanded", "itcf_overswing_twohanded", "itcf_slashright_twohanded", "itcf_slashleft_twohanded", "itcf_thrust_polearm", 
            "itcf_overswing_polearm", "itcf_slashright_polearm", "itcf_slashleft_polearm", "itcf_shoot_bow", "itcf_shoot_javelin", "itcf_shoot_crossbow", 
            "itcf_throw_stone", "itcf_throw_knife", "itcf_throw_axe", "itcf_throw_javelin", "itcf_shoot_pistol", "itcf_shoot_musket", "itcf_horseback_thrust_onehanded", 
            "itcf_horseback_overswing_right_onehanded", "itcf_horseback_overswing_left_onehanded", "itcf_horseback_slashright_onehanded", "itcf_horseback_slashleft_onehanded", 
            "itcf_thrust_onehanded_lance", "itcf_thrust_onehanded_lance_horseback", "itcf_carry_sword_left_hip", "itcf_carry_axe_left_hip", "itcf_carry_dagger_front_left",
            "itcf_carry_dagger_front_right", "itcf_carry_quiver_front_right", "itcf_carry_quiver_back_right", "itcf_carry_quiver_right_vertical", "itcf_carry_quiver_back", 
            "itcf_carry_revolver_right", "itcf_carry_pistol_front_left", "itcf_carry_bowcase_left", "itcf_carry_mace_left_hip", "itcf_carry_axe_back", 
            "itcf_carry_sword_back", "itcf_carry_kite_shield", "itcf_carry_round_shield", "itcf_carry_buckler_left", "itcf_carry_crossbow_back", 
            "itcf_carry_bow_back", "itcf_carry_spear", "itcf_carry_board_shield", "itcf_carry_katana", "itcf_carry_wakizashi", "itcf_show_holster_when_drawn", 
            "itcf_reload_pistol", "itcf_reload_musket", "itcf_parry_forward_onehanded", "itcf_parry_up_onehanded", "itcf_parry_right_onehanded", 
            "itcf_parry_left_onehanded", "itcf_parry_forward_twohanded", "itcf_parry_up_twohanded", "itcf_parry_right_twohanded", "itcf_parry_left_twohanded", 
            "itcf_parry_forward_polearm", "itcf_parry_up_polearm", "itcf_parry_right_polearm", "itcf_parry_left_polearm", "itcf_horseback_slash_polearm", 
            "itcf_overswing_spear", "itcf_overswing_musket", "itcf_thrust_musket", "itcf_force_64_bits" };
            DWORD64[] dwItemCapsFlags = { 0x0000000000000001, 0x0000000000000002, 0x0000000000000004, 0x0000000000000008, 0x0000000000000010, 
            0x0000000000000020, 0x0000000000000040, 0x0000000000000080, 0x0000000000000100, 0x0000000000000200, 0x0000000000000400, 0x0000000000000800, 
            0x0000000000001000, 0x0000000000002000, 0x0000000000004000, 0x0000000000010000, 0x0000000000020000, 0x0000000000030000, 0x0000000000040000, 
            0x0000000000070000, 0x0000000000080000, 0x0000000000100000, 0x0000000000200000, 0x0000000000400000, 0x0000000000800000, 0x0000000001000000, 
            0x0000000004000000, 0x0000000008000000, 0x0000000010000000, 0x0000000020000000, 0x0000000030000000, 0x0000000040000000, 0x0000000050000000, 
            0x0000000060000000, 0x0000000070000000, 0x0000000080000000, 0x0000000090000000, 0x00000000a0000000, 0x00000000b0000000, 0x00000000c0000000, 
            0x0000000100000000, 0x0000000110000000, 0x0000000120000000, 0x0000000130000000, 0x0000000140000000, 0x0000000150000000, 0x0000000160000000, 
            0x0000000170000000, 0x0000000180000000, 0x0000000210000000, 0x0000000220000000, 0x0000000800000000, 0x0000007000000000, 0x0000008000000000, 
            0x0000010000000000, 0x0000020000000000, 0x0000040000000000, 0x0000080000000000, 0x0000100000000000, 0x0000200000000000, 0x0000400000000000, 
            0x0000800000000000, 0x0001000000000000, 0x0002000000000000, 0x0004000000000000, 0x0008000000000000, 0x0010000000000000, 0x0020000000000000, 
            0x0040000000000000, 0x0080000000000000, 0x8000000000000000 };

            for (int i = dwItemCapsFlags.Length - 1; i >= 0; i--)
            {
                if (dwCapacity >= dwItemCapsFlags[i])
                {
                    strCapacity = strCapacity + strItemCapsFlags[i] + "|";
                    dwCapacity -= dwItemCapsFlags[i];
                }
            }*/

            string[] strCapsShoot = { "itcf_shoot_bow", "itcf_shoot_javelin", "itcf_shoot_crossbow", "itcf_throw_stone", "itcf_throw_knife", "itcf_throw_axe", 
            "itcf_throw_javelin", "itcf_shoot_pistol", "itcf_shoot_musket" };
            DWORD64[] dwCapsShoot = { 0x0000000000001000, 0x0000000000002000, 0x0000000000004000, 0x0000000000010000, 0x0000000000020000, 0x0000000000030000, 
            0x0000000000040000, 0x0000000000070000, 0x0000000000080000 };
            const DWORD64 dwCapsShootMask = 0x00000000000ff000;
            DWORD64 dwShoot = dwCapacity & dwCapsShootMask;
            for (int i = 0; i < dwCapsShoot.Length; i++)
            {
                if (dwShoot != dwCapsShoot[i]) continue;
                sbCapacity.Append(strCapsShoot[i]);
                sbCapacity.Append('|');
                break;
            }

            string[] strCapsCarry = { "itcf_carry_sword_left_hip", "itcf_carry_axe_left_hip", "itcf_carry_dagger_front_left", "itcf_carry_dagger_front_right",
            "itcf_carry_quiver_front_right", "itcf_carry_quiver_back_right", "itcf_carry_quiver_right_vertical", "itcf_carry_quiver_back", 
            "itcf_carry_revolver_right", "itcf_carry_pistol_front_left", "itcf_carry_bowcase_left", "itcf_carry_mace_left_hip", "itcf_carry_axe_back", 
            "itcf_carry_sword_back", "itcf_carry_kite_shield", "itcf_carry_round_shield", "itcf_carry_buckler_left", "itcf_carry_crossbow_back", 
            "itcf_carry_bow_back", "itcf_carry_spear", "itcf_carry_board_shield", "itcf_carry_katana", "itcf_carry_wakizashi" };
            DWORD64[] dwCapsCarry = { 0x0000000010000000, 0x0000000020000000, 0x0000000030000000, 0x0000000040000000, 0x0000000050000000, 0x0000000060000000, 
            0x0000000070000000, 0x0000000080000000, 0x0000000090000000, 0x00000000a0000000, 0x00000000b0000000, 0x00000000c0000000, 0x0000000100000000, 
            0x0000000110000000, 0x0000000120000000, 0x0000000130000000, 0x0000000140000000, 0x0000000150000000, 0x0000000160000000, 0x0000000170000000, 
            0x0000000180000000, 0x0000000210000000, 0x0000000220000000 };
            const DWORD64 dwCapsCarryMask = 0x00000007f0000000;
            DWORD64 dwCarry = dwCapacity & dwCapsCarryMask;
            for (int i = 0; i < dwCapsCarry.Length; i++)
            {
                if (dwCarry != dwCapsCarry[i]) continue;
                sbCapacity.Append(strCapsCarry[i]);
                sbCapacity.Append('|');
                break;
            }

            const DWORD64 dwCapsReloadMask = 0x000000f000000000;
            DWORD64 dwReload = dwCapacity & dwCapsReloadMask;
            switch (dwReload)
            {
                case 0x0000007000000000:
                    sbCapacity.Append("itcf_reload_pistol|");
                    break;
                case 0x0000008000000000:
                    sbCapacity.Append("itcf_reload_musket|");
                    break;
            }

            string[] strItemCapsConstant = { "itc_longsword", "itc_scimitar", "itc_parry_onehanded", "itc_greatsword", "itc_bastardsword", "itc_nodachi", 
            "itc_morningstar", "itc_parry_two_handed", "itc_cut_two_handed", "itc_poleaxe", "itc_staff", "itc_cutting_spear", "itc_spear", "itc_pike", 
            "itc_guandao", "itc_parry_polearm", "itc_greatlance", "itc_dagger", "itc_cleaver", "itc_musket_melee" };
            DWORD64[] dwItemCapsConstant = { 9223388529554358287, 9223388529554358286, 9223388529529192448, 9223635919737716976, 9223635919670608127, 
            9223635919670608096, 9223635919670608110, 9223635919645442048, 9223372036879941856, 4222124650663680, 4222124851990272, 4222124851987200, 
            4222124851986688, 201326848, 8725724303200000, 4222124650659840, 201326848, 9223372036879941647, 9223372036879941646, 58265320179105984 };
            for (int i = 0; i < dwItemCapsConstant.Length; i++)
            {
                ulong temp = dwCapacity & dwItemCapsConstant[i];
                if (temp - dwItemCapsConstant[i] != 0) continue;
                sbCapacity.Append(strItemCapsConstant[i]);
                sbCapacity.Append('|');
                dwCapacity ^= dwItemCapsConstant[i];
            }

            string[] strItemCapsFlags = { "itcf_thrust_onehanded", "itcf_overswing_onehanded", "itcf_slashright_onehanded", "itcf_slashleft_onehanded", 
            "itcf_thrust_twohanded", "itcf_overswing_twohanded", "itcf_slashright_twohanded", "itcf_slashleft_twohanded", "itcf_thrust_polearm", 
            "itcf_overswing_polearm", "itcf_slashright_polearm", "itcf_slashleft_polearm", "itcf_horseback_thrust_onehanded", 
            "itcf_horseback_overswing_right_onehanded", "itcf_horseback_overswing_left_onehanded", "itcf_horseback_slashright_onehanded", 
            "itcf_horseback_slashleft_onehanded", "itcf_thrust_onehanded_lance", "itcf_thrust_onehanded_lance_horseback", "itcf_show_holster_when_drawn", 
            "itcf_parry_forward_onehanded", "itcf_parry_up_onehanded", "itcf_parry_right_onehanded", "itcf_parry_left_onehanded", 
            "itcf_parry_forward_twohanded", "itcf_parry_up_twohanded", "itcf_parry_right_twohanded", "itcf_parry_left_twohanded", 
            "itcf_parry_forward_polearm", "itcf_parry_up_polearm", "itcf_parry_right_polearm", "itcf_parry_left_polearm", "itcf_horseback_slash_polearm", 
            "itcf_overswing_spear", "itcf_overswing_musket", "itcf_thrust_musket", "itcf_force_64_bits" };
            DWORD64[] dwItemCapsFlags = { 0x0000000000000001, 0x0000000000000002, 0x0000000000000004, 0x0000000000000008, 0x0000000000000010, 
            0x0000000000000020, 0x0000000000000040, 0x0000000000000080, 0x0000000000000100, 0x0000000000000200, 0x0000000000000400, 0x0000000000000800, 
            0x0000000000100000, 0x0000000000200000, 0x0000000000400000, 0x0000000000800000, 0x0000000001000000, 0x0000000004000000, 0x0000000008000000, 
            0x0000000800000000, 0x0000010000000000, 0x0000020000000000, 0x0000040000000000, 0x0000080000000000, 0x0000100000000000, 0x0000200000000000,
            0x0000400000000000, 0x0000800000000000, 0x0001000000000000, 0x0002000000000000, 0x0004000000000000, 0x0008000000000000, 0x0010000000000000,
            0x0020000000000000, 0x0040000000000000, 0x0080000000000000, 0x8000000000000000 };
            for (int i = 0; i < dwItemCapsFlags.Length; i++)
            {
                if ((dwCapacity & dwItemCapsFlags[i]) == 0) continue;
                sbCapacity.Append(strItemCapsFlags[i]);
                sbCapacity.Append('|');
            }

            if (sbCapacity.Length == 0)
                sbCapacity.Append('0');
            else
                sbCapacity.Length--;

            return sbCapacity.ToString();
        }

        public static void Decompile()
        {
            var fItems = new Text(Path.Combine(Common.InputPath, "item_kinds1.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_items.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Items);
            fItems.GetString();
            int iItems = fItems.GetInt();
            for (int i = 0; i < iItems; i++)
            {
                string strItemId = fItems.GetWord().Remove(0, 4);
                fSource.Write("  [\"{0}\"", strItemId);
                fItems.GetWord(); // skip second name

                string strItemName = fItems.GetWord();
                fSource.Write(",\"{0}\", [", strItemName);

                int iMeshes = fItems.GetInt();

                var sbMeshes = new StringBuilder(2048);
                for (int m = 0; m < iMeshes; m++)
                {
                    string strMeshName = fItems.GetWord();
                    DWORD64 dwMeshBits = fItems.GetUInt64();
                    sbMeshes.Append($"(\"{strMeshName}\", {DecompileMeshesImodBits(dwMeshBits)}),");
                }
                if(sbMeshes.Length > 0) sbMeshes.Length--;

                fSource.Write("{0}]", sbMeshes);

                DWORD64 dwItemFlags = fItems.GetUInt64();
                ulong lItemCaps = fItems.GetUInt64();

                fSource.Write(", {0}, {1},", DecompileFlags(dwItemFlags, out byte bType), DecompileCapabilities(lItemCaps));
                int iCost = fItems.GetInt();

                //items.GetWord();
                DWORD64 dwImodBits = fItems.GetUInt64();

                var sbItemStats = new StringBuilder("weight(" + fItems.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US")) + ")", 1024);
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
                    else if (bType == SHIELD_TYPE && strState == "weapon_length")
                        strState = "shield_width";
                    else if (bType == SHIELD_TYPE && strState == "shoot_speed")
                        strState = "shield_height";

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
                                sbItemStats.Append($"|horse_charge({iDamage})");
                            else
                                sbItemStats.Append($"|{strState}({iDamage}, {strDamageType})");
                        }
                        else
                            sbItemStats.Append($"|{strState}({iValue})");
                    }
                }
                fSource.Write("{0}, {1}, {2}", iCost, sbItemStats, DecompileImodBits(dwImodBits));

                int iFactions = fItems.GetInt();
                var factionsList = new int[iFactions];
                for (int f = 0; f < iFactions; f++) factionsList[f] = fItems.GetInt();
                string strFactionList = String.Join(",", factionsList.Select(f => f >= 0 && f < Common.Factions.Count ? "fac_" + Common.Factions[f] : Convert.ToString(f, CultureInfo.GetCultureInfo("en-US"))));

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

                if (iFactions != 0)
                    fSource.WriteLine(", [{0}]],", strFactionList);
                else 
                    fSource.WriteLine("],");

            }
            fSource.Write("]");
            fSource.Close();
            fItems.Close();

            Common.GenerateId("ID_items.py", Common.Items, "itm");
        }
    }
}
