using System;
using System.Globalization;
using System.IO;
using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Sounds
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "sounds.txt"))) return Array.Empty<string>();

            var fId = new Text(Path.Combine(Common.InputPath, "sounds.txt"));
            fId.GetString();
            int n = fId.GetInt();
            var aSounds = new string[n];

            for (int i = 0; i < n; i++)
            {
                fId.GetWord();
                fId.GetWord();
            }

            n = fId.GetInt();
            for (int i = 0; i < n; i++)
            {
                var strId = fId.GetWord();
                aSounds[i] = strId.Remove(0, 4);
                fId.GetString();
            }
            fId.Close();

            return aSounds;
        }

        public static string DecompileFlags(DWORD dwFlag)
        {
            var sbFlag = new StringBuilder(128);
            if ((dwFlag & 0x00100000) != 0)
            {
                sbFlag.Append("sf_always_send_via_network|");
                dwFlag ^= 0x00100000;
            }

            DWORD dwPriority = (dwFlag & 0xF0) >> 4;
            DWORD dwVol = (dwFlag & 0xF00) >> 8;

            string[] strFlags = { "sf_2d", "sf_looping", "sf_start_at_random_pos", "sf_stream_from_hd" };
		    DWORD[] dwFlags = { 1, 2, 4, 8 };
		    for (int i = 0; i < dwFlags.Length; i++)
		    {
		        if ((dwFlag & dwFlags[i]) == 0) continue;
		        sbFlag.Append(strFlags[i]);
		        sbFlag.Append('|');
		    }

            //priority:
            if (dwPriority != 0) sbFlag.Append("sf_priority_" + dwPriority + "|");
            if (dwVol != 0) sbFlag.Append("sf_vol_" + dwVol + "|");

            if (sbFlag.Length == 0)
                sbFlag.Append('0');
            else
                sbFlag.Length--;

            return sbFlag.ToString();
        }

        public static void Decompile()
        {
            var fSounds = new Text(Path.Combine(Common.InputPath, "sounds.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_sounds.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Sounds);
            fSounds.GetString();
            int iSamples = fSounds.GetInt();
            var aSamples = new string[iSamples];
            for (int s = 0; s < iSamples; s++)
            {
                aSamples[s] = fSounds.GetWord();
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
                    fSource.Write("{0}{1}", iSample < aSamples.Length ? '"' + aSamples[iSample] + '"' : iSample.ToString(CultureInfo.GetCultureInfo("en-US")), l == iListCount - 1 ? "" : ", ");
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
