using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Decomp.Core.Operators;

namespace Decomp.Core
{
    public enum Mode
    {
        Caribbean = 3, //Caribbean
        WarbandScriptEnhancer450 = 2, //Warband v1.171 + WSE
        WarbandScriptEnhancer320 = 1, //Warband v1.153 + WSE
        Vanilla = 0 //M&B v1.011/1.010
    }

    public static class Common
    {
        public static string ModuleConstantsText = @"from ID_animations import *
from ID_factions import *
from ID_info_pages import *
from ID_items import *
from ID_map_icons import *
from ID_menus import *
from ID_meshes import *
from ID_mission_templates import *
from ID_music import *
from ID_particle_systems import *
from ID_parties import *
from ID_party_templates import *
from ID_postfx_params import *
from ID_presentations import *
from ID_quests import *
from ID_scenes import *
from ID_scene_props import *
from ID_scripts import *
from ID_skills import *
from ID_sounds import *
from ID_strings import *
from ID_tableau_materials import *
from ID_troops import *";

        public static string ModuleConstantsVanillaText = @"from ID_animations import *
from ID_factions import *
from ID_items import *
from ID_map_icons import *
from ID_menus import *
from ID_meshes import *
from ID_mission_templates import *
from ID_music import *
from ID_particle_systems import *
from ID_parties import *
from ID_party_templates import *
from ID_presentations import *
from ID_quests import *
from ID_scenes import *
from ID_scene_props import *
from ID_scripts import *
from ID_skills import *
from ID_sounds import *
from ID_strings import *
from ID_tableau_materials import *
from ID_troops import *";

        public static Mode SelectedMode = Mode.WarbandScriptEnhancer450;

        public static bool IsVanillaMode => SelectedMode == Mode.Vanilla;

        public static string[] Procedures;
        public static string[] QuickStrings;
        public static string[] Strings;
        public static string[] Items;
        public static string[] Troops;
        public static string[] Factions;
        public static string[] Quests;
        public static string[] PTemps;
        public static string[] Parties;
        public static string[] Menus;
        public static string[] Sounds;
        public static string[] Skills;
        public static string[] Meshes;
        public static string[] Variables;
        public static string[] DialogStates;
        public static string[] Scenes;
        public static string[] MissionTemplates;
        public static string[] ParticleSystems;
        public static string[] SceneProps;
        public static string[] MapIcons;
        public static string[] Presentations;
        public static string[] Tableaus;
        public static string[] Animations;
        public static string[] Music;
        public static string[] Skins;

        public static string GetCommonIdentifier(string prefix, string[] array, int index, bool useQuotes = false)
        {
            if (index < 0 || index >= array.Length) return index.ToString(CultureInfo.GetCultureInfo("en-US"));
            var s = prefix + (prefix.Length > 0 && prefix[prefix.Length - 1] == '_' ? "" : "_") +
                    array[index];
            return useQuotes ? "\"" + s + "\"" : s;
        }

        public static string GetCommonIdentifier(string prefix, string[] array, ulong index, bool useQuotes = false)
        {
            if (index >= (ulong)array.Length) return index.ToString(CultureInfo.GetCultureInfo("en-US"));
            var s = prefix + (prefix.Length > 0 && prefix[prefix.Length - 1] == '_' ? "" : "_") +
                    array[index];
            return useQuotes ? "\"" + s + "\"" : s;
        }

        public static Dictionary<int, Operator> Operators;

        public static Operator FindOperator(int operatorCode)
        {
            return Operators.ContainsKey(operatorCode) ? 
                Operators[operatorCode] : 
                new Operator(operatorCode.ToString(CultureInfo.GetCultureInfo("en-US")), operatorCode);
        }

        public static string InputPath;
        public static string OutputPath;

