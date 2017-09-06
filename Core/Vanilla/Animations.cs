using System;
using System.Globalization;
using System.IO;
using DWORD = System.UInt32;

namespace Decomp.Core.Vanilla
{
    public static class Animations
    {
        public static string[] GetIdFromFile(string strFileName)
        {
            var fID = new StreamReader(strFileName);
            int n = Convert.ToInt32(fID.ReadLine());
            var aAnimations = new string[n];
            for (int i = 0; i < n; i++)
            {
                var animation = fID.ReadLine();
                if (animation == null)
                    continue;

                aAnimations[i] = animation.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

                int j = Convert.ToInt32(animation.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[2]);
                while (j != 0)
                {
                    fID.ReadLine();
                    j--;
                }
            }
            fID.Close();

            return aAnimations;
        }

        public static void Decompile()
        {
            var fActions = new Text(Common.InputPath + @"\actions.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_animations.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Animations);
            int iActions = fActions.GetInt();
            for (int a = 0; a < iActions; a++)
            {
                string strAnimID = fActions.GetWord();
                DWORD dwAnimFlags = fActions.GetDWord();
                fSource.WriteLine("  [\"{0}\", {1},", strAnimID, Core.Animations.DecompileFlags(dwAnimFlags));
                int iAnimSequences = fActions.GetInt();
                for (int s = 0; s < iAnimSequences; s++)
                {
                    double dDuration = fActions.GetDouble();
                    string strName = fActions.GetWord();
                    fSource.Write("    [{0}, \"{1}\",", dDuration.ToString(CultureInfo.GetCultureInfo("en-US")), strName);
                    int iBeginFrame = fActions.GetInt(), iEndingFrame = fActions.GetInt();
                    DWORD dwSequenceFlags = fActions.GetDWord();

                    var dd = new string[5]; //NOTE: Type string for non-english version of windows
                    bool bZeroes = true;
                    for (int d = 0; d < 5; d++)
                    {
                        dd[d] = fActions.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US"));
                        if (dd[d] != "0")
                            bZeroes = false;
                    }
                    if (bZeroes)
                        fSource.Write(" {0}, {1}, {2}],\r\n", iBeginFrame, iEndingFrame, Core.Animations.DecompileSequenceFlags(dwSequenceFlags));
                    else
                        fSource.Write(" {0}, {1}, {2}, {3}, ({4}, {5}, {6}), {7}],\r\n", iBeginFrame, iEndingFrame,
                                Core.Animations.DecompileSequenceFlags(dwSequenceFlags), Core.Animations.DecompilePack((DWORD)Convert.ToDouble(dd[0], CultureInfo.GetCultureInfo("en-US"))), dd[1], dd[2], dd[3], dd[4]);
                }
                fSource.WriteLine("  ],");
            }
            fSource.Write("]");
            fSource.Close();
            fActions.Close();
        }
    }
}
