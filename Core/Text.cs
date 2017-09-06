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
            long x;
            try
            {
                x = Convert.ToInt64(GetWord());
            }
            catch (Exception)
            {
                x = 0;
            }
            return x;
        }

        public ulong GetUInt64()
        {
            ulong x;
            try
            {
                x = Convert.ToUInt64(GetWord());
            }
            catch (Exception)
            {
                x = 0;
            }
            return x;
        }

        public int GetInt()
        {
            return (int)GetInt64();
        }
        
        public uint GetUInt()
        {
            return (uint)GetUInt64();
        }

        public uint GetDWord()
        {
            return (uint)GetUInt64();
        }
  
        public double GetDouble()
        {
            double x;
            try
            {
                x = Convert.ToDouble(GetWord(), new CultureInfo("en-US"));
            }
            catch (Exception)
            {
                x = 0.0;
            }
            return x;
        }

        public string GetString()
        {
            return ReadLine();
        }
    }
}