        public static string GetParam(ulong lParam)
        {
            ulong lTag = (lParam & 0xFF00000000000000) >> 56;
            switch (lTag)
            {
                case 1:
                    var iReg = (int)lParam;
                    return "reg" + Convert.ToString(iReg);
                case 2:
                    var iVariable = (int)lParam;
                    if (iVariable < Variables.Length)
                        return "\"$" + Variables[iVariable] + "\"";
                    return $"0x{lParam:x16}";
                case 3:
                    var iString = (int)lParam;
                    return iString < Strings.Length ? "\"str_" + Strings[iString] + "\"" : $"0x{lParam:x16}";
                case 4:
                    var iItem = (int)lParam;
                    return iItem < Items.Length ? "\"itm_" + Items[iItem] + "\"" : $"0x{lParam:x16}";
                case 5:
                    var iTroop = (int)lParam;
                    return iTroop < Troops.Length ? "\"trp_" + Troops[iTroop] + "\"" : $"0x{lParam:x16}";
                case 6:
                    var iFaction = (int)lParam;
                    return iFaction < Factions.Length ? "\"fac_" + Factions[iFaction] + "\"" : $"0x{lParam:x16}";
                case 7:
                    var iQuest = (int)lParam;
                    return iQuest < Quests.Length ? "\"qst_" + Quests[iQuest] + "\"" : $"0x{lParam:x16}";
                case 8:
                    var iPTemplate = (int)lParam;
                    return iPTemplate < PTemps.Length ? "\"pt_" + PTemps[iPTemplate] + "\"" : $"0x{lParam:x16}";
                case 9:
                    var iParty = (int)lParam;
                    return iParty < Parties.Length ? "\"p_" + Parties[iParty] + "\"" : $"0x{lParam:x16}";
                case 10:
                    var iScene = (int)lParam;
                    return iScene < Scenes.Length ? "\"scn_" + Scenes[iScene] + "\"" : $"0x{lParam:x16}";
                case 11:
                    var iMTemplate = (int)lParam;
                    return iMTemplate < MissionTemplates.Length ? "\"mt_" + MissionTemplates[iMTemplate] + "\"" : $"0x{lParam:x16}";
                case 12:
                    var iMenu = (int)lParam;
                    return iMenu < Menus.Length ? "\"mnu_" + Menus[iMenu] + "\"" : $"0x{lParam:x16}";
                case 13:
                    var iProcedure = (int)lParam;
                    return iProcedure < Procedures.Length ? "\"script_" + Procedures[iProcedure] + "\"" : $"0x{lParam:x16}";
                case 14:
                    var iParticle = (int)lParam;
                    return iParticle < ParticleSystems.Length ? "\"psys_" + ParticleSystems[iParticle] + "\"" :
                        $"0x{lParam:x16}";
                case 15:
                    var iSceneProp = (int)lParam;
                    return iSceneProp < SceneProps.Length ? "\"spr_" + SceneProps[iSceneProp] + "\"" : $"0x{lParam:x16}";
                case 16:
                    var iSound = (int)lParam;
                    return iSound < Sounds.Length ? "\"snd_" + Sounds[iSound] + "\"" : $"0x{lParam:x16}";
                case 17:
                    return "\":var" + Convert.ToString((int)lParam) + "\"";
                case 18:
                    var iIcon = (int)lParam;
                    return iIcon < MapIcons.Length ? "\"icon_" + MapIcons[iIcon] + "\"" : $"0x{lParam:x16}";
                case 19:
                    var iSkill = (int)lParam;
                    return iSkill < Skills.Length ? "\"skl_" + Skills[iSkill] + "\"" : $"0x{lParam:x16}";
                case 20:
                    var iMesh = (int)lParam;
                    return iMesh < Meshes.Length ? "\"mesh_" + Meshes[iMesh] + "\"" : $"0x{lParam:x16}";
                case 21:
                    var iPresentation = (int)lParam;
                    return iPresentation < Presentations.Length ? "\"prsnt_" + Presentations[iPresentation] + "\"" : $"0x{lParam:x16}";
                case 22:
                    var iQuickString = (int)lParam;
                    return iQuickString < QuickStrings.Length ? "\"@" + QuickStrings[iQuickString] + "\"" : $"0x{lParam:x16}";
                case 23:
                    var iTrack = (int)lParam;
                    return iTrack < Music.Length ? "\"track_" + Music[iTrack] + "\"" : $"0x{lParam:x16}";
                case 24:
                    var iTableau = (int)lParam;
                    return iTableau < Tableaus.Length ? "\"tableau_" + Tableaus[iTableau] + "\"" : $"0x{lParam:x16}";
                case 25:
                    var iAnim = (int)lParam;
                    return iAnim < Animations.Length ? "\"anim_" + Animations[iAnim] + "\"" : $"0x{lParam:x16}";
                default:
                    return lParam.ToString(CultureInfo.GetCultureInfo("en-US"));
            }
        }

