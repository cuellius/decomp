using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Decomp.Core
{
    public class Win32FileReader
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
        private static extern unsafe bool ReadFile(
            IntPtr hFile,
            void* lpBuffer,
            uint nNumberOfBytesToRead,
            uint* lpNumberOfBytesRead,
            IntPtr lpOverlapped
        );

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        private static extern unsafe uint GetFileSize(IntPtr hFile, uint* lpFileSizeHigh);
        
        [DllImport("Kernel32.dll")]
        private static extern unsafe int MultiByteToWideChar(
            uint uCodePage, 
            uint dwFlags, 
            byte* lpMultiByteStr, 
            int cbMultiByte, 
            char* lpWideCharStr, 
            int cchWideChar
        );

        // ReSharper disable InconsistentNaming
        private const uint GENERIC_READ = 0x80000000;
        private const uint OPEN_EXISTING = 3;
        private const uint CP_UTF8 = 65001;
        private const int ERROR_FILE_NOT_FOUND = 2;
        private const int ERROR_PATH_NOT_FOUND = 3;
        private const int ERROR_ACCESS_DENIED = 5;
        // ReSharper restore InconsistentNaming

        private char[] _buffer;
        private int _bufferPos;
        private int _bufferLength;

        private unsafe void ReadContentIntoBuffer(string fileName)
        {
            var pHandle = CreateFile(@"\\?\" + fileName, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

            if (pHandle == IntPtr.Zero)
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
            
            var dwFileSize = GetFileSize(pHandle, null);

            var utf8Buffer = new byte[dwFileSize + 1];
            var gchBuffer = GCHandle.Alloc(utf8Buffer, GCHandleType.Pinned);
            var pAddr = Marshal.UnsafeAddrOfPinnedArrayElement(utf8Buffer, 0);
            var pBuffer = (byte*)pAddr.ToPointer();

            uint dwBytesRead;
            ReadFile(pHandle, pBuffer, dwFileSize, &dwBytesRead, IntPtr.Zero);
            pBuffer[dwBytesRead + 1] = 0;

            var pNewBuffer = pBuffer;
            if (pBuffer[0] == 0xEF && pBuffer[1] == 0xBB && pBuffer[2] == 0xBF) //BOM
            {
                pNewBuffer += 3;
                dwBytesRead -= 3;
            }

            _bufferLength = MultiByteToWideChar(CP_UTF8, 0, pNewBuffer, (int)dwBytesRead, null, 0);
            _buffer = new char[_bufferLength + 1];
            fixed (char* pUnicodeBuffer = &_buffer[0])
            {
                MultiByteToWideChar(CP_UTF8, 0, pNewBuffer, (int)dwBytesRead, pUnicodeBuffer, _bufferLength);
                _buffer[_bufferLength] = '\0';
            }
            CloseHandle(pHandle);
            gchBuffer.Free();

            _bufferPos = 0;
        }

        public Win32FileReader(string s)
        {
            ReadContentIntoBuffer(s);
        }
        
        public int Read()
        {
            if (_bufferPos == _bufferLength) return -1;
            var c = _buffer[_bufferPos];
            _bufferPos++;
            return c;
        }

        public int Peek()
        {
            return _bufferPos == _bufferLength ? -1 : _buffer[_bufferPos];
        }

        public string ReadLine()
        {
            if (_bufferPos == _bufferLength) return null;
            
            int i = _bufferPos;
            do
            {
                var ch = _buffer[i];
                if (ch == '\r' || ch == '\n')
                {
                    var s = new string(_buffer, _bufferPos, i - _bufferPos);
                    _bufferPos = i + 1;
                    if (ch == '\r' && _bufferPos < _bufferLength)
                    {
                        if (_buffer[_bufferPos] == '\n') _bufferPos++;
                    }
                    return s;
                }
                i++;
            } while (i < _bufferLength);

            var t = new string(_buffer, _bufferPos, _bufferLength - _bufferPos);
            _bufferPos = _bufferLength;
            return t;
            //return null;
        }

        public void Close() //for compatibility with old code
        {
        }

        public static string[] ReadAllLines(string path)
        {
            var list = new List<string>();
            var f = new Win32FileReader(path);
            string item;
            while ((item = f.ReadLine()) != null) list.Add(item); 
            return list.ToArray();
        }
    }
}
