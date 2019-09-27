using System;
using System.IO;
using System.Linq;
using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core.WSE2
{
    public static class Shaders
    {
        public static string DecompileFlags(DWORD dwFlag)
        {
            var sbFlag = new StringBuilder(256);
            string[] strFlags = { "shf_specular_enable", "shf_static_lighting", "shf_has_reflections", "shf_preshaded",
                "shf_uses_instancing", "shf_biased", "shf_always_fail", "shf_special", "shf_uses_dot3", "shf_uses_diffuse_map",
                "shf_uses_diffuse_2_map", "shf_uses_normal_map", "shf_uses_specular_map", "shf_uses_environment_map",
                "shf_uses_hlsl", "shf_uses_skinning" };
            DWORD[] dwFlags = { 0x20, 0x80, 0x100, 0x1000, 0x2000, 0x8000, 0x10000, 0x20000, 0x40000, 0x100000, 0x200000, 0x400000, 0x800000, 0x1000000, 0x20000000, 0x80000000 };

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

        public static string DecompileQuality(DWORD dwQuality)
        {
            var sbFlag = new StringBuilder(256);
            string[] strFlags = { "shrf_lo_quality", "shrf_mid_quality", "shrf_hi_quality" };
            DWORD[] dwFlags = { 0x1000, 0x2000, 0x4000 };

            for (int i = 0; i < dwFlags.Length; i++)
            {
                if ((dwQuality & dwFlags[i]) == 0) continue;
                sbFlag.Append(strFlags[i]);
                sbFlag.Append('|');
                dwQuality ^= dwFlags[i];
            }

            if (sbFlag.Length == 0)
                sbFlag.Append('0');
            else
                sbFlag.Length--;

            return sbFlag.ToString();
        }

        public static void Decompile()
        {
            var fShaders = new Win32BinaryFileReader(Path.Combine(Common.InputPath, "core_shaders.brf"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "resource_shaders.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.ImprovedShaders);

            // 6 + 4 = 10 for "rfver "
            // 4 for 1 (__int32)
            // 6 + 4 = 10 for "shader"
            fShaders.SkipBytes(24);
            var iShaders = fShaders.ReadInt32();

            for (int s = 0; s < iShaders; s++)
            {
                var name = fShaders.ReadAsciiString();
                var flags = fShaders.ReadUInt32();
                var quality = fShaders.ReadUInt32();
                var technique = fShaders.ReadAsciiString();
                var technique2 = fShaders.ReadAsciiString();

                var iAlternatives = fShaders.ReadInt32();
                var alternatives = new string[iAlternatives];
                for (int i = 0; i < iAlternatives; i++) alternatives[i] = fShaders.ReadAsciiString();

                fShaders.ReadInt32(); //FUCKING MAGIC!

                fSource.WriteLine("  (\"{0}\", \"{1}\", \"{2}\", {3}, {4}, [{5}]),", name, technique, technique2, 
                    DecompileFlags(flags), DecompileQuality(quality), String.Join(",", alternatives.Select(j => '"' + j + '"')));
            }


            fSource.Write("]");
            fSource.Close();
            fShaders.Close();
        }
    }
}
