using System;
using System.Globalization;
using System.IO;
using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class ParticleSystems
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "particle_systems.txt"))) return Array.Empty<string>();

            var fId = new Text(Path.Combine(Common.InputPath, "particle_systems.txt"));
            fId.GetString();
            var n = Convert.ToInt32(fId.GetString(), CultureInfo.GetCultureInfo("en-US"));
            var aParticleSystems = new string[n];
            for (int i = 0; i < n; i++)
            {
                aParticleSystems[i] = fId.GetWord().Remove(0, 5);
                for (int j = 0; j < 37; j++) fId.GetWord();
            }
            fId.Close();

            return aParticleSystems;
        }

        public static string DecompileFlags(DWORD dwFlag)
        {
            var sbFlag = new StringBuilder(256);

            string[] strFlags = { "psf_forced", "psf_always_emit", "psf_global_emit_dir", "psf_emit_at_water_level", "psf_billboard_2d", "psf_billboard_3d", 
            "psf_billboard_drop", "psf_turn_to_velocity", "psf_randomize_rotation", "psf_randomize_size", "psf_2d_turbulance", "psf_next_effect_is_lod" };
            DWORD[] dwFlags = { 0x0000000001, 0x0000000002, 0x0000000010, 0x0000000020, 0x0000000100, 0x0000000200, 0x0000000300, 0x0000000400, 0x0000001000, 
            0x0000002000, 0x0000010000, 0x0000020000 };

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
            var fParticles = new Text(Path.Combine(Common.InputPath, "particle_systems.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_particle_systems.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.ParticleSystems);
            fParticles.GetString();
            int iParticles = fParticles.GetInt();
            for (int i = 0; i < iParticles; i++)
            {
                fSource.Write("  (\"{0}\", ", fParticles.GetWord().Remove(0, 5));

                DWORD dwFlag = fParticles.GetDWord();
                fSource.Write("{0}, \"{1}\",\r\n   ", DecompileFlags(dwFlag), fParticles.GetWord());
                for (int j = 0; j < 6; j++) fSource.Write(" {0},", fParticles.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US")));
                fSource.WriteLine();
                
                double d0, d1;
                for (int j = 0; j < 5; j++)
                {
                    d0 = fParticles.GetDouble(); d1 = fParticles.GetDouble();
                    fSource.Write("    ({0}, {1}),", d0.ToString(CultureInfo.GetCultureInfo("en-US")), d1.ToString(CultureInfo.GetCultureInfo("en-US")));
                    d0 = fParticles.GetDouble(); d1 = fParticles.GetDouble();
                    fSource.WriteLine(" ({0}, {1}),", d0.ToString(CultureInfo.GetCultureInfo("en-US")), d1.ToString(CultureInfo.GetCultureInfo("en-US")));
                }

                d0 = fParticles.GetDouble(); d1 = fParticles.GetDouble(); var d2 = fParticles.GetDouble();
                fSource.WriteLine("    ({0}, {1}, {2}),", d0.ToString(CultureInfo.GetCultureInfo("en-US")), d1.ToString(CultureInfo.GetCultureInfo("en-US")), d2.ToString(CultureInfo.GetCultureInfo("en-US")));

                d0 = fParticles.GetDouble(); d1 = fParticles.GetDouble(); d2 = fParticles.GetDouble();
                fSource.WriteLine("    ({0}, {1}, {2}),", d0.ToString(CultureInfo.GetCultureInfo("en-US")), d1.ToString(CultureInfo.GetCultureInfo("en-US")), d2.ToString(CultureInfo.GetCultureInfo("en-US")));

                d0 = fParticles.GetDouble(); d1 = fParticles.GetDouble(); d2 = fParticles.GetDouble();
                fSource.WriteLine("    {0},\r\n    {1}, {2}\r\n  ),\r\n", d0.ToString(CultureInfo.GetCultureInfo("en-US")), d1.ToString(CultureInfo.GetCultureInfo("en-US")), d2.ToString(CultureInfo.GetCultureInfo("en-US")));
            }
            fSource.Write("]");
            fSource.Close();
            fParticles.Close();

            Common.GenerateId("ID_particle_systems.py", Common.ParticleSystems, "psys");
        }
    }
}
