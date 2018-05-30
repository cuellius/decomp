using System;
using System.IO;
using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Music
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "music.txt"))) return new string[0];

            var fId = new Win32FileReader(Path.Combine(Common.InputPath, "music.txt"));
            int n = Convert.ToInt32(fId.ReadLine());
            var aMusic = new string[n];
            for (int i = 0; i < n; i++)
            {
                var str = fId.ReadLine();
                if (str == null) continue;

                aMusic[i] = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (aMusic[i].Length >= 4) aMusic[i] = aMusic[i].Remove(aMusic[i].Length - 4, 4);
            }
            fId.Close();

            return aMusic;
        }

        public static string DecompileFlags(DWORD dwFlag)
        {
            var sbFlag = new StringBuilder(256);
            string[] strMusicFlags = { "mtf_looping", "mtf_start_immediately", "mtf_persist_until_finished", "mtf_sit_tavern", "mtf_sit_fight", "mtf_sit_multiplayer_fight",
			"mtf_sit_ambushed", "mtf_sit_town", "mtf_sit_town_infiltrate", "mtf_sit_killed", "mtf_sit_travel", "mtf_sit_arena", "mtf_sit_siege", "mtf_sit_night",
			"mtf_sit_day", "mtf_sit_encounter_hostile", "mtf_sit_main_title", "mtf_sit_victorious", "mtf_sit_feast", "mtf_module_track" };
		    DWORD[] dwMusicFlags = { 0x00000040, 0x00000080, 0x00000100, 0x00000200, 0x00000400, 0x00000800, 0x00001000, 0x00002000, 0x00004000, 0x00008000, 0x00010000,
			0x00020000, 0x00040000, 0x00080000, 0x00100000, 0x00200000, 0x00400000, 0x00800000, 0x01000000, 0x10000000 };
            DWORD dwCulture = dwFlag & 0x3F;
            
            if (dwCulture == 0x3F)
            {
                sbFlag.Append("mtf_culture_all");
                dwFlag ^= 0x3F;
            }
            else if (dwCulture != 0)
            {
                for (uint t = 1, i = 1; t <= 0x20; t <<= 1, i++)
                {
                    if ((dwCulture & t) == 0) continue;
                    sbFlag.Append($"{(sbFlag.Length != 0 ? "|" : "")}mtf_culture_{i}");
                    dwFlag ^= t;
                }
            }

            for (int i = 0; i < dwMusicFlags.Length; i++)
            {
                if ((dwFlag & dwMusicFlags[i]) == 0) continue;
                if (sbFlag.Length != 0) sbFlag.Append('|');
                sbFlag.Append(strMusicFlags[i]);
                dwFlag ^= dwMusicFlags[i];
            }

            if (sbFlag.Length == 0) sbFlag.Append('0');

            return sbFlag.ToString();
        }

        public static void Decompile()
        {
            var fMusic = new Text(Path.Combine(Common.InputPath, "music.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_music.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Music);
            int iTracks = fMusic.GetInt();
            for (int t = 0; t < iTracks; t++)
            {
                string strTrack = fMusic.GetWord();
                DWORD dwTrackFlags = fMusic.GetUInt();
                DWORD dwContinueFlags = fMusic.GetUInt();
                string strTrackId = strTrack.Length >= 4 ? strTrack.Remove(strTrack.Length - 4, 4) : strTrack;
                fSource.WriteLine("  (\"{0}\", \"{1}\", {2}, {3}),", strTrackId, strTrack, DecompileFlags(dwTrackFlags), DecompileFlags(dwContinueFlags ^ dwTrackFlags));
            }
            fSource.Write("]");
            fSource.Close();
            fMusic.Close();

            Common.GenerateId("ID_music.py", Common.Music, "track");
        }
    }
}