        public static string GetTriggerParam(double dblParam)
        {
            switch ((int)dblParam)
            {
                case -2: return "ti_on_game_start";
                case -5: return "ti_simulate_battle";
                case -6: return "ti_on_party_encounter";
                case -8: return "ti_question_answered";
                case -15: return "ti_server_player_joined";
                case -16: return "ti_on_multiplayer_mission_end";
                case -19: return "ti_before_mission_start";
                case -20: return "ti_after_mission_start";
                case -21: return "ti_tab_pressed";
                case -22: return "ti_inventory_key_pressed";
                case -23: return "ti_escape_pressed";
                case -24: return "ti_battle_window_opened";
                case -25: return "ti_on_agent_spawn";
                case -26: return "ti_on_agent_killed_or_wounded";
                case -27: return "ti_on_agent_knocked_down";
                case -28: return "ti_on_agent_hit";
                case -29: return "ti_on_player_exit";
                case -30: return "ti_on_leave_area";
                case -40: return "ti_on_scene_prop_init";
                case -42: return "ti_on_scene_prop_hit";
                case -43: return "ti_on_scene_prop_destroy";
                case -44: return "ti_on_scene_prop_use";
                case -45: return "ti_on_scene_prop_is_animating";
                case -46: return "ti_on_scene_prop_animation_finished";
                case -47: return "ti_on_scene_prop_start_use";
                case -48: return "ti_on_scene_prop_cancel_use";
                case -50: return "ti_on_init_item";
                case -51: return "ti_on_weapon_attack";
                case -52: return "ti_on_missile_hit";
                case -53: return "ti_on_item_picked_up";
                case -54: return "ti_on_item_dropped";
                case -55: return "ti_on_agent_mount";
                case -56: return "ti_on_agent_dismount";
                case -57: return "ti_on_item_wielded";
                case -58: return "ti_on_item_unwielded";
                case -60: return "ti_on_presentation_load";
                case -61: return "ti_on_presentation_run";
                case -62: return "ti_on_presentation_event_state_change";
                case -63: return "ti_on_presentation_mouse_enter_leave";
                case -64: return "ti_on_presentation_mouse_press";
                case -70: return "ti_on_init_map_icon";
                case -71: return "ti_on_order_issued";
                case -75: return "ti_on_switch_to_map";
                case -76: return "ti_scene_prop_deformation_finished";
                case -80: return "ti_on_shield_hit";
                case -100: return "ti_on_scene_prop_stepped_on";
                case -101: return "ti_on_init_missile";
                case -102: return "ti_on_agent_turn";
                case -103: return SelectedMode == Mode.WarbandScriptEnhancer450 ? "ti_on_agent_blocked" : "ti_on_shield_hit"; 
                case -104: return "ti_on_missile_dive";
                case -105: return "ti_on_agent_start_reloading";
                case -106: return "ti_on_agent_end_reloading";
                case 100000000: return "ti_once";
                default: return dblParam.ToString(CultureInfo.GetCultureInfo("en-US"));
            }
        }

        public static string GetIndentations(int indentation)
        {
            return new String(' ', Math.Max(indentation, 0) << 1);
        }
        
