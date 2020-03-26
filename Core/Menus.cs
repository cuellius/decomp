using System;
using System.Globalization;
using System.IO;
using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Menus
    {
        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "menus.txt"))) return Array.Empty<string>();

            var fId = new Text(Path.Combine(Common.InputPath, "menus.txt"));
            fId.GetString();
            var n = Convert.ToInt32(fId.GetString(), CultureInfo.GetCultureInfo("en-US"));
            var aMenus = new string[n];
            for (int i = 0; i < n; i++)
            {
                string strId = fId.GetWord();
                aMenus[i] = strId.Remove(0, 5);

                fId.GetWord();
                fId.GetWord();
                fId.GetWord();

                var iRecords = fId.GetInt();
                if (iRecords != 0)
                {
                    for (int r = 0; r < iRecords; r++)
                    {
                        fId.GetWord();
                        var iParams = fId.GetInt();
                        for (int p = 0; p < iParams; p++) fId.GetWord();
                    }
                }

                var iMenuOptions = fId.GetInt();
                for (int j = 0; j < iMenuOptions; j++)
                {
                    fId.GetWord();
                    iRecords = fId.GetInt();
                    if (iRecords != 0)
                    {
                        for (int r = 0; r < iRecords; r++)
                        {
                            fId.GetWord();
                            var iParams = fId.GetInt();
                            for (int p = 0; p < iParams; p++) fId.GetWord();
                        }
                    }

                    fId.GetWord();

                    iRecords = fId.GetInt();
                    if (iRecords != 0)
                    {
                        for (int r = 0; r < iRecords; r++)
                        {
                            fId.GetWord();
                            var iParams = fId.GetInt();
                            for (int p = 0; p < iParams; p++) fId.GetWord();
                        }
                    }

                    fId.GetWord();
                }


                //idFile.ReadLine();
            }
            fId.Close();

            return aMenus;
        }

        public static string DecompileFlags(ulong lFlag)
        {
            var sbFlag = new StringBuilder(64);
            var dwMenuFlag = (DWORD)(lFlag & 0x00000000FFFFFFFF);
            var dwMenuColor = (DWORD)(lFlag >> 32);
            if (dwMenuColor != 0) sbFlag.Append($"menu_text_color(0x{dwMenuColor:X8})");

            string[] strMenuFlags = { "mnf_join_battle", "mnf_auto_enter", "mnf_enable_hot_keys", "mnf_disable_all_keys", "mnf_scale_picture" };
            DWORD[] dwMenuFlags = { 0x00000001, 0x00000010, 0x00000100, 0x00000200, 0x00001000 };
            for (int f = 0; f < 5; f++)
            {
                if ((dwMenuFlag & dwMenuFlags[f]) == 0) continue;
                if (sbFlag.Length != 0) sbFlag.Append('|');
                sbFlag.Append(strMenuFlags[f]);
            }

            return sbFlag.Length == 0 ? "0" : sbFlag.ToString();
        }

        public static void Decompile()
        {
            var fMenus = new Text(Path.Combine(Common.InputPath, "menus.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_game_menus.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Menus);
            fMenus.GetString();
            var iMenus = fMenus.GetInt();
            for (int m = 0; m < iMenus; m++)
            {
                var strMenuId = fMenus.GetWord();
                fSource.Write("  (\"{0}\",", strMenuId.Remove(0, 5));

                var lMenuFlags = fMenus.GetUInt64();
                fSource.WriteLine(" {0},", DecompileFlags(lMenuFlags));

                var strMenuText = fMenus.GetWord();
                fSource.WriteLine("    \"{0}\",", strMenuText.Replace('_', ' '));

                fSource.WriteLine("    \"{0}\",", fMenus.GetWord());

                var iRecords = fMenus.GetInt();
                if (iRecords != 0)
                {
                    fSource.WriteLine("    [");
                    Common.PrintStatement(ref fMenus, ref fSource, iRecords, "      ");
                    fSource.WriteLine("    ],");
                }
                else
                    fSource.WriteLine("    [],");

                var iMenuOptions = fMenus.GetInt();

                fSource.WriteLine("    [");
                for (int i = 0; i < iMenuOptions; i++)
                {
                    var szMenuOption = fMenus.GetWord();
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

                    var strMenuOptionText = fMenus.GetWord();
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

                    var strDoorName = fMenus.GetWord();
                    if (strDoorName != ".") fSource.WriteLine(",\r\n      \"{0}\"", strDoorName);
                    fSource.Write("      ),\r\n");

                    if (iMenuOptions - i - 1 != 0) fSource.WriteLine();
                }
                fSource.WriteLine("    ],");

                if (iMenuOptions == 0) fSource.WriteLine("    [],");

                fSource.WriteLine("  ),\r\n");
            }
            fSource.Write("]");
            fSource.Close();
            fMenus.Close();

            Common.GenerateId("ID_menus.py", Common.Menus, "menu");
        }
    }
}