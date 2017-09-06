namespace Decomp.Core
{
    public static class Scripts
    {
        public static string[] InitializeVariables()
        {
            return Win32FileReader.ReadAllLines(Common.InputPath + @"\variables.txt");
        }

        public static string[] Initialize()
        {
            var fID = new Text(Common.InputPath + @"\scripts.txt");
            fID.GetString();
            int n = fID.GetInt();
            var aScripts = new string[n];
            for (int i = 0; i < n; i++)
            {
                aScripts[i] = fID.GetWord();

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
            }
            fID.Close();

            return aScripts;
        }

        public static void Decompile()
        {
            var fScripts = new Text(Common.InputPath + @"\scripts.txt");
            var fSource = new Win32FileWriter(Common.OutputPath + @"\module_scripts.py");
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
        }
    }
}
