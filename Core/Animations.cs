using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Animations
    {
        public static string[] Initialize()
        {
            if(!File.Exists(Path.Combine(Common.InputPath, "actions.txt"))) return new string[0];

            var fId = new Win32FileReader(Path.Combine(Common.InputPath, "actions.txt"));
            var n = Convert.ToInt32(fId.ReadLine());
            var aAnimations = new string[n];
            for (int i = 0; i < n; i++)
            {
                var animation = fId.ReadLine();
                if (animation == null) continue;

                aAnimations[i] = animation.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

                int j = Convert.ToInt32(animation.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[3]);
                while (j != 0)
                {
                    fId.ReadLine();
                    j--;
                }
            }
            fId.Close();

            return aAnimations;
        }

        public static string DecompileFlags(DWORD dwFlag)
        {
            var sbFlag = new StringBuilder(2048);
            var dwAnimFlagsLength = dwFlag & 0xFF000000;
            string[] strAnimFlags = { "acf_synch_with_horse", "acf_align_with_ground", "acf_enforce_lowerbody", "acf_enforce_rightside", "acf_enforce_all", 
			"acf_parallels_for_look_slope", "acf_lock_camera", "acf_displace_position", "acf_ignore_slope", "acf_thrust", "acf_right_cut",
			"acf_left_cut", "acf_overswing", "acf_rot_vertical_bow", "acf_rot_vertical_sword" };
            DWORD[] dwAnimFlags = { 0x00000001, 0x00000002, 0x00000100, 0x00000200, 0x00000400, 0x00001000, 0x00002000, 0x00004000, 0x00008000, 0x00010000,
			0x00020000, 0x00040000, 0x00080000, 0x00100000, 0x00200000 };
            if (dwAnimFlagsLength != 0)
            {
                dwAnimFlagsLength = dwAnimFlagsLength >> 24;
                dwFlag ^= 0xFF000000;
                sbFlag.AppendFormat("acf_anim_length({0})", dwAnimFlagsLength);
            }

            for (int f = 0; f < dwAnimFlags.Length; f++)
		    {
		        if ((dwFlag & dwAnimFlags[f]) == 0) continue;
		        if (sbFlag.Length != 0) sbFlag.Append('|');
		        sbFlag.Append(strAnimFlags[f]);
		    }

            if (sbFlag.Length == 0) sbFlag.Append("0");

            return sbFlag.ToString();
        }

        public static string DecompileMasterFlags(DWORD dwMasterAnimFlags)
        {
            var sbMasterAnimFlags = new StringBuilder(2048);

            string[] strPriorityMasterFlags = { "amf_priority_jump", "amf_priority_continue", "amf_priority_attack", "amf_priority_cancel",
			"amf_priority_defend", "amf_priority_defend_parry", "amf_priority_kick", "amf_priority_jump_end",
			"amf_priority_reload", "amf_priority_mount", "amf_priority_equip", "amf_priority_rear", "amf_priority_striked", "amf_priority_fall_from_horse", "amf_priority_die" };
            DWORD[] dwPriorityMasterFlags = { 2, 1, 10, 12, 14, 15, 33, 33, 60, 64, 70, 74, 80, 81, 95 };
		    DWORD dwPriority = dwMasterAnimFlags & 0xFF;

            for(int p = 0; p < dwPriorityMasterFlags.Length; p++)
		    {
		        if (dwPriority != dwPriorityMasterFlags[p]) continue;
		        sbMasterAnimFlags.Append(strPriorityMasterFlags[p]);
		        dwMasterAnimFlags ^= dwPriorityMasterFlags[p];
		        break;
		    }

            string[] strRiderMasterFlags = { "amf_rider_rot_bow", "amf_rider_rot_throw", "amf_rider_rot_crossbow", "amf_rider_rot_pistol", "amf_rider_rot_overswing",
			"amf_rider_rot_thrust", "amf_rider_rot_swing_right", "amf_rider_rot_swing_left", "amf_rider_rot_couched_lance", "amf_rider_rot_shield",
			"amf_rider_rot_defend" };
            DWORD[] dwRiderMasterFlags = { 0x00001000, 0x00002000, 0x00003000, 0x00004000, 0x00005000, 0x00006000, 0x00007000, 0x00008000, 0x00009000, 0x0000a000, 0x0000b000 };
		    var dwRider = dwMasterAnimFlags & 0xF000;

            for (int r = 0; r < dwRiderMasterFlags.Length; r++)
		    {
		        if (dwRider != dwRiderMasterFlags[r]) continue;
		        if(sbMasterAnimFlags.Length != 0) sbMasterAnimFlags.Append('|');
		        sbMasterAnimFlags.Append(strRiderMasterFlags[r]);
		        dwMasterAnimFlags ^= dwRiderMasterFlags[r];
		        break;
		    }  

            string[] strActionsMasterFlags = { "amf_start_instantly", "amf_use_cycle_period", "amf_use_weapon_speed", "amf_use_defend_speed", "amf_accurate_body",
			"amf_client_prediction", "amf_play", "amf_keep", "amf_restart", "amf_hide_weapon", "amf_client_owner_prediction", "amf_use_inertia", "amf_continue_to_next" };
		    DWORD[] dwActionsMasterFlags = { 0x00010000, 0x00100000, 0x00200000, 0x00400000, 0x00800000, 0x01000000, 0x02000000, 0x04000000, 0x08000000, 0x10000000, 0x20000000, 0x40000000, 0x80000000 };

            for (int f = 0; f < dwActionsMasterFlags.Length; f++)
		    {
		        if ((dwMasterAnimFlags & dwActionsMasterFlags[f]) == 0) continue;
		        if (sbMasterAnimFlags.Length != 0) sbMasterAnimFlags.Append('|');
		        sbMasterAnimFlags.Append(strActionsMasterFlags[f]);
		        dwMasterAnimFlags ^= dwActionsMasterFlags[f];
		    }
            
            if (sbMasterAnimFlags.Length == 0) sbMasterAnimFlags.Append('0');

            return sbMasterAnimFlags.ToString();
        }

        public static string DecompileSequenceFlags(DWORD dwFlag)
        {
            var sbFlag = new StringBuilder(2048);
            DWORD dwSequenceBlend = dwFlag & 0xFF;
            if (dwSequenceBlend != 0) sbFlag.AppendFormat("arf_blend_in_{0}", dwSequenceBlend - 1);

            string[] strAnimSequenceFlags = { "arf_make_walk_sound", "arf_make_custom_sound", "arf_two_handed_blade", "arf_lancer", "arf_stick_item_to_left_hand",
				"arf_cyclic", "arf_use_walk_progress", "arf_use_stand_progress", "arf_use_inv_walk_progress" };
            DWORD[] dwAnimSequenceFlags = { 0x00000100, 0x00000200, 0x01000000, 0x02000000, 0x04000000, 0x10000000, 0x20000000, 0x40000000, 0x80000000 };

            for (int f = 0; f < dwAnimSequenceFlags.Length; f++)
            {
                if ((dwFlag & dwAnimSequenceFlags[f]) == 0) continue;
                if (sbFlag.Length != 0) sbFlag.Append('|');
                sbFlag.Append(strAnimSequenceFlags[f]);
            }

            if (sbFlag.Length == 0) sbFlag.Append('0');

            return sbFlag.ToString();
        }

        public static float DecompileByte(byte bParam) => (float)Math.Round(bParam / 255.0, 2);

        public static string UnPack2f(DWORD dwPack)
        {
            float d1 = DecompileByte((byte)((dwPack & 0xFF00) >> 8)),
                  d2 = DecompileByte((byte)(dwPack & 0xFF));
            return
                $"pack2f({d1.ToString(CultureInfo.GetCultureInfo("en-US"))}, {d2.ToString(CultureInfo.GetCultureInfo("en-US"))})";
        }

        public static string UnPack4f(DWORD dwPack)
        {
            float d1 = DecompileByte((byte)((dwPack & 0xFF000000) >> 24)),
                  d2 = DecompileByte((byte)((dwPack & 0xFF0000) >> 16)),
                  d3 = DecompileByte((byte)((dwPack & 0xFF00) >> 8)),
                  d4 = DecompileByte((byte)(dwPack & 0xFF));
            return
                $"pack4f({d1.ToString(CultureInfo.GetCultureInfo("en-US"))}, {d2.ToString(CultureInfo.GetCultureInfo("en-US"))}, {d3.ToString(CultureInfo.GetCultureInfo("en-US"))}, {d4.ToString(CultureInfo.GetCultureInfo("en-US"))})";
        }

        public static string DecompilePack(DWORD dwPack)
        {
            if (dwPack == 0) return "0";
            return dwPack <= 0xFFFF ? UnPack2f(dwPack) : UnPack4f(dwPack);
        }

        public static void Decompile()
        {
            var fActions = new Text(Path.Combine(Common.InputPath, "actions.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_animations.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Animations);
            int iActions = fActions.GetInt();
            for (int a = 0; a < iActions; a++)
            {
                string strAnimId = fActions.GetWord();
                DWORD dwAnimFlags = fActions.GetDWord();
                DWORD dwMasterAnimFlags = fActions.GetDWord();
                fSource.WriteLine("  [\"{0}\", {1}, {2},", strAnimId, DecompileFlags(dwAnimFlags), DecompileMasterFlags(dwMasterAnimFlags));
                int iAnimSequences = fActions.GetInt();
                for (int s = 0; s < iAnimSequences; s++)
                {
                    double dDuration = fActions.GetDouble();
                    string strName = fActions.GetWord();
                    fSource.Write("    [{0}, \"{1}\",", dDuration.ToString(CultureInfo.GetCultureInfo("en-US")), strName);
                    int iBeginFrame = fActions.GetInt(), iEndingFrame = fActions.GetInt();
                    DWORD dwSequenceFlags = fActions.GetDWord();

                    var dd = new string[5]; //NOTE: Type string for non-english version of windows
                    bool bZeroes = true;
                    for (int d = 0; d < 5; d++)
                    {
                        dd[d] = fActions.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US"));
                        if (dd[d] != "0")
                            bZeroes = false;
                    }
                    if (bZeroes)
                        fSource.Write(" {0}, {1}, {2}],\r\n", iBeginFrame, iEndingFrame, DecompileSequenceFlags(dwSequenceFlags));
                    else
                        fSource.Write(" {0}, {1}, {2}, {3}, ({4}, {5}, {6}), {7}],\r\n", iBeginFrame, iEndingFrame,
                                DecompileSequenceFlags(dwSequenceFlags), DecompilePack((DWORD)Convert.ToDouble(dd[0], CultureInfo.GetCultureInfo("en-US"))), dd[1], dd[2], dd[3], dd[4]);
                }
                fSource.WriteLine("  ],");
            }
            fSource.Write("]");
            fSource.Close();
            fActions.Close();

            Common.GenerateId("ID_animations.py", Common.Animations, "anim");
        }
    }
}
