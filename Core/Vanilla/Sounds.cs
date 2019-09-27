using System.IO;

namespace Decomp.Core.Vanilla
{
    public static class Sounds
    {
        public static void Decompile()
        {
            var fSounds = new Text(Path.Combine(Common.InputPath, "sounds.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_sounds.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Sounds);
            fSounds.GetString();
            var iSamples = fSounds.GetInt();
            var aSamples = new string[iSamples];
            for (int s = 0; s < iSamples; s++)
            {
                aSamples[s] = fSounds.GetWord();
                fSounds.GetString();
            }

            var iSounds = fSounds.GetInt();
            for (int s = 0; s < iSounds; s++)
            {
                fSource.Write("  (\"{0}\", {1},", fSounds.GetWord().Remove(0, 4), Core.Sounds.DecompileFlags(fSounds.GetDWord()));
                var iListCount = fSounds.GetInt();
                fSource.Write(" [");
                for (int l = 0; l < iListCount; l++)
                {
                    var iSample = fSounds.GetInt();
                    fSource.Write("{0}{1}", iSample < aSamples.Length ? '"' + aSamples[iSample] + '"' : iSample.ToString(), l == iListCount - 1 ? "" : ", ");
                }
                fSource.WriteLine("]),");
            }

            fSource.Write("]");
            fSource.Close();
            fSounds.Close();

            Common.GenerateId("ID_sounds.py", Common.Sounds, "snd");
        }
    }
}
