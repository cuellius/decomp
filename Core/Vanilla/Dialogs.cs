using DWORD = System.UInt32;

namespace Decomp.Core.Vanilla
{
    public static class Dialogs
    {
        public static void Decompile()
        {
            var fDialogs = new Text(Common.InputPath + @"\conversation.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_dialogs.py");
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Dialogs);
            fDialogs.GetString();
            int iDialogs = fDialogs.GetInt();
            for (int t = 0; t < iDialogs; t++)
            {
                fDialogs.GetWord();
                DWORD dwDialogPartner = fDialogs.GetUInt();
                int iStartingDialogState = fDialogs.GetInt();
                string strDialogPartner = "";

                string[] strRepeatsPrefix = { "repeat_for_factions", "repeat_for_parties", "repeat_for_troops", "repeat_for_100", "repeat_for_1000" };
                uint iRepeat = (dwDialogPartner & 0x00007000) >> 12;
                if (iRepeat != 0)
                {
                    strDialogPartner = strRepeatsPrefix[iRepeat - 1] + "|";
                }

                string[] strPartnerPrefix = { "plyr", "party_tpl", "auto_proceed", "multi_line" };
                int[] iPartnerPrefix = { 0x00010000, 0x00020000, 0x00040000, 0x00080000 };
                for (int i = 0; i < 4; i++)
                {
                    if ((iPartnerPrefix[i] & dwDialogPartner) != 0)
                    {
                        strDialogPartner += strPartnerPrefix[i] + "|";
                    }
                }

                DWORD dwPartner = (dwDialogPartner & 0x00000FFF);
                if (dwPartner == 0x00000FFF)
                {
                    strDialogPartner += "anyone" + "|";
                }
                else if (dwPartner != 0)
                {
                    strDialogPartner += "trp_" + Common.Troops[dwPartner] + "|";
                }

                DWORD dwOther = (dwDialogPartner & 0xFFF00000) >> 20;
                if (dwOther != 0)
                {
                    strDialogPartner += "other(trp_" + Common.Troops[dwOther] + ")|";
                }

                strDialogPartner = strDialogPartner == "" ? "0" : strDialogPartner.Remove(strDialogPartner.Length - 1, 1);
                fSource.Write("  [{0}, \"{1}\",\r\n    [", strDialogPartner, Common.DialogStates[iStartingDialogState]);

                int iRecords = fDialogs.GetInt();
                if (iRecords != 0)
                {
                    fSource.WriteLine();
                    Common.PrintStatement(ref fDialogs, ref fSource, iRecords, "      ");
                    fSource.WriteLine("    ],");
                }
                else
                    fSource.WriteLine("],");

                string strDialogText = fDialogs.GetWord();
                fSource.WriteLine("    \"{0}\",", strDialogText.Replace('_', ' '));

                int iEndingDialogState = fDialogs.GetInt();
                fSource.Write("    \"{0}\",\r\n    [", Common.DialogStates[iEndingDialogState]);

                iRecords = fDialogs.GetInt();
                if (iRecords != 0)
                {
                    fSource.WriteLine();
                    Common.PrintStatement(ref fDialogs, ref fSource, iRecords, "      ");
                    fSource.Write("    ]");
                }
                else
                    fSource.Write("]");

                fSource.WriteLine("],\r\n");
            }
            fSource.Write("]");
            fSource.Close();
            fDialogs.Close();
        }
    }
}
