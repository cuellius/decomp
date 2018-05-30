using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Decomp.Core
{
    public class Text : Win32FileReader
    {
        public static string GetFirstStringFromFile(string sFileName)
        {
            var f = new StreamReader(sFileName);
            var s = f.ReadLine();
            f.Close();
            return s;
        }
        
        public Text(string s) : base(s) { }

        public string GetWord()
        {
            int i;
            do
            {
                i = Read();
            }while (Char.IsWhiteSpace((char)i)); //pass spaces

            var c = (char)i;
            var sb = new StringBuilder();

            do
            {
                sb.Append(c);
                i = Peek();
                if (i == -1)
                    break;
                c = (char)i;
                if (char.IsWhiteSpace(c))
                    break;
                Read();
            }while (true);

            return sb.ToString();
        }

        public long GetInt64()
        {
            long.TryParse(GetWord(), out var x);
            return x;
        }

        public ulong GetUInt64()
        {
            ulong.TryParse(GetWord(), out var x);
            return x;
        }

        public int GetInt() => (int)GetInt64();
        public uint GetUInt() => (uint)GetUInt64();
        public uint GetDWord() => (uint)GetUInt64();
  
        public double GetDouble()
        {
            Double.TryParse(GetWord(), NumberStyles.Any, CultureInfo.GetCultureInfo(1033), out var x);
            return x;
        }

        public string GetString() => ReadLine();
    }
}
