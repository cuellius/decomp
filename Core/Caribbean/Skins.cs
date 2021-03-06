﻿using System;
using System.Globalization;
using System.IO;

namespace Decomp.Core.Caribbean
{
    public static class Skins
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "skins.txt"))) return Array.Empty<string>();

            var fId = new Text(Path.Combine(Common.InputPath, "skins.txt"));
            fId.GetString();
            int n = fId.GetInt();
            var aSkins = new string[n];
            for (int i = 0; i < n; i++)
            {
                aSkins[i] = fId.GetWord();

                fId.GetInt();
                fId.GetWord();
                fId.GetWord();
                fId.GetWord();
                fId.GetWord();

                int iFaceKeys = fId.GetInt();
                for (int j = 0; j < iFaceKeys; j++)
                {
                    fId.GetWord();
                    fId.GetWord();
                    fId.GetWord();
                    fId.GetWord();
                    fId.GetWord();
                    fId.GetWord();
                }

                int iMeshesHair = fId.GetInt();
                for (int j = 0; j < iMeshesHair; j++) fId.GetWord();

                int iMeshesBeard = fId.GetInt();
                for (int j = 0; j < iMeshesBeard; j++) fId.GetWord();

                for (int j = 0; j < 2; j++)
                {
                    int iTextures = fId.GetInt();
                    for (int t = 0; t < iTextures; t++) fId.GetWord();
                }

                int iTexturesFace = fId.GetInt();
                for (int j = 0; j < iTexturesFace; j++)
                {
                    fId.GetWord();
                    int iColors = fId.GetInt();
                    int iHairMats = fId.GetInt();
                    int iHairColors = fId.GetInt();
                    fId.GetWord();
                    for (int c = 0; c < iColors; c++) fId.GetWord();
                    for (int m = 0; m < iHairMats; m++) fId.GetWord();
                    for (int c = 0; c < iHairColors; c++) fId.GetWord();
                }

                int iVoices = fId.GetInt();
                for (int v = 0; v < iVoices; v++)
                {
                    fId.GetWord();
                    fId.GetWord();
                }

                fId.GetWord();
                fId.GetWord();
                fId.GetWord();
                fId.GetWord();

                int iConstraints = fId.GetInt();
                for (int j = 0; j < iConstraints; j++)
                {
                    fId.GetWord();
                    fId.GetWord();

                    int count = fId.GetInt();
                    for (int c = 0; c < count; c++)
                    {
                        fId.GetWord();
                        fId.GetWord();
                    }
                }
            }
            fId.Close();

            return aSkins;
        }

        public static void Decompile()
        {
            var fSkins = new Text(Path.Combine(Common.InputPath, "skins.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_skins.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Skins);
            fSkins.GetString();
            int iSkins = fSkins.GetInt();

            for (int s = 0; s < iSkins; s++)
            {
                fSource.WriteLine("  (\r\n    \"{0}\", {1},", fSkins.GetWord(), fSkins.GetInt());
                fSource.WriteLine("    \"{0}\", \"{1}\", \"{2}\",", fSkins.GetWord(), fSkins.GetWord(), fSkins.GetWord());
                fSource.WriteLine("    \"{0}\",\r\n    [", fSkins.GetWord());

                int iFaceKeys = fSkins.GetInt();
                for (int i = 0; i < iFaceKeys; i++)
                {
                    fSkins.GetWord();
                    double d1 = fSkins.GetDouble(), d2 = fSkins.GetDouble(), d3 = fSkins.GetDouble(), d4 = fSkins.GetDouble();
                    var strText = fSkins.GetWord();
                    fSource.WriteLine("      ({0}, {1}, {2}, {3}, \"{4}\"),", d1.ToString(CultureInfo.GetCultureInfo("en-US")), d2.ToString(CultureInfo.GetCultureInfo("en-US")),
                        d3.ToString(CultureInfo.GetCultureInfo("en-US")), d4.ToString(CultureInfo.GetCultureInfo("en-US")), strText.Replace('_', ' '));
                }
                fSource.WriteLine("    ],");

                int iMeshesHair = fSkins.GetInt();
                fSource.Write("    [");
                for (int i = 0; i < iMeshesHair; i++)
                    fSource.Write("\"{0}\"{1}", fSkins.GetWord(), i != iMeshesHair - 1 ? ", " : "");
                fSource.WriteLine("],");

                int iMeshesBeard = fSkins.GetInt();
                fSource.Write("    [");
                for (int i = 0; i < iMeshesBeard; i++)
                    fSource.Write("\"{0}\"{1}", fSkins.GetWord(), i != iMeshesBeard - 1 ? ", " : "");
                fSource.WriteLine("],");

                for (int i = 0; i < 2; i++)
                {
                    int iTextures = fSkins.GetInt();
                    fSource.Write("    [");
                    for (int t = 0; t < iTextures; t++)
                        fSource.Write("\"{0}\"{1}", fSkins.GetWord(), t != iTextures - 1 ? ", " : "");
                    fSource.WriteLine("],");
                }

                var iTexturesFace = fSkins.GetInt();
                fSource.WriteLine("    [");
                for (int i = 0; i < iTexturesFace; i++)
                {
                    fSource.Write("      (\"{0}\", ", fSkins.GetWord()); //0

                    var iColors = fSkins.GetInt(); //1
                    var iHairMats = fSkins.GetInt(); //2
                    var iHairColors = fSkins.GetInt(); //4
                    var dwColor = fSkins.GetDWord(); //3

                    fSource.Write("[");
                    for (int c = 0; c < iColors; c++) fSource.Write("0x{0:x}{1}", fSkins.GetUInt64(), c != iColors - 1 ? ", " : "");
                    fSource.Write("], ");
                    for (int m = 0; m < iHairMats; m++) fSource.Write("[\"{0}\"], ", fSkins.GetWord());
                    fSource.Write("0x{0:x}, [", dwColor);
                    for (int c = 0; c < iHairColors; c++) fSource.Write("0x{0:x}{1}", fSkins.GetUInt64(), c != iHairColors - 1 ? ", " : "");
                    fSource.WriteLine("]),");
                }
                fSource.WriteLine("    ],");

                int iVoices = fSkins.GetInt();
                fSource.Write("    [");
                for (int v = 0; v < iVoices; v++)
                {
                    var dwFlag = fSkins.GetDWord();
                    string[] strFlags = { "voice_die", "voice_hit", "voice_grunt", "voice_grunt_long", "voice_yell", "voice_warcry", "voice_victory", "voice_stun" };
                    if (dwFlag <= 7)
                        fSource.Write("({0},", strFlags[dwFlag]);
                    else
                        fSource.Write("({0},", dwFlag);

                    var strSound = fSkins.GetWord();
                    fSource.Write(" \"{0}\"){1}", strSound, v != iVoices - 1 ? "," : "");
                }
                fSource.WriteLine("],");

                var strSkeleton = fSkins.GetWord();
                fSource.WriteLine("    \"{0}\", {1},", strSkeleton, fSkins.GetWord());

                int ixParticleSystem1 = fSkins.GetInt(),
                    ixParticleSystem2 = fSkins.GetInt();
                fSource.WriteLine("    {0}, {1},",
                    ixParticleSystem1 < Common.ParticleSystems.Count ? "psys_" + Common.ParticleSystems[ixParticleSystem1] : ixParticleSystem1.ToString(CultureInfo.GetCultureInfo("en-US")),
                    ixParticleSystem2 < Common.ParticleSystems.Count ? "psys_" + Common.ParticleSystems[ixParticleSystem2] : ixParticleSystem2.ToString(CultureInfo.GetCultureInfo("en-US")));

                var iConstraints = fSkins.GetInt();
                fSource.Write("    [");
                for (int i = 0; i < iConstraints; i++)
                {
                    var d1 = fSkins.GetDouble();
                    fSource.Write("\r\n      [{0},", d1.ToString(CultureInfo.GetCultureInfo("en-US")));

                    var i1 = fSkins.GetInt();
                    var c1 = i1 == 1 ? "comp_greater_than" : i1 == -1 ? "comp_less_than" : "0";
                    if (c1 != "0")
                        fSource.Write(" {0}, ", c1);
                    else
                        fSource.Write(" {0}, ", i1);

                    var count = fSkins.GetInt();
                    for (int c = 0; c < count; c++)
                    {
                        var dc1 = fSkins.GetDouble();
                        var ic1 = fSkins.GetInt();

                        fSource.Write("({0}, {1}){2}", dc1.ToString(CultureInfo.GetCultureInfo("en-US")), ic1, c != count - 1 ? "," : "");
                    }
                    fSource.Write("],");
                }
                fSource.WriteLine("\r\n  ]),\r\n");
            }
            fSource.Write("]");
            fSource.Close();
            fSkins.Close();
        }
    }
}
