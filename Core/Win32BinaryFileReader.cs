using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Decomp.Core
{
    public class Win32BinaryFileReader
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

        // ReSharper disable InconsistentNaming
        private const uint GENERIC_READ = 0x80000000;
        private const uint OPEN_EXISTING = 3;
        private const int ERROR_FILE_NOT_FOUND = 2;
        private const int ERROR_PATH_NOT_FOUND = 3;
        private const int ERROR_ACCESS_DENIED = 5;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        // ReSharper restore InconsistentNaming

        private byte[] _buffer;
        public int Position { get; set; }

        private unsafe void ReadContentIntoBuffer(string fileName)
        {
            var pHandle = CreateFile(@"\\?\" + fileName, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

            if (pHandle == INVALID_HANDLE_VALUE)
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
            _buffer = new byte[dwFileSize];
            fixed (byte* pBuffer = &_buffer[0])
            {
                uint dwBytesRead;
                ReadFile(pHandle, pBuffer, dwFileSize, &dwBytesRead, IntPtr.Zero);
            }
            CloseHandle(pHandle);
            
            Position = 0;
        }

        public Win32BinaryFileReader(string s) => ReadContentIntoBuffer(s);

        public sbyte ReadSingedByte() => (sbyte)_buffer[Position++];
        public byte ReadByte() => _buffer[Position++];

        public unsafe byte[] ReadBytes(int numBytes)
        {
            var bytes = new byte[numBytes];
            fixed (byte* pBuffer = &bytes[0])
            {
                Marshal.Copy(_buffer, Position, (IntPtr)pBuffer, numBytes);
            }

            Position += numBytes;

            return bytes;
        }

        public void SkipBytes(int numBytes) => Position += numBytes;

        public short ReadInt16()
        {
            var x = BitConverter.ToInt16(_buffer, Position);
            Position += sizeof(short);
            return x;
        }

        public ushort ReadUInt16()
        {
            var x = BitConverter.ToUInt16(_buffer, Position);
            Position += sizeof(ushort);
            return x;
        }

        public int ReadInt32()
        {
            var x = BitConverter.ToInt32(_buffer, Position);
            Position += sizeof(int);
            return x;
        }

        public uint ReadUInt32()
        {
            var x = BitConverter.ToUInt32(_buffer, Position);
            Position += sizeof(uint);
            return x;
        }

        public long ReadInt64()
        {
            var x = BitConverter.ToInt64(_buffer, Position);
            Position += sizeof(long);
            return x;
        }

        public ulong ReadUInt64()
        {
            var x = BitConverter.ToUInt64(_buffer, Position);
            Position += sizeof(ulong);
            return x;
        }

        public float ReadFloat()
        {
            var x = BitConverter.ToSingle(_buffer, Position);
            Position += sizeof(float);
            return x;
        }

        public double ReadDouble()
        {
            var x = BitConverter.ToDouble(_buffer, Position);
            Position += sizeof(double);
            return x;
        }

        public unsafe string ReadAsciiString()
        {
            var length = ReadInt32();
            if (length == 0) return "";

            var bytes = ReadBytes(length);
            string s;
            fixed (byte* pBuffer = &bytes[0])
            {
                s = new string((sbyte*)pBuffer, 0, bytes.Length);
            }
            return s;
        }

        public void Close() //for compatibility with old code
        {
        }
    }
}
