using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Decomp.Core.Operators;

namespace Decomp.Core
{
    public enum Mode
    {
        Caribbean = 3, //Caribbean
        WarbandScriptEnhancer450 = 2, //Warband v1.173 + WSE
        WarbandScriptEnhancer320 = 1, //Warband v1.153 + WSE
        Vanilla = 0 //M&B v1.011/1.010
    }

    public static class Common
    {
        public static string ModuleConstantsText { get; set; } = @"from ID_animations import *
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

        public static string ModuleConstantsVanillaText { get; set; } = @"from ID_animations import *
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

        public static Mode SelectedMode { get; set; } = Mode.WarbandScriptEnhancer450;

        public static bool IsVanillaMode => SelectedMode == Mode.Vanilla;

        public static IReadOnlyList<string> Procedures { get; set; }
        public static IReadOnlyList<string> QuickStrings { get; set; }
        public static IReadOnlyList<string> Strings { get; set; }
        public static IReadOnlyList<string> Items { get; set; }
        public static IReadOnlyList<string> Troops { get; set; }
        public static IReadOnlyList<string> Factions { get; set; }
        public static IReadOnlyList<string> Quests { get; set; }
        public static IReadOnlyList<string> PTemps { get; set; }
        public static IReadOnlyList<string> Parties { get; set; }
        public static IReadOnlyList<string> Menus { get; set; }
        public static IReadOnlyList<string> Sounds { get; set; }
        public static IReadOnlyList<string> Skills { get; set; }
        public static IReadOnlyList<string> Meshes { get; set; }
        public static IReadOnlyList<string> Variables { get; set; }
        public static IReadOnlyList<string> DialogStates { get; set; }
        public static IReadOnlyList<string> Scenes { get; set; }
        public static IReadOnlyList<string> MissionTemplates { get; set; }
        public static IReadOnlyList<string> ParticleSystems { get; set; }
        public static IReadOnlyList<string> SceneProps { get; set; }
        public static IReadOnlyList<string> MapIcons { get; set; }
        public static IReadOnlyList<string> Presentations { get; set; }
        public static IReadOnlyList<string> Tableaus { get; set; }
        public static IReadOnlyList<string> Animations { get; set; }
        public static IReadOnlyList<string> Music { get; set; }
        public static IReadOnlyList<string> Skins { get; set; }
        public static IReadOnlyList<string> InfoPages { get; set; }

        public static string GetCommonIdentifier(string prefix, IList<string> array, int index, bool useQuotes = false)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            if (index < 0 || index >= array.Count) return index.ToString(CultureInfo.GetCultureInfo("en-US"));
            var s = prefix + (prefix.Length > 0 && prefix[prefix.Length - 1] == '_' ? "" : "_") +
                    array[index];
            return useQuotes ? "\"" + s + "\"" : s;
        }

