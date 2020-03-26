using System;
using System.Globalization;
using System.IO;
using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Skills
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "skills.txt"))) return Array.Empty<string>();

            var fId = new Win32FileReader(Path.Combine(Common.InputPath, "skills.txt"));
            int n = Convert.ToInt32(fId.ReadLine(), CultureInfo.GetCultureInfo("en-US"));
            var aSkills = new string[n];
            for (int i = 0; i < n; i++)
            {
                var str = fId.ReadLine();
                if (str != null) aSkills[i] = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Remove(0, 4);
            }
            fId.Close();

            return aSkills;
        }

        public static string DecompileFlags(DWORD dwFlag)
        {
            var sbFlag = new StringBuilder(32);
            DWORD dwBaseSkl = dwFlag & 0xF;
            switch (dwBaseSkl)
            {
                case 0:
                    sbFlag.Append("sf_base_att_str");
                    break;
                case 1:
                    sbFlag.Append("sf_base_att_agi");
                    break;
                case 2:
                    sbFlag.Append("sf_base_att_int");
                    break;
                case 3:
                    sbFlag.Append("sf_base_att_cha");
                    break;
            }
            if ((dwFlag & 0x10) != 0) sbFlag.Append("|sf_effects_party");
            if ((dwFlag & 0x100) != 0) sbFlag.Append("|sf_inactive");

            return sbFlag.ToString();
        }

        public static void Decompile()
        {
            var fSkills = new Text(Path.Combine(Common.InputPath, "skills.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_skills.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Skills);
            int iSkills = fSkills.GetInt();

            for (int s = 0; s < iSkills; s++)
            {
                fSource.WriteLine("  (\"{0}\", \"{1}\", {2}, {3}, \"{4}\"),", fSkills.GetWord().Remove(0, 4), fSkills.GetWord().Replace('_', ' '),
                    DecompileFlags(fSkills.GetDWord()), fSkills.GetInt(), fSkills.GetWord().Replace('_', ' '));
            }
            fSource.Write("]");
            fSource.Close();
            fSkills.Close();

            Common.GenerateId("ID_skills.py", Common.Skills, "skl");
        }
    }
}