        public static void PrintStatement(ref Text fInput, ref Win32FileWriter fOutput, int iRecords, string strDefaultIndentation)
        {
            int indentations = 0;
            for (int r = 0; r < iRecords; r++)
            {
                long iOpCode = fInput.GetInt64();

                string strPrefixNeg = "";
                if ((iOpCode & 0x80000000) != 0)
                {
                    strPrefixNeg = "neg|";
                    iOpCode ^= 0x80000000;
                }
                string strPrefixThisOrNext = "";
                if ((iOpCode & 0x40000000) != 0)
                {
                    strPrefixThisOrNext = "this_or_next|";
                    iOpCode ^= 0x40000000;
                }

                var op = FindOperator((int)(iOpCode & 0xFFFF));

                if (iOpCode == 4 || iOpCode == 6 || iOpCode == 7 || iOpCode == 11 || iOpCode == 12 || iOpCode == 15 || iOpCode == 16 || iOpCode == 17 ||
                    iOpCode == 18)
                    indentations++;
                if (iOpCode == 3)
                    indentations--;

                var strIdentations = iOpCode == 4 || iOpCode == 5 || iOpCode == 6 || iOpCode == 7 || iOpCode == 11 || iOpCode == 12 || iOpCode == 15 || iOpCode == 16 || iOpCode == 17 ||
                                      iOpCode == 18 ? GetIndentations(indentations - 1) : GetIndentations(indentations);

                string strOpCode = null;
                if (strPrefixNeg != "" && iOpCode >= 30 && iOpCode <= 32)
                {
                    switch (iOpCode)
                    {
                        case 30:
                            strOpCode = "lt";
                            break;
                        case 31:
                            strOpCode = "neq";
                            break;
                        case 32:
                            strOpCode = "le";
                            break;
                    }
                    fOutput.Write("{0}{1}({2}{3}", strIdentations, strDefaultIndentation, strPrefixThisOrNext, strOpCode);
                }
                else
                {
                    /*try
                    {
                        strOpCode = Operations[iOpCode];
                    }
                    catch (Exception)
                    {
                        strOpCode = Convert.ToString(iOpCode);
                    }*/

                    //strOpCode = Operations.ContainsKey(iOpCode) ? Operations[iOpCode] :
                    //    CustomOperations.HaveCommand((int)(iOpCode & 0x1FFF)) ? CustomOperations.GetCommandName((int)(iOpCode & 0x1FFF)) : Convert.ToString(iOpCode);
                    strOpCode = op.Value;
                    fOutput.Write("{0}{1}({2}{3}{4}", strIdentations, strDefaultIndentation, strPrefixNeg, strPrefixThisOrNext, strOpCode);
                }
                
                int iParams = fInput.GetInt();
                for (int p = 0; p < iParams; p++)
                {
                    string strParam = fInput.GetWord();
                    fOutput.Write(", {0}", op.GetParameter(p, strParam));
                }
                fOutput.WriteLine("),");

            }
        }

