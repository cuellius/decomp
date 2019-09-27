using System.Globalization;
using System.IO;

namespace Decomp.Core
{
    public static class Postfx
    {
        public static void Decompile()
        {
            var fPostfx = new Text(Path.Combine(Common.InputPath, "postfx.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_postfx.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Postfx);
            fPostfx.GetString();
            int iPostFXs = fPostfx.GetInt();
            var postfxList = new string[iPostFXs];
            for (int i = 0; i < iPostFXs; i++)
            {
                postfxList[i] = fPostfx.GetWord().Remove(0, 4);
                fSource.Write("  (\"{0}\"", postfxList[i]);

                var dwFlag = fPostfx.GetDWord();
                if (dwFlag == 1)
                    fSource.Write(", fxf_highhdr");
                else
                    fSource.Write(", {0}", dwFlag);

                var iOpType = fPostfx.GetInt();
                fSource.Write(", {0},", iOpType);
                for (int p = 0; p < 3; p++)
                {
                    double d1 = fPostfx.GetDouble(), d2 = fPostfx.GetDouble(), d3 = fPostfx.GetDouble(), d4 = fPostfx.GetDouble();
                    fSource.Write(" [{0}, {1}, {2}, {3}]{4}", d1.ToString(CultureInfo.GetCultureInfo("en-US")), d2.ToString(CultureInfo.GetCultureInfo("en-US")), 
                        d3.ToString(CultureInfo.GetCultureInfo("en-US")), d4.ToString(CultureInfo.GetCultureInfo("en-US")), p < 2 ? "," : "");
                }
                fSource.WriteLine("),");
            }
            fSource.Write("]");
            fSource.Close();
            fPostfx.Close();

            Common.GenerateId("ID_postfx_params.py", postfxList, "pfx");
        }
    }
}
