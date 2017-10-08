using System;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Skills
    {
        public static string[] Initialize()
        {
            var fID = new Win32FileReader(Common.InputPath + @"\skills.txt");
            int n = Convert.ToInt32(fID.ReadLine());
            var aSkills = new string[n];
            for (int i = 0; i < n; i++)
            {
                var str = fID.ReadLine();
                if (str != null)
                    aSkills[i] = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Remove(0, 4);
            }
            fID.Close();

            return aSkills;
        }

        public static string DecompileFlags(DWORD dwFlag)
        {
            string strFlag = "";
            DWORD dwBaseSkl = dwFlag & 0xF;
            switch (dwBaseSkl)
            {
                case 0:
                    strFlag = "sf_base_att_str";
                    break;
                case 1:
                    strFlag = "sf_base_att_agi";
                    break;
                case 2:
                    strFlag = "sf_base_att_int";
                    break;
                case 3:
                    strFlag = "sf_base_att_cha";
                    break;
            }
            if ((dwFlag & 0x10) != 0)
                strFlag = strFlag + "|sf_effects_party";
            if ((dwFlag & 0x100) != 0)
                strFlag = strFlag + "|sf_inactive";

            return strFlag;
        }

        public static void Decompile()
        {
            var fSkills = new Text(Common.InputPath + @"\skills.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_skills.py");
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
