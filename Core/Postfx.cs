using System.Globalization;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Postfx
    {
        public static void Decompile()
        {
            var fPostfx = new Text(Common.InputPath + @"\postfx.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_postfx.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Postfx);
            fPostfx.GetString();
            int iPostFXs = fPostfx.GetInt();
            for (int i = 0; i < iPostFXs; i++)
            {
                fSource.Write("  (\"{0}\"", fPostfx.GetWord().Remove(0, 4));

                DWORD dwFlag = fPostfx.GetDWord();
                if (dwFlag == 1)
                    fSource.Write(", fxf_highhdr");
                else
                    fSource.Write(", {0}", dwFlag);

                int iOpType = fPostfx.GetInt();
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
        }
    }
}
