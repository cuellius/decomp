using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class SceneProps
    {
        public static string[] Initialize()
        {
            var fID = new Text(Common.InputPath + @"\scene_props.txt");
            fID.GetString();
            int n = fID.GetInt();
            var aSceneProps = new string[n];
            for (int i = 0; i < n; i++)
            {
                aSceneProps[i] = fID.GetWord().Remove(0, 4);

                fID.GetWord();
                fID.GetWord();
                fID.GetWord();
                fID.GetWord();

                var iTriggers = fID.GetInt();

                //idSceneProps[i - 1] = sceneprop.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Remove(0, 4);
                //var numTriggers = Convert.ToInt32(sceneprop.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[5]);

                while (iTriggers != 0)
                {
                    fID.GetWord();

                    int iRecords = fID.GetInt();
                    if (iRecords != 0)
                    {
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
                    iTriggers--;
                }

                //idFile.ReadLine();
                //idFile.ReadLine();
            }
            fID.Close();

            return aSceneProps;
        }

        public static string DecompileFlags(DWORD dwFlag)
        {
            var sbFlag = new StringBuilder(2048);

            DWORD iHitPoints = (dwFlag >> 20) & 0xFF;
            if (iHitPoints != 0)
                sbFlag.Append("spr_hit_points(" + iHitPoints + ")|");
            DWORD iUseTime = (dwFlag >> 28) & 0xFF;
            if (iUseTime != 0)
                sbFlag.Append("spr_use_time(" + iUseTime + ")|");

            //dwFlag = dwFlag - iHitPoints - iUseTime;

            //FIX by K700+
            string[] strFlags = { "sokf_type_player_limiter", "sokf_type_barrier3d", "sokf_type_ladder",  "sokf_type_barrier_leave","sokf_type_barrier", "sokf_type_ai_limiter",
			"sokf_type_container", "sokf_add_fire", "sokf_add_smoke", "sokf_add_light", "sokf_show_hit_point_bar", "sokf_place_at_origin",
			"sokf_dynamic", "sokf_invisible", "sokf_destructible", "sokf_moveable", "sokf_face_player", "sokf_dynamic_physics", "sokf_missiles_not_attached",
            "sokf_enforce_shadows","sokf_dont_move_agent_over","sokf_handle_as_flora","sokf_static_movement", "sokf_weapon_knock_back_collision" };
            DWORD[] dwFlags = { 0x0000000d, 0x0000000c,  0x0000000b, 0x0000000a, 0x00000009, 0x00000008, 0x00000005, 0x00000100, 0x00000200, 0x00000400,
			0x00000800, 0x00001000, 0x00002000, 0x00004000, 0x00008000, 0x00010000, 0x00020000, 0x00040000, 0x00080000,
			0x00100000, 0x00200000, 0x01000000, 0x02000000, 0x10000000 };
            //FIX by K700-

            for (int i = 0; i < dwFlags.Length; i++)
            {
                DWORD temp = dwFlag & dwFlags[i];
                if (temp - dwFlags[i] == 0)
                {
                    dwFlag ^= dwFlags[i];
                    sbFlag.Append(strFlags[i]);
                    sbFlag.Append('|');
                }
            }

            //strFlag = strFlag == "" ? "0" : strFlag.Remove(strFlag.Length - 1, 1);
            if (sbFlag.Length == 0)
                sbFlag.Append('0');
            else 
                sbFlag.Length--;

            return sbFlag.ToString();
        }

        public static void Decompile()
        {
            var fSceneProps = new Text(Common.InputPath + @"\scene_props.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_scene_props.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.SceneProps);
            fSceneProps.GetString();
            int iSceneProps = fSceneProps.GetInt();

            for (int i = 0; i < iSceneProps; i++)
            {
                string strID = fSceneProps.GetWord();
                DWORD dwFlag = fSceneProps.GetUInt();
                fSceneProps.GetInt();
                fSource.Write("  (\"{0}\", {1}, \"{2}\", \"{3}\", [", strID.Remove(0, 4), DecompileFlags(dwFlag), fSceneProps.GetWord(), fSceneProps.GetWord());
                
		        int iTriggers = fSceneProps.GetInt();

                for (int t = 0; t < iTriggers; t++)
                {
                    double dInterval = fSceneProps.GetDouble();
                    fSource.Write("\r\n    ({0},[\r\n", Common.GetTriggerParam(dInterval));

                    int iRecords = fSceneProps.GetInt();
                    if (iRecords != 0)
                    {
                        Common.PrintStatement(ref fSceneProps, ref fSource, iRecords, "      ");
                    }
                    fSource.WriteLine("    ]),");
                }
                fSource.WriteLine(iTriggers > 0 ? "  ]),\r\n" : "]),\r\n");
            }
            fSource.Write("]");
            fSource.Close();
            fSceneProps.Close();
        }
    }
}
