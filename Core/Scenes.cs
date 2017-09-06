using System.Globalization;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Scenes
    {
        public static string[] Initialize()
        {
            var fID = new Text(Common.InputPath + @"\scenes.txt");
            fID.GetString();
            int n = fID.GetInt();
            var aScenes = new string[n];
            for (int i = 0; i < n; i++)
            {
                aScenes[i] = fID.GetWord().Remove(0, 4);

                for (int j = 0; j < 10; j++)
                {
                    fID.GetWord();
                }

                int iPassages = fID.GetInt();
                for (int j = 0; j < iPassages; j++)
                {
                    fID.GetWord();
                }

                int iChestTroops = fID.GetInt();
                for (int j = 0; j < iChestTroops; j++)
                {
                    fID.GetWord();
                }

                fID.GetWord();
                //idFile.ReadLine();
                //idFile.ReadLine();
                //idFile.ReadLine();
            }
            fID.Close();

            return aScenes;
        }

        public static string DecompileFlags(DWORD dwFlag)
        {
            string strFlag = "";
            string[] strFlags = { "sf_indoors", "sf_force_skybox", "sf_generate", "sf_randomize", "sf_auto_entry_points", "sf_no_horses", "sf_muddy_water" };
            DWORD[] dwFlags = { 0x00000001, 0x00000002, 0x00000100, 0x00000200, 0x00000400, 0x00000800, 0x00001000 };
            
            for (int i = 0; i < dwFlags.Length; i++)
            {
                if ((dwFlag & dwFlags[i]) != 0)
                    strFlag += strFlags[i] + "|";
            }

            strFlag = strFlag == "" ? "0" : strFlag.Remove(strFlag.Length - 1, 1);

            return strFlag;
        }

        public static void Decompile()
        {
            var fScenes = new Text(Common.InputPath + @"\scenes.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_scenes.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Scenes);
            fScenes.GetString();
            int iScenes = fScenes.GetInt();

            for (int iS = 0; iS < iScenes; iS++)
            {
                fScenes.GetWord();
                fSource.Write(" (\"{0}\"", fScenes.GetWord());

                DWORD dwFlag = fScenes.GetDWord();
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
                        fSource.Write("\"{0}\"", Common.Scenes[iScene]);
                    if (i < (iPassages - 1))
                        fSource.Write(", ");
                }
                fSource.Write("], [");

                int iChestTroops = fScenes.GetInt();

                for (int i = 0; i < iChestTroops; i++)
                {
                    int iTroop = fScenes.GetInt();
                    if (iTroop < Common.Troops.Length)
                        fSource.Write("\"{0}\"", Common.Troops[iTroop]);
                    else
                        fSource.Write("{0}", iTroop);
                    if (i < (iChestTroops - 1))
                        fSource.Write(", ");
                }
                fSource.Write("]");

                string strTerrain = fScenes.GetWord();
                if (strTerrain != "0") 
                    fSource.Write(", \"{0}\"", strTerrain);

                fSource.WriteLine("),");
            }
            fSource.Write("]");
            fSource.Close();
            fScenes.Close();
        }
    }
}
