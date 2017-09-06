using System;
using System.Globalization;
using System.IO;
using System.Linq;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class GroundSpecs
    {
        public static int GetLexemsInFile(string strFileName)
        {
            string[] strLines = File.ReadAllLines(strFileName);

            return strLines.Sum(strLine => strLine.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Length);
        }

        public static void Decompile()
        {
            var fGroundSpecs = new Text(Common.InputPath + @"\ground_specs.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_ground_specs.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.GroundSpecs);

            int n = GetLexemsInFile(Common.InputPath + @"\ground_specs.txt") / 8;

            for (int i = 0; i < n; i++)
            {
                string strID = fGroundSpecs.GetWord();
                DWORD dwFlag = fGroundSpecs.GetUInt();
                string strMaterial = fGroundSpecs.GetWord();
                double dblUVScale = fGroundSpecs.GetDouble();
                string strMultitexMaterialName = fGroundSpecs.GetWord();
                double dColor1 = fGroundSpecs.GetDouble(),
                       dColor2 = fGroundSpecs.GetDouble(),
                       dColor3 = fGroundSpecs.GetDouble();

                string strFlag = "";
                string[] strFlags = { "gtf_overlay", "gtf_dusty", "gtf_has_color" };
                DWORD[] dwFlags = { 1, 2, 4 };

                for (int j = 0; j < dwFlags.Length; j++)
                {
                    if ((dwFlag & dwFlags[j]) != 0)
                    {
                        dwFlag ^= dwFlags[j];
                        strFlag += strFlags[j] + "|";
                    }
                }

                strFlag = strFlag == "" ? "0" : strFlag.Remove(strFlag.Length - 1, 1);

                fSource.WriteLine("  (\"{0}\", {1}, \"{2}\", {3}, \"{4}\", ({5}, {6}, {7})),", strID, strFlag, strMaterial,
                    dblUVScale.ToString(CultureInfo.GetCultureInfo("en-US")), strMultitexMaterialName,
                    dColor1.ToString(CultureInfo.GetCultureInfo("en-US")), dColor2.ToString(CultureInfo.GetCultureInfo("en-US")),
                    dColor3.ToString(CultureInfo.GetCultureInfo("en-US")));
            }
            fSource.WriteLine(@"]

def write_vec(file,vec):
  file.write("" %f %f %f ""%vec)
  
def save_ground_specs():
  file = open(export_dir + ""Data/ground_specs.txt"",""w"")
  for ground_spec in ground_specs:
    file.write("" %s %d %s %f %s""%(ground_spec[0],ground_spec[1],ground_spec[2],ground_spec[3],ground_spec[4]))
    if (ground_spec[1] & gtf_has_color):
      file.write("" %f %f %f""%ground_spec[5])
    file.write(""\n"")
  file.close()

def save_c_header():
  file = open(export_dir + ""ground_spec_codes.h"",""w"")
  file.write(""#ifndef _GROUND_SPEC_CODES_H\n"")
  file.write(""#define _GROUND_SPEC_CODES_H\n\n"")
  file.write(""typedef enum {\n"")
  for ground_spec in ground_specs:
    file.write(""  ground_%s,\n""%ground_spec[0])
  file.write(""}Ground_spec_codes;\n"")
  file.write(""const int num_ground_specs = %d;\n""%(len(ground_specs)))
  file.write(""\n\n"")
  file.write(""\n#endif\n"")
  file.close()
  
def save_python_header():
  file = open(""./header_ground_types.py"",""w"")
  for ig in xrange(len(ground_specs)):
    ground_spec = ground_specs[ig]
    file.write(""ground_%s = %d\n""%(ground_spec[0], ig))
  file.write(""\n\n"")
  file.close()

print ""Exporting ground_spec data...""
save_ground_specs()
save_c_header()
save_python_header()");
            fSource.Close();
            fGroundSpecs.Close();
        }
    }
}