        public static string GetKey(ulong lKeyCode)
        {
            switch (lKeyCode)
            {
                case 0x02: return "key_1";
                case 0x03: return "key_2";
                case 0x04: return "key_3";
                case 0x05: return "key_4";
                case 0x06: return "key_5";
                case 0x07: return "key_6";
                case 0x08: return "key_7";
                case 0x09: return "key_8";
                case 0x0a: return "key_9";
                case 0x0b: return "key_0";
                case 0x1e: return "key_a";
                case 0x30: return "key_b";
                case 0x2e: return "key_c";
                case 0x20: return "key_d";
                case 0x12: return "key_e";
                case 0x21: return "key_f";
                case 0x22: return "key_g";
                case 0x23: return "key_h";
                case 0x17: return "key_i";
                case 0x24: return "key_j";
                case 0x25: return "key_k";
                case 0x26: return "key_l";
                case 0x32: return "key_m";
                case 0x31: return "key_n";
                case 0x18: return "key_o";
                case 0x19: return "key_p";
                case 0x10: return "key_q";
                case 0x13: return "key_r";
                case 0x1f: return "key_s";
                case 0x14: return "key_t";
                case 0x16: return "key_u";
                case 0x2f: return "key_v";
                case 0x11: return "key_w";
                case 0x2d: return "key_x";
                case 0x15: return "key_y";
                case 0x2c: return "key_z";
                case 0x52: return "key_numpad_0";
                case 0x4f: return "key_numpad_1";
                case 0x50: return "key_numpad_2";
                case 0x51: return "key_numpad_3";
                case 0x4b: return "key_numpad_4";
                case 0x4c: return "key_numpad_5";
                case 0x4d: return "key_numpad_6";
                case 0x47: return "key_numpad_7";
                case 0x48: return "key_numpad_8";
                case 0x49: return "key_numpad_9";
                case 0x45: return "key_num_lock";
                case 0xb5: return "key_numpad_slash";
                case 0x37: return "key_numpad_multiply";
                case 0x4a: return "key_numpad_minus";
                case 0x4e: return "key_numpad_plus";
                case 0x9c: return "key_numpad_enter";
                case 0x53: return "key_numpad_period";
                case 0xd2: return "key_insert";
                case 0xd3: return "key_delete";
                case 0xc7: return "key_home";
                case 0xcf: return "key_end";
                case 0xc9: return "key_page_up";
                case 0xd1: return "key_page_down";
                case 0xc8: return "key_up";
                case 0xd0: return "key_down";
                case 0xcb: return "key_left";
                case 0xcd: return "key_right";
                case 0x3b: return "key_f1";
                case 0x3c: return "key_f2";
                case 0x3d: return "key_f3";
                case 0x3e: return "key_f4";
                case 0x3f: return "key_f5";
                case 0x40: return "key_f6";
                case 0x41: return "key_f7";
                case 0x42: return "key_f8";
                case 0x43: return "key_f9";
                case 0x44: return "key_f10";
                case 0x57: return "key_f11";
                case 0x58: return "key_f12";
                case 0x39: return "key_space";
                case 0x01: return "key_escape";
                case 0x1c: return "key_enter";
                case 0x0f: return "key_tab";
                case 0x0e: return "key_back_space";
                case 0x1a: return "key_open_braces";
                case 0x1b: return "key_close_braces";
                case 0x33: return "key_comma";
                case 0x34: return "key_period";
                case 0x35: return "key_slash";
                case 0x2b: return "key_back_slash";
                case 0x0d: return "key_equals";
                case 0x0c: return "key_minus";
                case 0x27: return "key_semicolon";
                case 0x28: return "key_apostrophe";
                case 0x29: return "key_tilde";
                case 0x3a: return "key_caps_lock";
                case 0x2a: return "key_left_shift";
                case 0x36: return "key_right_shift";
                case 0x1d: return "key_left_control";
                case 0x9d: return "key_right_control";
                case 0x38: return "key_left_alt";
                case 0xb8: return "key_right_alt";
                case 0xe0: return "key_left_mouse_button";
                case 0xe1: return "key_right_mouse_button";
                case 0xe2: return "key_middle_mouse_button";
                case 0xe3: return "key_mouse_button_4";
                case 0xe4: return "key_mouse_button_5";
                case 0xe5: return "key_mouse_button_6";
                case 0xe6: return "key_mouse_button_7";
                case 0xe7: return "key_mouse_button_8";
                case 0xee: return "key_mouse_scroll_up";
                case 0xef: return "key_mouse_scroll_down";
                case 0xf0: return "key_xbox_a";
                case 0xf1: return "key_xbox_b";
                case 0xf2: return "key_xbox_x";
                case 0xf3: return "key_xbox_y";
                case 0xf4: return "key_xbox_dpad_up";
                case 0xf5: return "key_xbox_dpad_down";
                case 0xf6: return "key_xbox_dpad_right";
                case 0xf7: return "key_xbox_dpad_left";
                case 0xf8: return "key_xbox_start";
                case 0xf9: return "key_xbox_back";
                case 0xfa: return "key_xbox_rbumber";
                case 0xfb: return "key_xbox_lbumber";
                case 0xfc: return "key_xbox_ltrigger";
                case 0xfd: return "key_xbox_rtrigger";
                case 0xfe: return "key_xbox_rstick";
                case 0xff: return "key_xbox_lstick";
                default: return $"0x{lKeyCode:x}";
            }
        }

        public static string GetGameKey(ulong lKeyCode)
        {
            switch (lKeyCode)
            {
                case 0: return "gk_move_forward";
                case 1: return "gk_move_backward";
                case 2: return "gk_move_left";
                case 3: return "gk_move_right";
                case 4: return "gk_action";
                case 5: return "gk_jump";
                case 6: return "gk_attack";
                case 7: return "gk_defend";
                case 8: return "gk_kick";
                case 9: return "gk_toggle_weapon_mode";
                case 10: return "gk_equip_weapon_1";
                case 11: return "gk_equip_weapon_2";
                case 12: return "gk_equip_weapon_3";
                case 13: return "gk_equip_weapon_4";
                case 14: return "gk_equip_primary_weapon";
                case 15: return "gk_equip_secondary_weapon";
                case 16: return "gk_drop_weapon";
                case 17: return "gk_sheath_weapon";
                case 18: return "gk_leave";
                case 19: return "gk_zoom";
                case 20: return "gk_view_char";
                case 21: return "gk_cam_toggle";
                case 22: return "gk_view_orders";
                case 23: return "gk_order_1";
                case 24: return "gk_order_2";
                case 25: return "gk_order_3";
                case 26: return "gk_order_4";
                case 27: return "gk_order_5";
                case 28: return "gk_order_6";
                case 29: return "gk_everyone_hear";
                case 30: return "gk_infantry_hear";
                case 31: return "gk_archers_hear";
                case 32: return "gk_cavalry_hear";
                case 33: return "gk_group3_hear";
                case 34: return "gk_group4_hear";
                case 35: return "gk_group5_hear";
                case 36: return "gk_group6_hear";
                case 37: return "gk_group7_hear";
                case 38: return "gk_group8_hear";
                case 39: return "gk_reverse_order_group";
                case 40: return "gk_everyone_around_hear";
                case 41: return "gk_mp_message_all";
                case 42: return "gk_mp_message_team";
                case 43: return "gk_character_window";
                case 44: return "gk_inventory_window";
                case 45: return "gk_party_window";
                case 46: return "gk_quests_window";
                case 47: return "gk_game_log_window";
                case 48: return "gk_quick_save";
                case 49: return "gk_crouch";
                case 50: return "gk_order_7";
                case 51: return "gk_order_8";
                default: return $"0x{lKeyCode:x}";
            }
        }

