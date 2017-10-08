using System;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Menus
    {
        public static string[] Initialize()
        {
            var fID = new Text(Common.InputPath + @"\menus.txt");
            fID.GetString();
            int n = Convert.ToInt32(fID.GetString());
            var aMenus = new string[n];
            for (int i = 0; i < n; i++)
            {
                string strID = fID.GetWord();
                aMenus[i] = strID.Remove(0, 5);

                fID.GetWord();
                fID.GetWord();
                fID.GetWord();

                int iRecords = fID.GetInt();
                if (iRecords != 0)
                {
                    for (int r = 0; r < iRecords; r++)
                    {
                        fID.GetWord();
                        int iParams = fID.GetInt();
                        for (int p = 0; p < iParams; p++)
                        {
                            fID.GetWord();
                        }
                    }
                }

                int iMenuOptions = fID.GetInt();
                for (int j = 0; j < iMenuOptions; j++)
                {
                    fID.GetWord();
                    iRecords = fID.GetInt();
                    if (iRecords != 0)
                    {
                        for (int r = 0; r < iRecords; r++)
                        {
                            fID.GetWord();
                            int iParams = fID.GetInt();
                            for (int p = 0; p < iParams; p++)
                            {
                                fID.GetWord();
                            }
                        }
                    }

                    fID.GetWord();

                    iRecords = fID.GetInt();
                    if (iRecords != 0)
                    {
                        for (int r = 0; r < iRecords; r++)
                        {
                            fID.GetWord();
                            int iParams = fID.GetInt();
                            for (int p = 0; p < iParams; p++)
                            {
                                fID.GetWord();
                            }
                        }
                    }

                    fID.GetWord();
                }


                //idFile.ReadLine();
            }
            fID.Close();

            return aMenus;
        }

        public static string DecompileFlags(ulong lFlag)
        {
            string strFlag = "";
            var dwMenuFlag = (DWORD)(lFlag & 0x00000000FFFFFFFF);
            var dwMenuColor = (DWORD)(lFlag >> 32);
            if (dwMenuColor != 0)
                strFlag = $"menu_text_color(0x{dwMenuColor:X8})";

            string[] strMenuFlags = { "mnf_join_battle", "mnf_auto_enter", "mnf_enable_hot_keys", "mnf_disable_all_keys", "mnf_scale_picture" };
            DWORD[] dwMenuFlags = { 0x00000001, 0x00000010, 0x00000100, 0x00000200, 0x00001000 };
            for (int f = 0; f < 5; f++)
            {
                if ((dwMenuFlag & dwMenuFlags[f]) != 0)
                {
                    if (strFlag != "")
                        strFlag += "|";
                    strFlag += strMenuFlags[f];
                }
            }

            return strFlag == "" ? "0" : strFlag;
        }

        public static void Decompile()
        {
            var fMenus = new Text(Common.InputPath + @"\menus.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_game_menus.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Menus);
            fMenus.GetString();
            int iMenus = fMenus.GetInt();
            for (int m = 0; m < iMenus; m++)
            {
                string strMenuID = fMenus.GetWord();
                fSource.Write("  (\"{0}\",", strMenuID.Remove(0, 5));

                ulong lMenuFlags = fMenus.GetUInt64();
                fSource.WriteLine(" {0},", DecompileFlags(lMenuFlags));

                string strMenuText = fMenus.GetWord();
                fSource.WriteLine("    \"{0}\",", strMenuText.Replace('_', ' '));

                fSource.WriteLine("    \"{0}\",", fMenus.GetWord());

                int iRecords = fMenus.GetInt();
                if (iRecords != 0)
                {
                    fSource.WriteLine("    [");
                    Common.PrintStatement(ref fMenus, ref fSource, iRecords, "      ");
                    fSource.WriteLine("    ],");
                }
                else
                    fSource.WriteLine("    [],");

                int iMenuOptions = fMenus.GetInt();

                fSource.WriteLine("    [");
                for (int i = 0; i < iMenuOptions; i++)
                {
                    string szMenuOption = fMenus.GetWord();
                    fSource.WriteLine("      (\"{0}\",", szMenuOption.Remove(0, 4));

                    iRecords = fMenus.GetInt();
                    if (iRecords != 0)
                    {
                        fSource.WriteLine("      [");
                        Common.PrintStatement(ref fMenus, ref fSource, iRecords, "        ");
                        fSource.WriteLine("      ],");
                    }
                    else
                        fSource.WriteLine("      [],");

                    string strMenuOptionText = fMenus.GetWord();
                    fSource.WriteLine("      \"{0}\",", strMenuOptionText);

                    iRecords = fMenus.GetInt();
                    if (iRecords != 0)
                    {
                        fSource.WriteLine("      [");
                        Common.PrintStatement(ref fMenus, ref fSource, iRecords, "        ");
                        fSource.WriteLine("      ]");
                    }
                    else
                        fSource.WriteLine("      []");

                    string strDoorName = fMenus.GetWord();
                    if (strDoorName != ".")
                    {
                        fSource.WriteLine(",\r\n      \"{0}\"", strDoorName);
                    }
                    fSource.Write("      ),\r\n");

                    if ((iMenuOptions - i - 1) != 0)
                        fSource.WriteLine();
                }
                fSource.WriteLine("    ],");

                if (iMenuOptions == 0)
                    fSource.WriteLine("    [],");

                fSource.WriteLine("  ),\r\n");
            }
            fSource.Write("]");
            fSource.Close();
            fMenus.Close();

            Common.GenerateId("ID_menus.py", Common.Menus, "menu");
        }
    }
}