        public static string GetCommonIdentifier(string prefix, IList<string> array, ulong index, bool useQuotes = false)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            if (index >= (ulong)array.Count) return index.ToString(CultureInfo.GetCultureInfo("en-US"));
            var s = prefix + (prefix.Length > 0 && prefix[prefix.Length - 1] == '_' ? "" : "_") +
                    array[(int)index];
            return useQuotes ? "\"" + s + "\"" : s;
        }

        public static string GetCommonIdentifier(string prefix, IReadOnlyList<string> array, int index, bool useQuotes = false)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            if (index < 0 || index >= array.Count) return index.ToString(CultureInfo.GetCultureInfo("en-US"));
            var s = prefix + (prefix.Length > 0 && prefix[prefix.Length - 1] == '_' ? "" : "_") +
                    array[index];
            return useQuotes ? "\"" + s + "\"" : s;
        }

        public static string GetCommonIdentifier(string prefix, IReadOnlyList<string> array, ulong index, bool useQuotes = false)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            if (index >= (ulong)array.Count) return index.ToString(CultureInfo.GetCultureInfo("en-US"));
            var s = prefix + (prefix.Length > 0 && prefix[prefix.Length - 1] == '_' ? "" : "_") +
                    array[(int)index];
            return useQuotes ? "\"" + s + "\"" : s;
        }

        public static IReadOnlyDictionary<int, Operator> Operators { get; set; }

        public static Operator FindOperator(int operatorCode) => Operators.ContainsKey(operatorCode) ?  Operators[operatorCode] : new Operator(operatorCode.ToString(CultureInfo.GetCultureInfo("en-US")), operatorCode);

        public static string InputPath { get; set; }
        public static string OutputPath { get; set; }

        public static string GetParam(ulong lParam)
        {
            ulong lTag = (lParam & 0xFF00000000000000) >> 56;
            switch (lTag)
            {
                case 1:
                    var iReg = (int)lParam;
                    return "reg" + Convert.ToString(iReg, CultureInfo.GetCultureInfo("en-US"));
                case 2:
                    var iVariable = (int)lParam;
                    if (iVariable < Variables.Count)
                        return "\"$" + Variables[iVariable] + "\"";
                    return $"0x{lParam:x16}";
                case 3:
                    var iString = (int)lParam;
                    return iString < Strings.Count ? "\"str_" + Strings[iString] + "\"" : $"0x{lParam:x16}";
                case 4:
                    var iItem = (int)lParam;
                    return iItem < Items.Count ? "\"itm_" + Items[iItem] + "\"" : $"0x{lParam:x16}";
                case 5:
                    var iTroop = (int)lParam;
                    return iTroop < Troops.Count ? "\"trp_" + Troops[iTroop] + "\"" : $"0x{lParam:x16}";
                case 6:
                    var iFaction = (int)lParam;
                    return iFaction < Factions.Count ? "\"fac_" + Factions[iFaction] + "\"" : $"0x{lParam:x16}";
                case 7:
                    var iQuest = (int)lParam;
                    return iQuest < Quests.Count ? "\"qst_" + Quests[iQuest] + "\"" : $"0x{lParam:x16}";
                case 8:
                    var iPTemplate = (int)lParam;
                    return iPTemplate < PTemps.Count ? "\"pt_" + PTemps[iPTemplate] + "\"" : $"0x{lParam:x16}";
                case 9:
                    var iParty = (int)lParam;
                    return iParty < Parties.Count ? "\"p_" + Parties[iParty] + "\"" : $"0x{lParam:x16}";
                case 10:
                    var iScene = (int)lParam;
                    return iScene < Scenes.Count ? "\"scn_" + Scenes[iScene] + "\"" : $"0x{lParam:x16}";
                case 11:
                    var iMTemplate = (int)lParam;
                    return iMTemplate < MissionTemplates.Count ? "\"mt_" + MissionTemplates[iMTemplate] + "\"" : $"0x{lParam:x16}";
                case 12:
                    var iMenu = (int)lParam;
                    return iMenu < Menus.Count ? "\"mnu_" + Menus[iMenu] + "\"" : $"0x{lParam:x16}";
                case 13:
                    var iProcedure = (int)lParam;
                    return iProcedure < Procedures.Count ? "\"script_" + Procedures[iProcedure] + "\"" : $"0x{lParam:x16}";
                case 14:
                    var iParticle = (int)lParam;
                    return iParticle < ParticleSystems.Count ? "\"psys_" + ParticleSystems[iParticle] + "\"" :
                        $"0x{lParam:x16}";
                case 15:
                    var iSceneProp = (int)lParam;
                    return iSceneProp < SceneProps.Count ? "\"spr_" + SceneProps[iSceneProp] + "\"" : $"0x{lParam:x16}";
                case 16:
                    var iSound = (int)lParam;
                    return iSound < Sounds.Count ? "\"snd_" + Sounds[iSound] + "\"" : $"0x{lParam:x16}";
                case 17:
                    return "\":var" + Convert.ToString((int)lParam, CultureInfo.GetCultureInfo("en-US")) + "\"";
                case 18:
                    var iIcon = (int)lParam;
                    return iIcon < MapIcons.Count ? "\"icon_" + MapIcons[iIcon] + "\"" : $"0x{lParam:x16}";
                case 19:
                    var iSkill = (int)lParam;
                    return iSkill < Skills.Count ? "\"skl_" + Skills[iSkill] + "\"" : $"0x{lParam:x16}";
                case 20:
                    var iMesh = (int)lParam;
                    return iMesh < Meshes.Count ? "\"mesh_" + Meshes[iMesh] + "\"" : $"0x{lParam:x16}";
                case 21:
                    var iPresentation = (int)lParam;
                    return iPresentation < Presentations.Count ? "\"prsnt_" + Presentations[iPresentation] + "\"" : $"0x{lParam:x16}";
                case 22:
                    var iQuickString = (int)lParam;
                    return iQuickString < QuickStrings.Count ? "\"@" + QuickStrings[iQuickString] + "\"" : $"0x{lParam:x16}";
                case 23:
                    var iTrack = (int)lParam;
                    return iTrack < Music.Count ? "\"track_" + Music[iTrack] + "\"" : $"0x{lParam:x16}";
                case 24:
                    var iTableau = (int)lParam;
                    return iTableau < Tableaus.Count ? "\"tableau_" + Tableaus[iTableau] + "\"" : $"0x{lParam:x16}";
                case 25:
                    var iAnim = (int)lParam;
                    return iAnim < Animations.Count ? "\"anim_" + Animations[iAnim] + "\"" : $"0x{lParam:x16}";
                default:
                    return lParam.ToString(CultureInfo.GetCultureInfo("en-US"));
            }
        }

        public static string GetTriggerParam(double dblParam) => (int)dblParam switch
        {
            -2 => "ti_on_game_start",
            -5 => "ti_simulate_battle",
            -6 => "ti_on_party_encounter",
            -8 => "ti_question_answered",
            -15 => "ti_server_player_joined",
            -16 => "ti_on_multiplayer_mission_end",
            -19 => "ti_before_mission_start",
            -20 => "ti_after_mission_start",
            -21 => "ti_tab_pressed",
            -22 => "ti_inventory_key_pressed",
            -23 => "ti_escape_pressed",
            -24 => "ti_battle_window_opened",
            -25 => "ti_on_agent_spawn",
            -26 => "ti_on_agent_killed_or_wounded",
            -27 => "ti_on_agent_knocked_down",
            -28 => "ti_on_agent_hit",
            -29 => "ti_on_player_exit",
            -30 => "ti_on_leave_area",
            -40 => "ti_on_scene_prop_init",
            -42 => "ti_on_scene_prop_hit",
            -43 => "ti_on_scene_prop_destroy",
            -44 => "ti_on_scene_prop_use",
            -45 => "ti_on_scene_prop_is_animating",
            -46 => "ti_on_scene_prop_animation_finished",
            -47 => "ti_on_scene_prop_start_use",
            -48 => "ti_on_scene_prop_cancel_use",
            -50 => "ti_on_init_item",
            -51 => "ti_on_weapon_attack",
            -52 => "ti_on_missile_hit",
            -53 => "ti_on_item_picked_up",
            -54 => "ti_on_item_dropped",
            -55 => "ti_on_agent_mount",
            -56 => "ti_on_agent_dismount",
            -57 => "ti_on_item_wielded",
            -58 => "ti_on_item_unwielded",
            -60 => "ti_on_presentation_load",
            -61 => "ti_on_presentation_run",
            -62 => "ti_on_presentation_event_state_change",
            -63 => "ti_on_presentation_mouse_enter_leave",
            -64 => "ti_on_presentation_mouse_press",
            -70 => "ti_on_init_map_icon",
            -71 => "ti_on_order_issued",
            -75 => "ti_on_switch_to_map",
            -76 => "ti_scene_prop_deformation_finished",
            -80 => "ti_on_shield_hit",
            -100 => "ti_on_scene_prop_stepped_on",
            -101 => "ti_on_init_missile",
            -102 => "ti_on_agent_turn",
            -103 => SelectedMode == Mode.WarbandScriptEnhancer450 ? "ti_on_agent_blocked" : "ti_on_shield_hit",
            -104 => "ti_on_missile_dive",
            -105 => "ti_on_agent_start_reloading",
            -106 => "ti_on_agent_end_reloading",
            -107 => "ti_on_shield_penetrated",
            100000000 => "ti_once",
            _ => dblParam.ToString(CultureInfo.GetCultureInfo("en-US"))
        };

        public static string GetIndentations(int indentation) => new String(' ', Math.Max(indentation, 0) << 1);
        
        public static void PrintStatement(ref Text fInput, ref Win32FileWriter fOutput, int iRecords, string strDefaultIndentation)
        {
            if (fInput == null) throw new ArgumentNullException(nameof(fInput));
            if (fOutput == null) throw new ArgumentNullException(nameof(fOutput));

            var indentations = 0;
            for (int r = 0; r < iRecords; r++)
            {
                var iOpCode = fInput.GetInt64();

                var strPrefixNeg = "";
                if ((iOpCode & 0x80000000) != 0)
                {
                    strPrefixNeg = "neg|";
                    iOpCode ^= 0x80000000;
                }
                var strPrefixThisOrNext = "";
                if ((iOpCode & 0x40000000) != 0)
                {
                    strPrefixThisOrNext = "this_or_next|";
                    iOpCode ^= 0x40000000;
                }

                var op = FindOperator((int)(iOpCode & 0xFFFF));

                if (iOpCode == 4 || iOpCode == 6 || iOpCode == 7 || iOpCode == 11 || iOpCode == 12 || iOpCode == 15 || iOpCode == 16 || iOpCode == 17 ||
                    iOpCode == 18)
                    indentations++;
                else if (iOpCode == 3)
                    indentations--;

                var strIdentations = iOpCode == 4 || iOpCode == 5 || iOpCode == 6 || iOpCode == 7 || iOpCode == 11 || iOpCode == 12 || iOpCode == 15 || iOpCode == 16 || iOpCode == 17 ||
                                      iOpCode == 18 ? GetIndentations(indentations - 1) : GetIndentations(indentations);

                string strOpCode = null;
                if (strPrefixNeg.Length > 0 && iOpCode >= 30 && iOpCode <= 32)
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
                    strOpCode = op.Value;
                    fOutput.Write("{0}{1}({2}{3}{4}", strIdentations, strDefaultIndentation, strPrefixNeg, strPrefixThisOrNext, strOpCode);
                }
                
                int iParams = fInput.GetInt();
                for (int p = 0; p < iParams; p++)
                {
                    var strParam = fInput.GetWord();
                    fOutput.Write(", {0}", op.GetParameter(p, strParam));
                }
                fOutput.WriteLine("),");

            }
        }

        public static string GetKey(ulong lKeyCode) => lKeyCode switch
        {
            0x02 => "key_1",
            0x03 => "key_2",
            0x04 => "key_3",
            0x05 => "key_4",
            0x06 => "key_5",
            0x07 => "key_6",
            0x08 => "key_7",
            0x09 => "key_8",
            0x0a => "key_9",
            0x0b => "key_0",
            0x1e => "key_a",
            0x30 => "key_b",
            0x2e => "key_c",
            0x20 => "key_d",
            0x12 => "key_e",
            0x21 => "key_f",
            0x22 => "key_g",
            0x23 => "key_h",
            0x17 => "key_i",
            0x24 => "key_j",
            0x25 => "key_k",
            0x26 => "key_l",
            0x32 => "key_m",
            0x31 => "key_n",
            0x18 => "key_o",
            0x19 => "key_p",
            0x10 => "key_q",
            0x13 => "key_r",
            0x1f => "key_s",
            0x14 => "key_t",
            0x16 => "key_u",
            0x2f => "key_v",
            0x11 => "key_w",
            0x2d => "key_x",
            0x15 => "key_y",
            0x2c => "key_z",
            0x52 => "key_numpad_0",
            0x4f => "key_numpad_1",
            0x50 => "key_numpad_2",
            0x51 => "key_numpad_3",
            0x4b => "key_numpad_4",
            0x4c => "key_numpad_5",
            0x4d => "key_numpad_6",
            0x47 => "key_numpad_7",
            0x48 => "key_numpad_8",
            0x49 => "key_numpad_9",
            0x45 => "key_num_lock",
            0xb5 => "key_numpad_slash",
            0x37 => "key_numpad_multiply",
            0x4a => "key_numpad_minus",
            0x4e => "key_numpad_plus",
            0x9c => "key_numpad_enter",
            0x53 => "key_numpad_period",
            0xd2 => "key_insert",
            0xd3 => "key_delete",
            0xc7 => "key_home",
            0xcf => "key_end",
            0xc9 => "key_page_up",
            0xd1 => "key_page_down",
            0xc8 => "key_up",
            0xd0 => "key_down",
            0xcb => "key_left",
            0xcd => "key_right",
            0x3b => "key_f1",
            0x3c => "key_f2",
            0x3d => "key_f3",
            0x3e => "key_f4",
            0x3f => "key_f5",
            0x40 => "key_f6",
            0x41 => "key_f7",
            0x42 => "key_f8",
            0x43 => "key_f9",
            0x44 => "key_f10",
            0x57 => "key_f11",
            0x58 => "key_f12",
            0x39 => "key_space",
            0x01 => "key_escape",
            0x1c => "key_enter",
            0x0f => "key_tab",
            0x0e => "key_back_space",
            0x1a => "key_open_braces",
            0x1b => "key_close_braces",
            0x33 => "key_comma",
            0x34 => "key_period",
            0x35 => "key_slash",
            0x2b => "key_back_slash",
            0x0d => "key_equals",
            0x0c => "key_minus",
            0x27 => "key_semicolon",
            0x28 => "key_apostrophe",
            0x29 => "key_tilde",
            0x3a => "key_caps_lock",
            0x2a => "key_left_shift",
            0x36 => "key_right_shift",
            0x1d => "key_left_control",
            0x9d => "key_right_control",
            0x38 => "key_left_alt",
            0xb8 => "key_right_alt",
            0xe0 => "key_left_mouse_button",
            0xe1 => "key_right_mouse_button",
            0xe2 => "key_middle_mouse_button",
            0xe3 => "key_mouse_button_4",
            0xe4 => "key_mouse_button_5",
            0xe5 => "key_mouse_button_6",
            0xe6 => "key_mouse_button_7",
            0xe7 => "key_mouse_button_8",
            0xee => "key_mouse_scroll_up",
            0xef => "key_mouse_scroll_down",
            0xf0 => "key_xbox_a",
            0xf1 => "key_xbox_b",
            0xf2 => "key_xbox_x",
            0xf3 => "key_xbox_y",
            0xf4 => "key_xbox_dpad_up",
            0xf5 => "key_xbox_dpad_down",
            0xf6 => "key_xbox_dpad_right",
            0xf7 => "key_xbox_dpad_left",
            0xf8 => "key_xbox_start",
            0xf9 => "key_xbox_back",
            0xfa => "key_xbox_rbumber",
            0xfb => "key_xbox_lbumber",
            0xfc => "key_xbox_ltrigger",
            0xfd => "key_xbox_rtrigger",
            0xfe => "key_xbox_rstick",
            0xff => "key_xbox_lstick",
            _ => $"0x{lKeyCode:x}"
        };

        public static string GetGameKey(ulong lKeyCode) => lKeyCode switch
        {
            0 => "gk_move_forward",
            1 => "gk_move_backward",
            2 => "gk_move_left",
            3 => "gk_move_right",
            4 => "gk_action",
            5 => "gk_jump",
            6 => "gk_attack",
            7 => "gk_defend",
            8 => "gk_kick",
            9 => "gk_toggle_weapon_mode",
            10 => "gk_equip_weapon_1",
            11 => "gk_equip_weapon_2",
            12 => "gk_equip_weapon_3",
            13 => "gk_equip_weapon_4",
            14 => "gk_equip_primary_weapon",
            15 => "gk_equip_secondary_weapon",
            16 => "gk_drop_weapon",
            17 => "gk_sheath_weapon",
            18 => "gk_leave",
            19 => "gk_zoom",
            20 => "gk_view_char",
            21 => "gk_cam_toggle",
            22 => "gk_view_orders",
            23 => "gk_order_1",
            24 => "gk_order_2",
            25 => "gk_order_3",
            26 => "gk_order_4",
            27 => "gk_order_5",
            28 => "gk_order_6",
            29 => "gk_everyone_hear",
            30 => "gk_infantry_hear",
            31 => "gk_archers_hear",
            32 => "gk_cavalry_hear",
            33 => "gk_group3_hear",
            34 => "gk_group4_hear",
            35 => "gk_group5_hear",
            36 => "gk_group6_hear",
            37 => "gk_group7_hear",
            38 => "gk_group8_hear",
            39 => "gk_reverse_order_group",
            40 => "gk_everyone_around_hear",
            41 => "gk_mp_message_all",
            42 => "gk_mp_message_team",
            43 => "gk_character_window",
            44 => "gk_inventory_window",
            45 => "gk_party_window",
            46 => "gk_quests_window",
            47 => "gk_game_log_window",
            48 => "gk_quick_save",
            49 => "gk_crouch",
            50 => "gk_order_7",
            51 => "gk_order_8",
            _ => $"0x{lKeyCode:x}",
        };
        

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

        public static string GetFaceKey(ulong lFaceKeyCode) => Convert.ToString(lFaceKeyCode, CultureInfo.GetCultureInfo("en-US"));

        public static string DecompileTextFlags(uint dwFlag)
        {
            var sbFlag = new StringBuilder(32);

            string[] strFlags = { "tf_left_align", "tf_right_align", "tf_center_justify", "tf_double_space", "tf_vertical_align_center", "tf_scrollable",
            "tf_single_line", "tf_with_outline", "tf_scrollable_style_2" };
            uint[] dwFlags = { 0x00000004, 0x00000008, 0x00000010, 0x00000800, 0x00001000, 0x00002000, 0x00008000, 0x00010000, 0x00020000 };
            for (int i = 0; i < dwFlags.Length; i++)
            {
                if ((dwFlag & dwFlags[i]) == 0) continue;
                sbFlag.Append(strFlags[i]);
                sbFlag.Append('|');
                dwFlag ^= dwFlags[i];
            }

            if (sbFlag.Length == 0)
                sbFlag.Append('0');
            else
                sbFlag.Length--;

            return sbFlag.ToString();
        }

        public static string GetAgentClass(ulong lClass) => lClass switch 
        {
            0 => "grc_infantry",
            1 => "grc_archers",
            2 => "grc_cavalry",
            3 => "grc_infantry",
            9 => "grc_everyone",
            _ => lClass.ToString(CultureInfo.GetCultureInfo(1033))
        };

        public static string GetTeamOrder(ulong lOrder) => lOrder switch
        {
            0 => "mordr_hold",
            1 => "mordr_follow",
            2 => "mordr_charge",
            3 => "mordr_mount",
            4 => "mordr_dismount",
            5 => "mordr_advance",
            6 => "mordr_fall_back",
            7 => "mordr_stand_closer",
            8 => "mordr_spread_out",
            9 => "mordr_use_blunt_weapons",
            10 => "mordr_use_melee_weapons",
            11 => "mordr_use_ranged_weapons",
            12 => "mordr_use_any_weapon",
            13 => "mordr_stand_ground",
            14 => "mordr_fire_at_my_command",
            15 => "mordr_all_fire_now",
            16 => "mordr_left_fire_now",
            17 => "mordr_middle_fire_now",
            18 => "mordr_right_fire_now",
            19 => "mordr_fire_at_will",
            20 => "mordr_retreat",
            21 => "mordr_form_1_row",
            22 => "mordr_form_2_row",
            23 => "mordr_form_3_row",
            24 => "mordr_form_4_row",
            25 => "mordr_form_5_row",
            _ => lOrder.ToString(CultureInfo.GetCultureInfo(1033)),
        };
        

        public static string GetPartyBehavior(ulong lBehavior)
        {
            var iAIbehaviour = (int)lBehavior;
            string[] strAIbehaviours = { "ai_bhvr_hold", "ai_bhvr_travel_to_party", "ai_bhvr_patrol_location", "ai_bhvr_patrol_party",
                "ai_bhvr_attack_party", "ai_bhvr_avoid_party", "ai_bhvr_travel_to_point", "ai_bhvr_negotiate_party", "ai_bhvr_in_town",
                "ai_bhvr_travel_to_ship", "ai_bhvr_escort_party", "ai_bhvr_driven_by_party" };
            return iAIbehaviour <= 11 ? strAIbehaviours[iAIbehaviour] : iAIbehaviour.ToString(CultureInfo.GetCultureInfo("en-US"));
        }

        public static string GetCharacterAttribute(ulong lAttribute) => lAttribute switch
        {
            0 => "ca_strength",
            1 => "ca_agility",
            2 => "ca_intelligence",
            3 => "ca_charisma",
            _ => lAttribute.ToString(CultureInfo.GetCultureInfo("en-US")),
        };
        

        public static string GetWeaponProficiency(ulong lProficiency) => lProficiency switch
        {
            0 => "wpt_one_handed_weapon",
            1 => "wpt_two_handed_weapon",
            2 => "wpt_polearm",
            3 => "wpt_archery",
            4 => "wpt_crossbow",
            5 => "wpt_throwing",
            6 => "wpt_firearm",
            _ => lProficiency.ToString(CultureInfo.GetCultureInfo("en-US")),
        };

        public static string GetInventorySlot(ulong lSlot) => lSlot switch
        {
            0 => "ek_item_0",
            1 => "ek_item_1",
            2 => "ek_item_2",
            3 => "ek_item_3",
            4 => "ek_head",
            5 => "ek_body",
            6 => "ek_foot",
            7 => "ek_gloves",
            8 => "ek_horse",
            9 => "ek_food",
            _ => lSlot.ToString(CultureInfo.GetCultureInfo("en-US")),
        };
        
        
        public static string GetTooltip(ulong t) => t switch
        {
            1 => "tooltip_agent",
            2 => "tooltip_horse",
            3 => "tooltip_my_horse",
            5 => "tooltip_container",
            6 => "tooltip_door",
            7 => "tooltip_item",
            8 => "tooltip_leave_area",
            9 => "tooltip_prop",
            10 => "tooltip_destructible_prop",
            _ => t.ToString(CultureInfo.GetCultureInfo("en-US")),
        };

        public static string GetColor(ulong color)
        {
            if (color <= 0xFFFFFFFF && color > 0x00FFFFFF) return "0x" + color.ToString("X8", CultureInfo.GetCultureInfo("en-US"));
            if (color <= 0x00FFFFFF) return "0x" + color.ToString("X6", CultureInfo.GetCultureInfo("en-US"));
            return "0x" + color.ToString("X", CultureInfo.GetCultureInfo("en-US"));
        }

        public static string GetAlpha(ulong alpha) => String.Concat("0x", alpha <= 0xFF ? alpha.ToString("X2", CultureInfo.GetCultureInfo("en-US")) : alpha.ToString("X", CultureInfo.GetCultureInfo("en-US")));

        public static string DecompileSortMode(ulong sm) => (sm & 3) switch
        {
            0x0 => "0",
            0x1 => "sort_f_desc",
            0x10 => "sort_f_ci",
            0x11 => "sort_f_ci | sort_f_desc",
            _ => sm.ToString(CultureInfo.GetCultureInfo("en-US")),
        };
        

        public static bool NeedId { get; set; } = true;
        public static void GenerateId(string fileOut, IEnumerable<string> content, string prefix = "")
        {
            if (!NeedId || prefix == null || content == null) return;
            var f = new Win32FileWriter(Path.Combine(OutputPath, fileOut));
            if (prefix.Length > 0 && prefix[prefix.Length - 1] != '_') prefix += '_';
            var enumerable = content.ToArray();
            for (int i = 0; i < enumerable.Length; i++) f.WriteLine("{0}{1} = {2}", prefix, enumerable[i], i);
            f.Close();
        }
    }
}
