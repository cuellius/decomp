using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace Decomp.Windows
{
    public class OpenFileOrFolderDialog : CommonDialog
    {
        // ReSharper disable InconsistentNaming
        private const uint BFFM_INITIALIZED = 1;
        private const uint BFFM_SELCHANGED = 2;
        private const uint WM_USER = 0x0400;
        private const uint WM_SETICON = 0x0080;
        private const uint BFFM_SETSELECTIONW = WM_USER + 103;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint WM_SETFONT = 0x0030;
        private const uint WM_DESTROY = 0x0002;
        private const uint WM_NOTIFY = 0x4E;
        private const uint CDIS_HOT = 0x0040;
        private const int MAX_PATH = 260;
        private const int NM_FIRST = 0;
        private const int NM_CUSTOMDRAW = NM_FIRST - 12;

        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;

        private delegate int BrowseCallbackProc(IntPtr hWnd, uint uMsg, IntPtr lParam, IntPtr lpData);

        // ReSharper disable UnusedMember.Local
        [Flags]
        private enum BrowseInfoFlags : uint
        {
            ReturnOnlyFileSystemDirs = 0x00000001,
            DontGoBelowDomain = 0x00000002,
            StatusText = 0x00000004,
            ReturnFileSystemAncestors = 0x00000008,
            EditBox = 0x00000010,
            Validate = 0x00000020,
            NewDialogStyle = 0x00000040,
            BrowseIncludeUrls = 0x00000080,
            UseNewUI = EditBox | NewDialogStyle,
            UaHint = 0x00000100,
            NoNewFolderButton = 0x00000200,
            NoTranslateTargets = 0x00000400,
            BrowseForComputer = 0x00001000,
            BrowseForPrinter = 0x00002000,
            BrowseIncludeFiles = 0x00004000,
            Shareable = 0x00008000,
            BrowseFileJunctions = 0x00010000,
        }
        // ReSharper restore UnusedMember.Local

        [StructLayout(LayoutKind.Sequential)]
        private struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public IntPtr pszDisplayName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszTitle;
            public BrowseInfoFlags ulFlags;
            public BrowseCallbackProc lpfn;
            private readonly IntPtr lParam;
            private readonly int iImage;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        private struct SCROLLINFO
        {
            public uint cbSize;
            public uint fMask;
            public readonly int nMin;
            public int nMax;
            public uint nPage;
            public readonly int nPos;
            private readonly int nTrackPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NMHDR
        {
            public readonly IntPtr hwndFrom;
            private readonly IntPtr idFrom;
            public readonly int code;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NMCUSTOMDRAW
        {
            private readonly NMHDR hdr;
            private readonly int dwDrawStage;
            public readonly IntPtr hdc;
            public RECT rc;
            private readonly IntPtr dwItemSpec;
            public readonly uint uItemState;
            private readonly IntPtr lItemlParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000002-0000-0000-C000-000000000046")]
        private interface IMalloc
        {
            [PreserveSig]
            IntPtr Alloc([In] int cb);
            [PreserveSig]
            IntPtr Realloc([In] IntPtr pv, [In] int cb);
            [PreserveSig]
            void Free([In] IntPtr pv);
            [PreserveSig]
            int GetSize([In] IntPtr pv);
            [PreserveSig]
            int DidAlloc(IntPtr pv);
            [PreserveSig]
            void HeapMinimize();
        }

        [DllImport("Shell32.dll")]
        private static extern int SHGetMalloc(out IMalloc ppMalloc);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHBrowseForFolderW")]
        private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHGetPathFromIDListW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszPath);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, EntryPoint = "SendMessageW")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint uMessage, IntPtr wParam, IntPtr lParam);
        
        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetWindowTextW", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "FindWindowExW", ExactSpelling = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, IntPtr lpszWindow);

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("Shell32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, IntPtr *pidl, uint sfgaoIn, uint *psfgaoOut);
        
        [DllImport("User32.dll", CharSet = CharSet.Unicode, EntryPoint = "PostMessageW")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);

        [DllImport("Gdi32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateFontW")]
        private static extern IntPtr CreateFont(
            int nHeight, 
            int nWidth, 
            int nEscapement, 
            int nOrientation,
            int fnWeight, 
            uint fdwItalic, 
            uint fdwUnderline, 
            uint fdwStrikeOut, 
            uint fdwCharSet, 
            uint fdwOutputPrecision, 
            uint fdwClipPrecision, 
            uint fdwQuality, 
            uint fdwPitchAndFamily,
            string lpszFace
        );

        internal delegate IntPtr WndProcDelegate(IntPtr hWnd, uint uMessage, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", EntryPoint = "GetWindowLong")]
        internal static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("User32.dll", EntryPoint = "GetWindowLongPtr")]
        internal static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        internal static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLongPtr32(hWnd, nIndex);
        }

        [DllImport("User32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("User32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        internal static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("User32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, WndProcDelegate dwNewLong);

        [DllImport("User32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, WndProcDelegate dwNewLong);

        internal static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, WndProcDelegate dwNewLong)
        {
            return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong));
        }

        public enum GWL
        {
            GWL_WNDPROC = -4,
            GWL_HINSTANCE = -6,
            GWL_HWNDPARENT = -8,
            GWL_STYLE = -16,
            GWL_EXSTYLE = -20,
            GWL_USERDATA = -21,
            GWL_ID = -12
        }

        public static IntPtr SetClassLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size > 4 ? SetClassLongPtr64(hWnd, nIndex, dwNewLong) : new IntPtr(SetClassLongPtr32(hWnd, nIndex, unchecked((uint)dwNewLong.ToInt32())));
        }

        [DllImport("User32.dll", EntryPoint = "SetClassLong")]
        public static extern uint SetClassLongPtr32(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("User32.dll", EntryPoint = "SetClassLongPtr")]
        public static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("User32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "GetClassNameW")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("Gdi32.dll")]
        private static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

        [DllImport("Gdi32.dll")]
        private static extern IntPtr CreateSolidBrush(uint crColor);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("User32.dll")]
        private static extern int FillRect(IntPtr hDC, [In] ref RECT lprc, IntPtr hbr);

        [DllImport("Gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("Gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MoveToEx(IntPtr hdc, int X, int Y, IntPtr lpPoint);
        
        [DllImport("Gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LineTo(IntPtr hdc, int nXEnd, int nYEnd);

        private static void Line(IntPtr hdc, int x1, int y1, int x2, int y2)
        {
            MoveToEx(hdc, x1, y1, IntPtr.Zero);
            LineTo(hdc, x2, y2);
        }

        private static uint RGB(byte r, byte g, byte b)
        {
            return r | ((uint)g << 8) | (uint)b << 16;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHSTOCKICONINFO
        {
            public uint cbSize;
            public readonly IntPtr hIcon;
            private readonly int iSysIconIndex;
            private readonly int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            private readonly string szPath;
        }

        [DllImport("Shell32.dll", SetLastError = false)]
        private static extern int SHGetStockIconInfo(uint siid, uint uFlags, ref SHSTOCKICONINFO psii);

        //[DllImport("User32.dll")]
        //private static extern int DestroyIcon(IntPtr hIcon);

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x000000000;
        private const uint SHGSI_SMALLICON = 0x000000001;
        private const uint SIID_FOLDER = 3;
        
        private const int TV_FIRST = 0x1100;
        private const int TVM_GETNEXTITEM = TV_FIRST + 10;
        private const int TVGN_ROOT = 0x0;
        private const int TVGN_CHILD = 0x4;
        private const int TVGN_NEXTVISIBLE = 0x6;
        private const int TVGN_CARET = 0x9;
        
        private const int SIF_RANGE = 0x1;
        private const int SIF_PAGE = 0x2;
        private const int SIF_POS = 0x4;
        
        private const int SB_VERT = 0x1;
        
        private const int SB_LINEUP = 0;
        private const int SB_LINEDOWN = 1;
        private const int WM_VSCROLL = 0x115;

        // ReSharper restore InconsistentNaming

        public string Path { get; set; }
        public string Title { get; set; }
        public string RootFolder { get; set; }
        public bool AcceptFiles { get; set; }
        public bool ShowNewFolderButton { get; set; }

        public OpenFileOrFolderDialog()
        {
            Path = "";
            Title = "";
            RootFolder = "";
            AcceptFiles = true;
            ShowNewFolderButton = true;
        }

        public override void Reset()
        {
            Path = "";
            Title = "";
            RootFolder = "";
            AcceptFiles = true;
            ShowNewFolderButton = true;
        }

        private IntPtr _pOrigWindowProc;
        private WndProcDelegate _windowProc;
        public int BrowseInfoProc(IntPtr hwnd, uint uMessage, IntPtr lParam, IntPtr lpData)
        {
            switch (uMessage)
            {
                case BFFM_INITIALIZED:
                    if (Directory.Exists(Path) || File.Exists(Path))
                    {
                        var lpszInitialPath = Marshal.StringToHGlobalUni(Path);
                        SendMessage(hwnd, BFFM_SETSELECTIONW, (IntPtr)1, lpszInitialPath);
                        Marshal.FreeHGlobal(lpszInitialPath);
                    }
                    var hFont = CreateFont(-12, 0, 0, 0, 400, 0, 0, 0, 1, 4, 0, 2, 0, "Segoe UI");
                    SetWindowText(GetDlgItem(hwnd, 0x3748), Application.GetResource("LocalizationFolder"));
                    SetWindowText(GetDlgItem(hwnd, 0x3746), Application.GetResource("LocalizationCreateNewFolder"));
                    SetWindowText(GetDlgItem(hwnd, 1), "OK");
                    SetWindowText(GetDlgItem(hwnd, 2), Application.GetResource("LocalizationCancel"));
                    var hwndFolders = FindWindowEx(hwnd, IntPtr.Zero, "SHBrowseForFolder ShellNameSpace Control",
                        IntPtr.Zero);
                    SetWindowPos(hwndFolders, IntPtr.Zero, 10, 10, 300, 200, SWP_NOZORDER);
                    SetWindowPos(GetDlgItem(hwnd, 0x3746), IntPtr.Zero, 10, 254, 150, 24, SWP_NOZORDER);
                    SetWindowPos(GetDlgItem(hwnd, 1), IntPtr.Zero, 149, 254, 78, 24, SWP_NOZORDER);
                    SetWindowPos(GetDlgItem(hwnd, 2), IntPtr.Zero, 232, 254, 78, 24, SWP_NOZORDER);
                    SetWindowPos(GetDlgItem(hwnd, 0x3744), IntPtr.Zero, 0, 0, 242, 22, SWP_NOZORDER | SWP_NOMOVE);
                    SetWindowPos(GetDlgItem(hwnd, 0x3748), IntPtr.Zero, 20, 221, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
                    ShowWindow(GetDlgItem(hwnd, 0x3742), 0);
                    if (!String.IsNullOrEmpty(Title)) SetWindowText(hwnd, Title);
                    if (hFont != IntPtr.Zero)
                    {
                        SendMessage(GetDlgItem(hwnd, 1), WM_SETFONT, hFont, (IntPtr)1);
                        SendMessage(GetDlgItem(hwnd, 2), WM_SETFONT, hFont, (IntPtr)1);
                        SendMessage(GetDlgItem(hwnd, 0x3746), WM_SETFONT, hFont, (IntPtr)1);
                        SendMessage(GetDlgItem(hwnd, 0x3748), WM_SETFONT, hFont, (IntPtr)1);
                        SendMessage(GetDlgItem(hwnd, 0x3744), WM_SETFONT, hFont, (IntPtr)1);
                        SendMessage(FindWindowEx(FindWindowEx(hwnd, IntPtr.Zero, "SHBrowseForFolder ShellNameSpace Control", IntPtr.Zero), IntPtr.Zero, "SysTreeView32", IntPtr.Zero), WM_SETFONT, hFont, (IntPtr)1);
                    }

                    var sii = new SHSTOCKICONINFO { cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO)) };
                    SHGetStockIconInfo(SIID_FOLDER, SHGFI_ICON | SHGFI_LARGEICON, ref sii);
                    SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_BIG, sii.hIcon);
                    SHGetStockIconInfo(SIID_FOLDER, SHGFI_ICON | SHGSI_SMALLICON, ref sii);
                    SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_SMALL, sii.hIcon);

                    _windowProc = WindowProc;
                    _pOrigWindowProc = SetWindowLongPtr(hwnd, (int)GWL.GWL_WNDPROC, _windowProc);
                    return 1;
                case BFFM_SELCHANGED:
                    var hTree = FindWindowEx(FindWindowEx(hwnd, IntPtr.Zero, "SHBrowseForFolder ShellNameSpace Control", IntPtr.Zero), IntPtr.Zero, "SysTreeView32", IntPtr.Zero);
                    var hTreeItemRoot = SendMessage(hTree, TVM_GETNEXTITEM, (IntPtr)TVGN_ROOT, IntPtr.Zero);
                    var hTreeItemCaret = SendMessage(hTree, TVM_GETNEXTITEM, (IntPtr)TVGN_CARET, IntPtr.Zero);
                    int count = 0, pos = 0;
                    for (var hTreeItemChild = SendMessage(hTree, TVM_GETNEXTITEM, (IntPtr)TVGN_CHILD, hTreeItemRoot); hTreeItemChild != IntPtr.Zero; hTreeItemChild = SendMessage(hTree, TVM_GETNEXTITEM, (IntPtr)TVGN_NEXTVISIBLE, hTreeItemChild), count++) 
                        if (hTreeItemCaret == hTreeItemChild) pos = count; 
                    var si = new SCROLLINFO();
                    si.cbSize = (uint)Marshal.SizeOf(si);
                    si.fMask = SIF_POS | SIF_RANGE | SIF_PAGE;
                    GetScrollInfo(hTree, SB_VERT, ref si);
                    si.nPage >>= 1;
                    if (pos > (int)(si.nMin + si.nPage) && pos <= (int)(si.nMax - si.nMin - si.nPage))
                    {
                        si.nMax = si.nPos - si.nMin + (int)si.nPage;
                        for (; pos < si.nMax; pos++) PostMessage(hTree, WM_VSCROLL, (IntPtr)SB_LINEUP, IntPtr.Zero);
                        for (; pos > si.nMax; pos--) PostMessage(hTree, WM_VSCROLL, (IntPtr)SB_LINEDOWN, IntPtr.Zero);
                    }
                    return 0;
                default:
                    return 0;
            }
        }

        private unsafe IntPtr WindowProc(IntPtr hWnd, uint uMessage, IntPtr wParam, IntPtr lParam)
        {
            switch (uMessage)
            {
                case WM_NOTIFY:
                    var nm = (NMHDR*)lParam;
                    var sb = new StringBuilder(256);
                    GetClassName(nm->hwndFrom, sb, sb.Capacity);
                    if (sb.ToString().ToLower() == "button" && nm->code == NM_CUSTOMDRAW)
                    {
                        var cd = (NMCUSTOMDRAW*)nm;
                        var hPen = CreatePen(0, 0, (cd->uItemState & CDIS_HOT) != 0 ? RGB(0x3C, 0x7F, 0xB1) : RGB(0x70, 0x70, 0x70));
                        var hBrush = CreateSolidBrush((cd->uItemState & CDIS_HOT) != 0 ? RGB(0xBE, 0xE6, 0xFD) : RGB(0xDD, 0xDD, 0xDD));

                        var hOldPen = SelectObject(cd->hdc, hPen);
                        var hOldBrush = SelectObject(cd->hdc, hBrush);

                        FillRect(cd->hdc, ref cd->rc, hBrush);
                        Line(cd->hdc, cd->rc.left, cd->rc.top, cd->rc.right - 1, cd->rc.top);
                        Line(cd->hdc, cd->rc.right - 1, cd->rc.top, cd->rc.right - 1, cd->rc.bottom - 1);
                        Line(cd->hdc, cd->rc.right - 1, cd->rc.bottom - 1, cd->rc.left, cd->rc.bottom - 1);
                        Line(cd->hdc, cd->rc.left, cd->rc.top, cd->rc.left, cd->rc.bottom);

                        SelectObject(cd->hdc, hOldPen);
                        SelectObject(cd->hdc, hOldBrush);
                        DeleteObject(hPen);
                        DeleteObject(hBrush);
                        return IntPtr.Zero;
                    }
                    break;
                case WM_DESTROY:
                    SetWindowLongPtr(hWnd, (int)GWL.GWL_WNDPROC, _pOrigWindowProc);
                    break;
                default:
                    return CallWindowProc(_pOrigWindowProc, hWnd, uMessage, wParam, lParam);
            }
            return CallWindowProc(_pOrigWindowProc, hWnd, uMessage, wParam, lParam);
        }

        protected override unsafe bool RunDialog(IntPtr hOwner)
        {
            var pszBuffer = Marshal.AllocHGlobal(MAX_PATH << 1);

            var bi = new BROWSEINFO
            {
                hwndOwner = hOwner,
                pszDisplayName = pszBuffer,
                lpszTitle = Title,
                ulFlags = BrowseInfoFlags.ReturnOnlyFileSystemDirs | BrowseInfoFlags.EditBox |
                    BrowseInfoFlags.NewDialogStyle | BrowseInfoFlags.NoTranslateTargets,
                lpfn = BrowseInfoProc
            };

            var pidlRoot = IntPtr.Zero;
            if (Directory.Exists(RootFolder))
            {
                SHParseDisplayName(RootFolder, IntPtr.Zero, &pidlRoot, 0, null);
                bi.pidlRoot = pidlRoot;
            }

            if (AcceptFiles) bi.ulFlags |= BrowseInfoFlags.BrowseIncludeFiles;
            if (!ShowNewFolderButton) bi.ulFlags |= BrowseInfoFlags.NoNewFolderButton;

            var pidlList = IntPtr.Zero;
            try
            {
                pidlList = SHBrowseForFolder(ref bi);

                Marshal.FreeHGlobal(pszBuffer);

                if (pidlList == IntPtr.Zero) return false;

                var sb = new StringBuilder(MAX_PATH);
                SHGetPathFromIDList(pidlList, sb);
                Path = sb.ToString();
            }
            finally
            {
                IMalloc malloc;
                SHGetMalloc(out malloc);
                if (pidlRoot != IntPtr.Zero) malloc.Free(pidlRoot);
                if (pidlList != IntPtr.Zero) malloc.Free(pidlList);
            }
            return true;
        }
    }
}
