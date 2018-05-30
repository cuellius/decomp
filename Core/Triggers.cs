using System.IO;

namespace Decomp.Core
{
    public static class Triggers
    {
        public static string GetTriggerParam(double dblParam) => Common.GetTriggerParam(dblParam);

        public static void Decompile()
        {
            var fTriggers = new Text(Path.Combine(Common.InputPath, "triggers.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_triggers.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Triggers);
            fTriggers.GetString();
            int iTriggers = fTriggers.GetInt();
            for (int t = 0; t < iTriggers; t++)
            {
                double dCheckInterval = fTriggers.GetDouble(), dDelayInterval = fTriggers.GetDouble(), dReArmInterval = fTriggers.GetDouble();
                fSource.Write("  ({0}, {1}, {2},[", GetTriggerParam(dCheckInterval), GetTriggerParam(dDelayInterval), GetTriggerParam(dReArmInterval));
                int iConditionRecords = fTriggers.GetInt();
                if (iConditionRecords != 0)
                {
                    fSource.WriteLine();
                    Common.PrintStatement(ref fTriggers, ref fSource, iConditionRecords, "    ");
                    fSource.Write("  ");
                }
                fSource.Write("],\r\n  [");
                iConditionRecords = fTriggers.GetInt();
                if (iConditionRecords != 0)
                {
                    fSource.WriteLine();
                    Common.PrintStatement(ref fTriggers, ref fSource, iConditionRecords, "    ");
                    fSource.Write("  ");
                }

                fSource.WriteLine("]),\r\n");
            }
            fSource.Write("]");
            fSource.Close();
            fTriggers.Close();
        }
    }
}