        public static bool IsStringRegister(ulong lParam)
        {
            ulong lTag = (lParam & 0xFF00000000000000) >> 56;
            return lTag == 0;
        }

        public static bool IsKey(ulong lKeyCode)
        {
            ulong lTag = (lKeyCode & 0xFF00000000000000) >> 56;
            return lTag == 0;
        }

        public static bool IsTextFlags(ulong lFlags)
        {
            ulong lTag = (lFlags & 0xFF00000000000000) >> 56;
            return lTag == 0;
        }

        public static bool IsPosition(ulong lFlags)
        {
            ulong lTag = (lFlags & 0xFF00000000000000) >> 56;
            return lTag == 0;
        }

        public static bool IsFloatRegister(ulong lFlags)
        {
            ulong lTag = (lFlags & 0xFF00000000000000) >> 56;
            return lTag == 0;
        }

        public static bool IsFaceKey(ulong lFlags)
        {
            ulong lTag = (lFlags & 0xFF00000000000000) >> 56;
            return lTag == 0;
        }

        public static bool NotParam(ulong lFlags)
        {
            var lTag = (lFlags & 0xFF00000000000000) >> 56;
            return lTag == 0;
        }

        public static string GetFaceKey(ulong lFaceKeyCode)
        {
            return Convert.ToString(lFaceKeyCode);
        }

        public static string DecompileTextFlags(uint dwFlag)
        {
            string strFlag = "";

            string[] strFlags = { "tf_left_align", "tf_right_align", "tf_center_justify", "tf_double_space", "tf_vertical_align_center", "tf_scrollable",
            "tf_single_line", "tf_with_outline", "tf_scrollable_style_2" };
            uint[] dwFlags = { 0x00000004, 0x00000008, 0x00000010, 0x00000800, 0x00001000, 0x00002000, 0x00008000, 0x00010000, 0x00020000 };
            for (int i = 0; i < (dwFlags.Length); i++)
            {
                if ((dwFlag & dwFlags[i]) != 0)
                {
                    strFlag += strFlags[i] + "|";
                    dwFlag ^= dwFlags[i];
                }
            }

            strFlag = strFlag == "" ? "0" : strFlag.Remove(strFlag.Length - 1, 1);

            return strFlag;
        }

        public static string GetAgentClass(ulong lClass)
        {
            switch (lClass)
            {
                case 0:
                    return "grc_infantry";
                case 1:
                    return "grc_archers";
                case 2:
                    return "grc_cavalry";
                case 3:
                    return "grc_infantry";
                case 9:
                    return "grc_everyone";
                default:
                    return lClass.ToString(CultureInfo.GetCultureInfo(1033));
            }
        }

        public static string GetTeamOrder(ulong lOrder)
        {
            switch (lOrder)
            {
                case 0: return "mordr_hold";
                case 1: return "mordr_follow";
                case 2: return "mordr_charge";
                case 3: return "mordr_mount";
                case 4: return "mordr_dismount";
                case 5: return "mordr_advance";
                case 6: return "mordr_fall_back";
                case 7: return "mordr_stand_closer";
                case 8: return "mordr_spread_out";
                case 9: return "mordr_use_blunt_weapons";
                case 10: return "mordr_use_melee_weapons";
                case 11: return "mordr_use_ranged_weapons";
                case 12: return "mordr_use_any_weapon";
                case 13: return "mordr_stand_ground";
                case 14: return "mordr_fire_at_my_command";
                case 15: return "mordr_all_fire_now";
                case 16: return "mordr_left_fire_now";
                case 17: return "mordr_middle_fire_now";
                case 18: return "mordr_right_fire_now";
                case 19: return "mordr_fire_at_will";
                case 20: return "mordr_retreat";
                case 21: return "mordr_form_1_row";
                case 22: return "mordr_form_2_row";
                case 23: return "mordr_form_3_row";
                case 24: return "mordr_form_4_row";
                case 25: return "mordr_form_5_row";
                default: return lOrder.ToString(CultureInfo.GetCultureInfo(1033));
            }
        }

