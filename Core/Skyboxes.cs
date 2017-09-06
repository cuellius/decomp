using System.Globalization;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Skyboxes
    {
        public static string DecompileFlags(DWORD dwFlag)
        {
            DWORD dwTime = dwFlag & 0xF;
            DWORD dwClouds = (dwFlag & 0xF0) >> 4;
            DWORD dwShadow = dwFlag & 0xF0000000;

            string strFlag = "";
            
            switch (dwTime)
            {
                case 2:
                    strFlag = "sf_night|";
                    break;
                case 1:
                    strFlag = "sf_dawn|";
                    break;
                case 0:
                    strFlag = "sf_day|";
                    break;
            }

            strFlag += "sf_clouds_" + dwClouds + "|";

            switch (dwShadow)
            {
                case 0x10000000: 
                    strFlag += "sf_no_shadows|"; 
                    break;
                case 0x20000000: 
                    strFlag += "sf_HDR|";
                    break;
                case 0x30000000:
                    strFlag += "sf_no_shadows|sf_HDR|";
                    break;
            }

            strFlag = strFlag == "" ? "0" : strFlag.Remove(strFlag.Length - 1, 1);

            return strFlag;
        }

        public static void Decompile()
        {
            var fSkyboxes = new Text(Common.InputPath + @"\skyboxes.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_skyboxes.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Skyboxes);

            int iSkyboxes = fSkyboxes.GetInt();
            for (int i = 0; i < iSkyboxes; i++)
            {
                string strID = fSkyboxes.GetWord();
                DWORD dwFlags = fSkyboxes.GetDWord();
                fSource.Write("  (\"{0}\", {1},", strID, DecompileFlags(dwFlags));

                for(int j = 0; j < 3; j++)
                    fSource.Write(" {0},", fSkyboxes.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US")));

                fSource.Write(" \"{0}\",", fSkyboxes.GetWord());

                for (int j = 0; j < 3; j++)
                    fSource.Write(" ({0}, {1}, {2}),", fSkyboxes.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US")),
                        fSkyboxes.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US")),
                        fSkyboxes.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US")));

                fSource.WriteLine(" ({0}, 0x{1:X8})),", fSkyboxes.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US")), fSkyboxes.GetDWord());
            }
            fSource.Write(@"]
def save_skyboxes():
  file = open(export_dir + ""Data/skyboxes.txt"",""w"")
  file.write(""d\n""%len(skyboxes))
  for skybox in  skyboxes:
    file.write(""%s %d %f %f %f %s\n""%(skybox[0],skybox[1],skybox[2],skybox[3],skybox[4],skybox[5]))
    file.write("" %f %f %f ""%skybox[6])
    file.write("" %f %f %f ""%skybox[7])
    file.write("" %f %f %f ""%skybox[8])
    file.write("" %f %d\n""%skybox[9])
  file.close()

print ""Exporting skyboxes...""
save_skyboxes()");
            fSource.Close();
            fSkyboxes.Close();
        }
    }
}
