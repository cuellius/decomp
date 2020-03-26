using System;
using System.Globalization;
using System.IO;

namespace Decomp.Core
{
    public static class Quests
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "quests.txt"))) return Array.Empty<string>();

            var fId = new Win32FileReader(Path.Combine(Common.InputPath, "quests.txt"));
            fId.ReadLine();
            int n = Convert.ToInt32(fId.ReadLine(), CultureInfo.GetCultureInfo("en-US"));
            var aQuests = new string[n];
            for (int i = 0; i < n; i++)
            {
                var str = fId.ReadLine();
                if (str != null)
                    aQuests[i] = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Remove(0, 4);
            }
            fId.Close();

            return aQuests;
        }

        public static string DecompileFlags(int iFlag) => iFlag switch
        {
            0x00000001 => "qf_show_progression",
            0x00000002 => "qf_random_quest",
            0x00000003 => "qf_show_progression|qf_random_quest",
            _ => "0",
        };

        public static void Decompile()
        {
            var fQuests = new Text(Path.Combine(Common.InputPath, "quests.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_quests.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Quests);
            fQuests.GetString();
            int iQuests = fQuests.GetInt();

            for (int iQuest = 0; iQuest < iQuests; iQuest++)
                fSource.WriteLine("  (\"{0}\", \"{1}\", {2}, \"{3}\"),", fQuests.GetWord().Remove(0, 4), fQuests.GetWord().Replace('_', ' '), DecompileFlags(fQuests.GetInt()), fQuests.GetWord().Replace('_', ' '));

            fSource.Write("]");
            fSource.Close();
            fQuests.Close();

            Common.GenerateId("ID_quests.py", Common.Quests, "qst");
        }
    }
}