        public static string GetPartyBehavior(ulong lBehavior)
        {
            var iAIbehaviour = (int)lBehavior;
            string[] strAIbehaviours = { "ai_bhvr_hold", "ai_bhvr_travel_to_party", "ai_bhvr_patrol_location", "ai_bhvr_patrol_party",
                "ai_bhvr_attack_party", "ai_bhvr_avoid_party", "ai_bhvr_travel_to_point", "ai_bhvr_negotiate_party", "ai_bhvr_in_town",
                "ai_bhvr_travel_to_ship", "ai_bhvr_escort_party", "ai_bhvr_driven_by_party" };
            return iAIbehaviour <= 11 ? strAIbehaviours[iAIbehaviour] : iAIbehaviour.ToString(CultureInfo.GetCultureInfo("en-US"));
        }

        public static string GetCharacterAttribute(ulong lAttribute)
        {
            switch (lAttribute)
            {
                case 0:
                    return "ca_strength";
                case 1:
                    return "ca_agility";
                case 2:
                    return "ca_intelligence";
                case 3:
                    return "ca_charisma";
                default:
                    return lAttribute.ToString(CultureInfo.GetCultureInfo("en-US"));
            }
        }

        public static string GetWeaponProficiency(ulong lProficiency)
        {
            switch (lProficiency)
            {
                case 0: return "wpt_one_handed_weapon";
                case 1: return "wpt_two_handed_weapon";
                case 2: return "wpt_polearm";
                case 3: return "wpt_archery";
                case 4: return "wpt_crossbow";
                case 5: return "wpt_throwing";
                case 6: return "wpt_firearm";
                default: return lProficiency.ToString(CultureInfo.GetCultureInfo("en-US"));
            }
        }

        public static string GetInventorySlot(ulong lSlot)
        {
            switch (lSlot)
            {
                case 0: return "ek_item_0";
                case 1: return "ek_item_1";
                case 2: return "ek_item_2";
                case 3: return "ek_item_3";
                case 4: return "ek_head";
                case 5: return "ek_body";
                case 6: return "ek_foot";
                case 7: return "ek_gloves";
                case 8: return "ek_horse";
                case 9: return "ek_food";
                default: return lSlot.ToString(CultureInfo.GetCultureInfo("en-US"));
            }
        }
        
        public static string GetTooltip(ulong t)
        {
            switch (t)
            {
                case 1: return "tooltip_agent";
                case 2: return "tooltip_horse";
                case 3: return "tooltip_my_horse";
                case 5: return "tooltip_container";
                case 6: return "tooltip_door";
                case 7: return "tooltip_item";
                case 8: return "tooltip_leave_area";
                case 9: return "tooltip_prop";
                case 10: return "tooltip_destructible_prop";
                default: return t.ToString(CultureInfo.GetCultureInfo("en-US"));
            }
        }

        public static string GetColor(ulong color)
        {
            if (color <= 0xFFFFFFFF && color > 0x00FFFFFF) return "0x" + color.ToString("X8");
            if (color <= 0x00FFFFFF) return "0x" + color.ToString("X6");
            return "0x" + color.ToString("X");
        }

        public static string GetAlpha(ulong alpha)
        {
            return String.Concat("0x", alpha <= 0xFF ? alpha.ToString("X2") : alpha.ToString("X"));
        }

        public static bool NeedId = true;
        public static void GenerateId(string fileOut, string[] content, string prefix = "")
        {
            if (!NeedId) return;
            var f = new Win32FileWriter(Path.Combine(OutputPath, fileOut));
            if (prefix.Length > 0 && prefix[prefix.Length - 1] != '_') prefix += '_';
            for (int i = 0; i < content.Length; i++) f.WriteLine("{0}{1} = {2}", prefix, content[i], i);
            f.Close();
        } 
    }
}
