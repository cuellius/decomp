using System.Collections.Generic;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Sounds
    {
        public static string[] Initialize()
        {
            var fID = new Text(Common.InputPath + @"\sounds.txt");
            fID.GetString();
            int n = fID.GetInt();
            var aSounds = new string[n];

            for (int i = 0; i < n; i++)
            {
                fID.GetWord();
                fID.GetWord();
            }

            n = fID.GetInt();
            for (int i = 0; i < n; i++)
            {
                string strID = fID.GetWord();
                aSounds[i] = strID.Remove(0, 4);
                fID.GetString();
                //int iListCount = fID.GetInt();
                //for (int l = 0; l < iListCount; l++)
                //{
                //    fID.GetWord();
                //    fID.GetWord();
                //}
            }
            fID.Close();

            return aSounds;
        }

        public static string DecompileFlags(DWORD dwFlag)
        {
            string strFlag = "";
            if ((dwFlag & 0x00100000) != 0)
            {
                strFlag = "sf_always_send_via_network|";
                dwFlag ^= 0x00100000;
            }

            DWORD dwPriority = (dwFlag & 0xF0) >> 4;
            DWORD dwVol = (dwFlag & 0xF00) >> 8;

            string[] strFlags = { "sf_2d", "sf_looping", "sf_start_at_random_pos", "sf_stream_from_hd" };
		    DWORD[] dwFlags = { 1, 2, 4, 8 };
		    for (int i = 0; i < dwFlags.Length; i++)
		    {
			    if((dwFlag & dwFlags[i]) != 0)
			    {
                    strFlag += strFlags[i] + "|";
			    }
		    }

            //priority:
            if (dwPriority != 0) strFlag = strFlag + "sf_priority_" + dwPriority + "|";
            if (dwVol != 0) strFlag = strFlag + "sf_vol_" + dwVol + "|";

            strFlag = strFlag == "" ? "0" : strFlag.Remove(strFlag.Length - 1, 1);

            return strFlag;
        }

        public static void Decompile()
        {
            var fSounds = new Text(Common.InputPath + @"\sounds.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_sounds.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Sounds);
            fSounds.GetString();
            int iSamples = fSounds.GetInt();
            var aSamples = new List<string>();
            for (int s = 0; s < iSamples; s++)
            {
                aSamples.Add(fSounds.GetWord());
                fSounds.GetString();
            }

            int iSounds = fSounds.GetInt();
            for (int s = 0; s < iSounds; s++)
            {
                fSource.Write("  (\"{0}\", {1},", fSounds.GetWord().Remove(0, 4), DecompileFlags(fSounds.GetDWord()));
                int iListCount = fSounds.GetInt();
                fSource.Write(" [");
                for (int l = 0; l < iListCount; l++)
                {
                    int iSample = fSounds.GetInt();
                    fSounds.GetInt();
                    fSource.Write("\"{0}\"{1}", aSamples[iSample], l == iListCount - 1 ? "" : ", ");
                }
                fSource.WriteLine("]),");
            }

            fSource.Write("]");
            fSource.Close();
            fSounds.Close();

            Common.GenerateId("ID_sounds.py", Common.Sounds, "snd");
        }
    }
}
