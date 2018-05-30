using System.IO;

namespace Decomp.Core
{
    public static class Scripts
    {
        public static string[] InitializeVariables() => Win32FileReader.ReadAllLines(Path.Combine(Common.InputPath, "variables.txt"));

        public static string[] Initialize()
        {
            if (!File.Exists(Path.Combine(Common.InputPath, "scripts.txt"))) return new string[0];

            var fId = new Text(Path.Combine(Common.InputPath, "scripts.txt"));
            fId.GetString();
            int n = fId.GetInt();
            var aScripts = new string[n];
            for (int i = 0; i < n; i++)
            {
                aScripts[i] = fId.GetWord();

                fId.GetWord();

                int iRecords = fId.GetInt();
                if (iRecords != 0)
                {
                    for (int r = 0; r < iRecords; r++)
                    {
                        fId.GetWord();
                        int iParams = fId.GetInt();
                        for (int p = 0; p < iParams; p++) fId.GetWord();
                    }
                }
            }
            fId.Close();

            return aScripts;
        }

        public static void Decompile()
        {
            var fScripts = new Text(Path.Combine(Common.InputPath, "scripts.txt"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "module_scripts.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.Scripts);
            fScripts.GetString();
            int iScripts = fScripts.GetInt();
            
            for (int s = 0; s < iScripts; s++)
            {
                fSource.Write("  (\"{0}\",\r\n  [\r\n", fScripts.GetWord());
                fScripts.GetInt();
                int iRecords = fScripts.GetInt();
                Common.PrintStatement(ref fScripts, ref fSource, iRecords, "    ");
                fSource.Write("  ]),\r\n\r\n");
            }
            fSource.Write("]");
            fSource.Close();
            fScripts.Close();

            Common.GenerateId("ID_scripts.py", Common.Procedures, "script");
        }
    }
}
