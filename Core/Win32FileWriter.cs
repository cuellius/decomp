using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Decomp.Core
{
    public class Win32FileWriter
    {
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW", SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern unsafe bool WriteFile(
            IntPtr hFile,
            void* lpBuffer,
            uint nNumberOfBytesToWrite,
            uint* lpNumberOfBytesWritten,
            IntPtr lpOverlapped
        );
        
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        private static extern unsafe int WideCharToMultiByte(
            uint uCodePage,
            uint dwFlags,
            [MarshalAs(UnmanagedType.LPWStr)]string lpWideCharStr,
            int cchWideChar,
            byte* lpMultiByteStr,
            int cbMultiByte,
            byte* lpDefaultChar,
            int* lpUsedDefaultChar
        );

        // ReSharper disable InconsistentNaming
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint CREATE_ALWAYS = 2;
        private const uint CP_UTF8 = 65001;
        private const int ERROR_FILE_NOT_FOUND = 2;
        private const int ERROR_PATH_NOT_FOUND = 3;
        private const int ERROR_ACCESS_DENIED = 5;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        // ReSharper restore InconsistentNaming

        private readonly StringBuilder _sb;
        private readonly string _fileName;

        private static readonly CultureInfo FormatProvider = CultureInfo.GetCultureInfo("en-US");
        
        public Win32FileWriter(string s)
        {
            _fileName = s;
            _sb = new StringBuilder(2048);
        }

        public void Write(char value) => _sb.Append(value);
        public void Write(char[] buffer) => _sb.Append(buffer);
        public void Write(string value) => _sb.Append(value);
        public void Write(bool value) => Write(value ? "True" : "False");
        public void Write(int value) => Write(value.ToString(FormatProvider));
        public void Write(uint value) => Write(value.ToString(FormatProvider));
        public void Write(long value) => Write(value.ToString(FormatProvider));
        public void Write(ulong value) => Write(value.ToString(FormatProvider));
        public void Write(float value) => Write(value.ToString(FormatProvider));
        public void Write(double value) => Write(value.ToString(FormatProvider));
        public void Write(decimal value) => Write(value.ToString(FormatProvider));

        public void Write(object value)
        {
            if (value == null) return;
            var f = value as IFormattable;
            Write(f?.ToString(null, FormatProvider) ?? value.ToString());
        }

        public void Write(string format, params object[] arg) => Write(arg == null ? format : String.Format(FormatProvider, format, arg));

        public void WriteLine()
        {
            _sb.Append('\r');
            _sb.Append('\n');
        }

        public void WriteLine(char value)
        {
            Write(value);
            WriteLine();
        }
        
        public void WriteLine(char[] buffer)
        {
            Write(buffer);
            WriteLine();
        }

        public void WriteLine(bool value)
        {
            Write(value);
            WriteLine();
        }
        
        public void WriteLine(int value)
        {
            Write(value);
            WriteLine();
        }
        
        public void WriteLine(uint value)
        {
            Write(value);
            WriteLine();
        }

        public void WriteLine(long value)
        {
            Write(value);
            WriteLine();
        }
        
        public void WriteLine(ulong value)
        {
            Write(value);
            WriteLine();
        }
        
        public void WriteLine(float value)
        {
            Write(value);
            WriteLine();
        }

        public void WriteLine(double value)
        {
            Write(value);
            WriteLine();
        }

        public void WriteLine(decimal value)
        {
            Write(value);
            WriteLine();
        }

        public void WriteLine(string value)
        {
            Write(value);
            WriteLine();
        }

        public void WriteLine(object value)
        {
            Write(value);
            WriteLine();
        }

        public void WriteLine(string format, params object[] arg) => WriteLine(arg == null ? format : String.Format(FormatProvider, format, arg));

        public void Close() => WriteContentIntoFile();
        
        private void WriteContentIntoFile() => WriteAllText(_fileName, _sb.ToString());

        public static unsafe void WriteAllText(string fileName, string data)
        {
            var bufferSize = WideCharToMultiByte(CP_UTF8, 0, data, data.Length, null, 0, null, null);
            var buffer = new byte[bufferSize];

            fixed (byte* pUtf8Buffer = &buffer[0])
            {
                WideCharToMultiByte(CP_UTF8, 0, data, data.Length, pUtf8Buffer, bufferSize, null, null);

                var pHandle = CreateFile(@"\\?\" + fileName, GENERIC_WRITE, 0, IntPtr.Zero, CREATE_ALWAYS, 0, IntPtr.Zero);

                if (/*pHandle == IntPtr.Zero || */ pHandle == INVALID_HANDLE_VALUE)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    switch (errorCode)
                    {
                        case ERROR_FILE_NOT_FOUND: throw new FileNotFoundException($"File \"{fileName}\" not found", fileName);
                        case ERROR_PATH_NOT_FOUND: throw new DirectoryNotFoundException($"Directory \"{Path.GetDirectoryName(fileName)}\" not found");
                        case ERROR_ACCESS_DENIED: throw new UnauthorizedAccessException($"Can't open file: \"{fileName}\"");
                        default: throw new Win32Exception(errorCode);
                    }
                }

                uint bytesWritten;
                WriteFile(pHandle, pUtf8Buffer, (uint)bufferSize, &bytesWritten, IntPtr.Zero);
                CloseHandle(pHandle);
            }
        }
    }
}
