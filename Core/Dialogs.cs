using System;
using System.IO;
using System.Text;
using DWORD = System.UInt32;

namespace Decomp.Core
{
    public static class Dialogs
    {
        public static string[] Initialize() => File.Exists(Path.Combine(Common.InputPath, "dialog_states.txt")) ? Win32FileReader.ReadAllLines(Path.Combine(Common.InputPath, "dialog_states.txt")) : Array.Empty<string>();

        public static void Decompile()
        {
            var fDialogs = new Text(Path.Combine(Common.InputPath, "conversation.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_dialogs.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Dialogs);
            fDialogs.GetString();
            int iDialogs = fDialogs.GetInt();
            for (int t = 0; t < iDialogs; t++)
            {
                fDialogs.GetWord();
                DWORD dwDialogPartner = fDialogs.GetUInt();
                int iStartingDialogState = fDialogs.GetInt();
                var sbDialogPartner = new StringBuilder(256);

                string[] strRepeatsPrefix = { "repeat_for_factions", "repeat_for_parties", "repeat_for_troops", "repeat_for_100", "repeat_for_1000" };
                uint iRepeat = (dwDialogPartner & 0x00007000) >> 12;
                if (iRepeat != 0)
                {
                    sbDialogPartner.Append(strRepeatsPrefix[iRepeat - 1]);
                    sbDialogPartner.Append('|');
                }

                string[] strPartnerPrefix = { "plyr", "party_tpl", "auto_proceed", "multi_line" };
                int[] iPartnerPrefix = { 0x00010000, 0x00020000, 0x00040000, 0x00080000 };
                for (int i = 0; i < 4; i++)
                {
                    if ((iPartnerPrefix[i] & dwDialogPartner) == 0) continue;
                    sbDialogPartner.Append(strPartnerPrefix[i]);
                    sbDialogPartner.Append('|');
                }

                // ReSharper disable once InconsistentNaming
                const DWORD PARTY_TPL = 0x00020000;
                DWORD dwPartner = dwDialogPartner & 0x00000FFF;
                if (dwPartner == 0x00000FFF)
                    sbDialogPartner.Append("anyone|");
		        else if (dwPartner != 0)
                {
                    if ((dwDialogPartner & PARTY_TPL) != 0)
                        sbDialogPartner.Append(dwPartner < Common.PTemps.Count ? "pt_" + Common.PTemps[(int)dwPartner] + "|" : $"{dwPartner}|");
                    else
                        sbDialogPartner.Append(dwPartner < Common.Troops.Count ? "trp_" + Common.Troops[(int)dwPartner] + "|" : $"{dwPartner}|");
                }
                DWORD dwOther = (dwDialogPartner & 0xFFF00000) >> 20;
                if (dwOther != 0)
                    sbDialogPartner.Append(dwOther < Common.Troops.Count ? "other(trp_" + Common.Troops[(int)dwOther] + ")|" : $"other({dwOther})|");
                
                if (sbDialogPartner.Length == 0)
                    sbDialogPartner.Append('0');
                else
                    sbDialogPartner.Length--;

                if (iStartingDialogState < Common.DialogStates.Count)
                    fSource.Write("  [{0}, \"{1}\",\r\n    [", sbDialogPartner, Common.DialogStates[iStartingDialogState]);
                else
                    fSource.Write("  [{0}, {1},\r\n    [", sbDialogPartner, iStartingDialogState);

                int iRecords = fDialogs.GetInt();
                if (iRecords != 0)
                {
                    fSource.WriteLine();
                    Common.PrintStatement(ref fDialogs, ref fSource, iRecords, "      ");
                    fSource.WriteLine("    ],");
                }
                else
                    fSource.WriteLine("],");

                var strDialogText = fDialogs.GetWord();
                fSource.WriteLine("    \"{0}\",", strDialogText.Replace('_', ' '));

                int iEndingDialogState = fDialogs.GetInt();
                if (iEndingDialogState < Common.DialogStates.Count)
                    fSource.Write("    \"{0}\",\r\n    [", Common.DialogStates[iEndingDialogState]);
                else
                    fSource.Write("    {0},\r\n    [", iEndingDialogState);

                iRecords = fDialogs.GetInt();
                if (iRecords != 0)
                {
                    fSource.WriteLine();
                    Common.PrintStatement(ref fDialogs, ref fSource, iRecords, "      ");
                    fSource.Write("    ]");
                }
                else
                    fSource.Write("]");

                var strVoiceOver = fDialogs.GetWord();
                if (strVoiceOver.Trim() != "NO_VOICEOVER") fSource.Write(",\r\n    [\"{0}\"]", strVoiceOver);

                fSource.WriteLine("],\r\n");
            }
            fSource.Write("]");
            fSource.Close();
            fDialogs.Close();
        }
    }
}
