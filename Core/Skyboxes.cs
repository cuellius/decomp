using System.Globalization;
using System.IO;
using System.Text;
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

            var sbFlag = new StringBuilder(32);
            
            switch (dwTime)
            {
                case 3:
                    sbFlag.Append("sf_mask|");
                    break;
                case 2:
                    sbFlag.Append("sf_night|");
                    break;
                case 1:
                    sbFlag.Append("sf_dawn|");
                    break;
                case 0:
                    sbFlag.Append("sf_day|");
                    break;
            }

            sbFlag.Append("sf_clouds_" + dwClouds + "|");

            switch (dwShadow)
            {
                case 0x10000000:
                    sbFlag.Append("sf_no_shadows|"); 
                    break;
                case 0x20000000:
                    sbFlag.Append("sf_HDR|");
                    break;
                case 0x30000000:
                    sbFlag.Append("sf_no_shadows|sf_HDR|");
                    break;
            }

            if (sbFlag.Length == 0) sbFlag.Append('0'); else sbFlag.Length--;

            return sbFlag.ToString();
        }

        public static void Decompile()
        {
            var fSkyboxes = new Text(Path.Combine(Common.InputPath, "skyboxes.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_skyboxes.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Skyboxes);

            int iSkyboxes = fSkyboxes.GetInt();
            for (int i = 0; i < iSkyboxes; i++)
            {
                string strId = fSkyboxes.GetWord();
                DWORD dwFlags = fSkyboxes.GetDWord();
                fSource.Write("  (\"{0}\", {1},", strId, DecompileFlags(dwFlags));

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
