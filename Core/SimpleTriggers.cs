using System.IO;

namespace Decomp.Core
{
    public static class SimpleTriggers
    {
        public static void Decompile()
        {
            var fTriggers = new Text(Path.Combine(Common.InputPath, "simple_triggers.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_simple_triggers.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.SimpleTriggers);
            fTriggers.GetString();
            int iSimpleTriggers = fTriggers.GetInt();
            for (int t = 0; t < iSimpleTriggers; t++)
            {
                fSource.Write("  ({0},\r\n  [", Common.GetTriggerParam(fTriggers.GetDouble()));
                int iRecords = fTriggers.GetInt();
                if (iRecords != 0)
                {
                    fSource.WriteLine();
                    Common.PrintStatement(ref fTriggers, ref fSource, iRecords, "    ");
                    fSource.Write("  ");
                }
                fSource.Write("]),\r\n\r\n");
            }
            fSource.Write("]");
            fSource.Close();
            fTriggers.Close();
        }
    }
}
