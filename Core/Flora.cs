using System.IO;
using System.Text;
using DWORD = System.UInt32;
using DWORD64 = System.UInt64;

namespace Decomp.Core
{
    public static class Flora
    {
        public static bool IsTree(DWORD64 dwFlag) => (dwFlag & 0x00400000U) != 0;

        public static string DecompileFlags(DWORD64 dwFlag)
        {
            DWORD64 dwDensity = (dwFlag & 0xFFFF00000000) >> 32;
            dwFlag = dwFlag & 0xFFFFFFFF;

            var sbFlag = new StringBuilder(2048);

            if (dwDensity != 0) sbFlag.AppendFormat("density({0})|", dwDensity);

            string[] strFlags = { "fkf_plain", "fkf_steppe", "fkf_snow", "fkf_desert", "fkf_plain_forest", 
            "fkf_steppe_forest", "fkf_snow_forest", "fkf_desert_forest", "fkf_realtime_ligting", "fkf_point_up", "fkf_align_with_ground", 
            "fkf_grass", "fkf_on_green_ground", "fkf_rock", "fkf_tree", "fkf_snowy", "fkf_guarantee", "fkf_speedtree", "fkf_has_colony_props" };
            DWORD[] dwFlags = { 0x00000004, 0x00000008, 0x00000010, 0x00000020, 0x00000400, 0x00000800, 0x00001000, 0x00002000, 0x00010000,
            0x00020000, 0x00040000, 0x00080000, 0x00100000, 0x00200000, 0x00400000, 0x00800000, 0x01000000, 0x02000000, 0x04000000 };

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

        public static void Decompile()
        {
            var fFloraKinds = new Text(Path.Combine(Common.InputPath, "flora_kinds.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_flora_kinds.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Flora);

            var iFloraKinds = fFloraKinds.GetInt();
            for (int f = 0; f < iFloraKinds; f++)
            {
                var strId = fFloraKinds.GetWord();
                var dwFlag = fFloraKinds.GetUInt64();
                var iNumMeshes = fFloraKinds.GetInt();

                fSource.Write("  (\"{0}\", {1}, [", strId, DecompileFlags(dwFlag));

                //string strMeshesList = "";
                if (IsTree(dwFlag))
                {
                    for (int m = 0; m < iNumMeshes; m++)
                    {
                        string strMeshName = fFloraKinds.GetWord(),
                            strMeshCollision = fFloraKinds.GetWord(),
                            strAlternativeMeshName = fFloraKinds.GetWord(), //fFloraKinds.GetInt().ToString(CultureInfo.GetCultureInfo("en-US")),   
                            strAlternativeCollision = fFloraKinds.GetWord(); //fFloraKinds.GetInt().ToString(CultureInfo.GetCultureInfo("en-US")); 
                        //System.Windows.MessageBox.Show(strAlternativeMeshName, strAlternativeCollision);
                        fSource.Write("(\"{0}\", \"{1}\",(\"{2}\",\"{3}\")){4}", strMeshName, strMeshCollision,
                            strAlternativeMeshName, strAlternativeCollision, m == iNumMeshes - 1 ? "" : ",");
                    }
                }
                else
                {
                    for (int m = 0; m < iNumMeshes; m++)
                    {
                        fSource.Write("[\"{0}\", \"{1}\"]{2}", fFloraKinds.GetWord(), fFloraKinds.GetWord(), m == iNumMeshes - 1 ? "" : ",");
                    }
                }
                fSource.WriteLine("]),");
            }
            fSource.Write(@"]
def save_fauna_kinds():
  file = open(export_dir + ""Data/flora_kinds.txt"",""w"")
  file.write(""%d\n""%len(fauna_kinds))
  for fauna_kind in fauna_kinds:
    meshes_list = fauna_kind[2]
    file.write(""%s %d %d\n""%(fauna_kind[0], (dword_mask & fauna_kind[1]), len(meshes_list)))
    for m in meshes_list:
      file.write("" %s ""%(m[0]))
      if (len(m) > 1):
        file.write("" %s\n""%(m[1]))
      else:
        file.write("" 0\n"")
      if ( fauna_kind[1] & (fkf_tree|fkf_speedtree) ):  #if this fails make sure that you have entered the alternative tree definition (NOT FUNCTIONAL in Warband)
        speedtree_alternative = m[2]
        file.write("" %s %s\n""%(speedtree_alternative[0], speedtree_alternative[1]))
    if ( fauna_kind[1] & fkf_has_colony_props ):
      file.write("" %s %s\n""%(fauna_kind[3], fauna_kind[4]))
  file.close()

def two_to_pow(x):
  result = 1
  for i in xrange(x):
    result = result * 2
  return result

fauna_mask = 0x80000000000000000000000000000000
low_fauna_mask =             0x8000000000000000
def save_python_header():
  file = open(""./fauna_codes.py"",""w"")
  for i_fauna_kind in xrange(len(fauna_kinds)):
    file.write(""%s_1 = 0x""%(fauna_kinds[i_fauna_kind][0]))
    file.write(""%x\n""%(fauna_mask | two_to_pow(i_fauna_kind)))
    file.write(""%s_2 = 0x""%(fauna_kinds[i_fauna_kind][0]))
    file.write(""%x\n""%(fauna_mask | ((low_fauna_mask|two_to_pow(i_fauna_kind)) << 64)))
    file.write(""%s_3 = 0x""%(fauna_kinds[i_fauna_kind][0]))
    file.write(""%x\n""%(fauna_mask | ((low_fauna_mask|two_to_pow(i_fauna_kind)) << 64) | two_to_pow(i_fauna_kind)))
  file.close()

print ""Exporting flora data...""
save_fauna_kinds()");
            fSource.Close();
            fFloraKinds.Close();
        }
    }
}
