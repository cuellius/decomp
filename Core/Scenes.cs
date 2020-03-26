using System;
using System.Globalization;
using System.IO;
using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Scenes
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "scenes.txt"))) return Array.Empty<string>();

            var fId = new Text(Path.Combine(Common.InputPath, "scenes.txt"));
            fId.GetString();
            int n = fId.GetInt();
            var aScenes = new string[n];
            for (int i = 0; i < n; i++)
            {
                aScenes[i] = fId.GetWord().Remove(0, 4);

                for (int j = 0; j < 10; j++) fId.GetWord();

                int iPassages = fId.GetInt();
                for (int j = 0; j < iPassages; j++) fId.GetWord();

                int iChestTroops = fId.GetInt();
                for (int j = 0; j < iChestTroops; j++) fId.GetWord();

                fId.GetWord();
            }
            fId.Close();

            return aScenes;
        }

        public static string DecompileFlags(DWORD dwFlag)
        {
            var sbFlag = new StringBuilder(32);
            string[] strFlags = { "sf_indoors", "sf_force_skybox", "sf_generate", "sf_randomize", "sf_auto_entry_points", "sf_no_horses", "sf_muddy_water" };
            DWORD[] dwFlags = { 0x00000001, 0x00000002, 0x00000100, 0x00000200, 0x00000400, 0x00000800, 0x00001000 };
            
            for (int i = 0; i < dwFlags.Length; i++)
            {
                if ((dwFlag & dwFlags[i]) == 0) continue;
                sbFlag.Append(strFlags[i]);
                sbFlag.Append('|');
            }

            if (sbFlag.Length == 0)
                sbFlag.Append('0');
            else
                sbFlag.Length--;

            return sbFlag.ToString();
        }

        public static void Decompile()
        {
            var fScenes = new Text(Path.Combine(Common.InputPath, "scenes.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_scenes.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Scenes);
            fScenes.GetString();
            int iScenes = fScenes.GetInt();

            for (int iS = 0; iS < iScenes; iS++)
            {
                fScenes.GetWord();
                fSource.Write(" (\"{0}\"", fScenes.GetWord());

                var dwFlag = fScenes.GetDWord();
                fSource.Write(", {0}, \"{1}\", \"{2}\"", DecompileFlags(dwFlag), fScenes.GetWord(), fScenes.GetWord());

                double d1 = fScenes.GetDouble(), d2 = fScenes.GetDouble();
                fSource.Write(", ({0}, {1})", d1.ToString(CultureInfo.GetCultureInfo("en-US")), d2.ToString(CultureInfo.GetCultureInfo("en-US")));
                d1 = fScenes.GetDouble(); d2 = fScenes.GetDouble();
                fSource.Write(", ({0}, {1})", d1.ToString(CultureInfo.GetCultureInfo("en-US")), d2.ToString(CultureInfo.GetCultureInfo("en-US")));

                fSource.Write(", {0}, \"{1}\",[", fScenes.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US")), fScenes.GetWord());

                int iPassages = fScenes.GetInt();
                for (int i = 0; i < iPassages; i++)
                {
                    int iScene = fScenes.GetInt();
                    if (iScene == 100000)
                        fSource.Write("\"exit\"");
                    //else if (iScene == 0)
                    //    fprintf(g_fOutput, "\"\"");
                    else
                        fSource.Write("{0}", iScene < Common.Scenes.Count ? '"' + Common.Scenes[iScene] + '"' : iScene.ToString(CultureInfo.GetCultureInfo("en-US")));
                    if (i < iPassages - 1)
                        fSource.Write(", ");
                }
                fSource.Write("], [");

                int iChestTroops = fScenes.GetInt();

                for (int i = 0; i < iChestTroops; i++)
                {
                    int iTroop = fScenes.GetInt();
                    if (iTroop < Common.Troops.Count)
                        fSource.Write("\"{0}\"", Common.Troops[iTroop]);
                    else
                        fSource.Write("{0}", iTroop);
                    if (i < iChestTroops - 1)
                        fSource.Write(", ");
                }
                fSource.Write("]");

                var strTerrain = fScenes.GetWord();
                if (strTerrain != "0") 
                    fSource.Write(", \"{0}\"", strTerrain);

                fSource.WriteLine("),");
            }
            fSource.Write("]");
            fSource.Close();
            fScenes.Close();

            Common.GenerateId("ID_scenes.py", Common.Scenes, "scn");
        }
    }
}
