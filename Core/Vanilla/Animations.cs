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
            var fId = new StreamReader(strFileName);
            int n = Convert.ToInt32(fId.ReadLine());
            var aAnimations = new string[n];
            for (int i = 0; i < n; i++)
            {
                var animation = fId.ReadLine();
                if (animation == null)
                    continue;

                aAnimations[i] = animation.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

                int j = Convert.ToInt32(animation.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[2]);
                while (j != 0)
                {
                    fId.ReadLine();
                    j--;
                }
            }
            fId.Close();

            return aAnimations;
        }

        public static void Decompile()
        {
            var fActions = new Text(Path.Combine(Common.InputPath, "actions.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_animations.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Animations);
            var iActions = fActions.GetInt();
            for (int a = 0; a < iActions; a++)
            {
                var strAnimId = fActions.GetWord();
                var dwAnimFlags = fActions.GetDWord();
                fSource.WriteLine("  [\"{0}\", {1},", strAnimId, Core.Animations.DecompileFlags(dwAnimFlags));
                var iAnimSequences = fActions.GetInt();
                for (int s = 0; s < iAnimSequences; s++)
                {
                    var dDuration = fActions.GetDouble();
                    var strName = fActions.GetWord();
                    fSource.Write("    [{0}, \"{1}\",", dDuration.ToString(CultureInfo.GetCultureInfo("en-US")), strName);
                    int iBeginFrame = fActions.GetInt(), iEndingFrame = fActions.GetInt();
                    var dwSequenceFlags = fActions.GetDWord();

                    var dd = new string[5]; //NOTE: Type string for non-english version of windows
                    var bZeroes = true;
                    for (int d = 0; d < 5; d++)
                    {
                        dd[d] = fActions.GetDouble().ToString(CultureInfo.GetCultureInfo("en-US"));
                        if (dd[d] != "0") bZeroes = false;
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

            Common.GenerateId("ID_animations.py", Common.Animations, "anim");
        }
    }
}
