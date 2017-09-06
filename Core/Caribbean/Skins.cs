using System.Globalization;
using DWORD = System.UInt32;

namespace Decomp.Core.Caribbean
{
    public static class Skins
    {
        public static void Decompile()
        {
            var fSkins = new Text(Common.InputPath + @"\skins.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_skins.py");
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
                    string strText = fSkins.GetWord();
                    fSource.WriteLine("      ({0}, {1}, {2}, {3}, \"{4}\"),", d1.ToString(CultureInfo.GetCultureInfo("en-US")), d2.ToString(CultureInfo.GetCultureInfo("en-US")),
                        d3.ToString(CultureInfo.GetCultureInfo("en-US")), d4.ToString(CultureInfo.GetCultureInfo("en-US")), strText.Replace('_', ' '));
                }
                fSource.WriteLine("    ],");

                int iMeshesHair = fSkins.GetInt();
                fSource.Write("    [");
                for (int i = 0; i < iMeshesHair; i++)
                {
                    fSource.Write("\"{0}\"{1}", fSkins.GetWord(), i != iMeshesHair - 1 ? ", " : "");
                }
                fSource.WriteLine("],");

                int iMeshesBeard = fSkins.GetInt();
                fSource.Write("    [");
                for (int i = 0; i < iMeshesBeard; i++)
                {
                    fSource.Write("\"{0}\"{1}", fSkins.GetWord(), i != iMeshesBeard - 1 ? ", " : "");
                }
                fSource.WriteLine("],");

                for (int i = 0; i < 2; i++)
                {
                    int iTextures = fSkins.GetInt();
                    fSource.Write("    [");
                    for (int t = 0; t < iTextures; t++)
                    {
                        fSource.Write("\"{0}\"{1}", fSkins.GetWord(), t != iTextures - 1 ? ", " : "");
                    }
                    fSource.WriteLine("],");
                }

                int iTexturesFace = fSkins.GetInt();
                fSource.WriteLine("    [");
                for (int i = 0; i < iTexturesFace; i++)
                {
                    //("manface_young_2", 0xFFCBE0E0, ["hair_blonde"], [0xffffffff, 0xffb04717, 0xff502a19]),

                    fSource.Write("      (\"{0}\", 0x{1:X}, ", fSkins.GetWord(), fSkins.GetDWord());
                    fSkins.GetWord();
                    int iHairColors = fSkins.GetInt();
                    fSource.Write("{0:x8}, {1:x8}, {2:x8}, {3:x8}, ", fSkins.GetInt64(), fSkins.GetInt64(), fSkins.GetInt64(), fSkins.GetInt64());
                    fSource.Write("[\"{0}\"], ", fSkins.GetWord());
                    for (int m = 0; m < iHairColors; m++)
                    {
                        fSource.Write("{0:x8}", fSkins.GetInt64());
                        if (m != iHairColors - 1)
                        {
                            fSource.Write(", ");
                        }
                    }
                    fSource.WriteLine("),");
                }
                fSource.WriteLine("    ],");

                int iVoices = fSkins.GetInt();
                fSource.Write("    [");
                for (int v = 0; v < iVoices; v++)
                {
                    DWORD dwFlag = fSkins.GetDWord();
                    string[] strFlags = { "voice_die", "voice_hit", "voice_grunt", "voice_grunt_long", "voice_yell", "voice_warcry", "voice_victory", "voice_stun" };
                    if (dwFlag <= 7)
                        fSource.Write("({0},", strFlags[dwFlag]);
                    else
                        fSource.Write("({0},", dwFlag);

                    string strSound = fSkins.GetWord();
                    fSource.Write(" \"{0}\"){1}", strSound, v != iVoices - 1 ? "," : "");
                }
                fSource.WriteLine("],");

                string strSkeleton = fSkins.GetWord();
                fSource.WriteLine("    \"{0}\", {1},", strSkeleton, fSkins.GetWord());

                int ixParticleSystem1 = fSkins.GetInt(),
                    ixParticleSystem2 = fSkins.GetInt();
                fSource.WriteLine("    psys_{0}, psys_{1},", Common.ParticleSystems[ixParticleSystem1], Common.ParticleSystems[ixParticleSystem2]);

                int iConstraints = fSkins.GetInt();
                fSource.Write("    [");
                for (int i = 0; i < iConstraints; i++)
                {
                    double d1 = fSkins.GetDouble();
                    fSource.Write("\r\n      [{0},", d1.ToString(CultureInfo.GetCultureInfo("en-US")));

                    int i1 = fSkins.GetInt();
                    string c1 = i1 == 1 ? "comp_greater_than" : i1 == -1 ? "comp_less_than" : "0";
                    if (c1 != "0")
                        fSource.Write(" {0}, ", c1);
                    else
                        fSource.Write(" {0}, ", i1);

                    int count = fSkins.GetInt();
                    for (int c = 0; c < count; c++)
                    {
                        double dc1 = fSkins.GetDouble();
                        int ic1 = fSkins.GetInt();

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
