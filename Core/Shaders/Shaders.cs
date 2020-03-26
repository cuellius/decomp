using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace Decomp.Core.Shaders
{
    public static unsafe class Shaders
    {
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WNDCLASS
        {
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
        }

        [DllImport("User32.dll", EntryPoint = "RegisterClassW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern ushort RegisterClass(
            [In] ref WNDCLASS lpWndClass
        );

        [DllImport("User32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle,
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam
        );

        [DllImport("User32.dll", EntryPoint = "DefWindowProcW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr DefWindowProc(
            IntPtr hWnd,
            uint uMsg,
            IntPtr wParam,
            IntPtr lParam
        );

        [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyWindow(
            IntPtr hwnd
        );

        [DllImport("User32.dll", EntryPoint = "UnregisterClassW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterClass(
            string lpClassName, 
            IntPtr hInstance
        );


        [DllImport("Kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(
            IntPtr lpModuleName
        );

        [StructLayout(LayoutKind.Sequential)]
        private struct D3DPRESENT_PARAMETERS
        {
            public uint BackBufferWidth;
            public uint BackBufferHeight;
            public uint BackBufferFormat;
            public uint BackBufferCount;
            public uint MultiSampleType;
            public uint MultiSampleQuality;
            public uint SwapEffect;
            public IntPtr hDeviceWindow;
            [MarshalAs(UnmanagedType.Bool)]
            public bool Windowed;
            [MarshalAs(UnmanagedType.Bool)]
            public bool EnableAutoDepthStencil;
            public uint AutoDepthStencilFormat;
            public uint Flags;
            public uint FullScreen_RefreshRateInHz;
            public uint PresentationInterval;
        }

#pragma warning disable CA1707 // Identifiers should not contain underscores
        public const uint WS_OVERLAPPED = 0x00000000;
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_MINIMIZE = 0x20000000;
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_DISABLED = 0x08000000;
        public const uint WS_CLIPSIBLINGS = 0x04000000;
        public const uint WS_CLIPCHILDREN = 0x02000000;
        public const uint WS_MAXIMIZE = 0x01000000;
        public const uint WS_CAPTION = 0x00C00000;     /* WS_BORDER | WS_DLGFRAME  */
        public const uint WS_BORDER = 0x00800000;
        public const uint WS_DLGFRAME = 0x00400000;
        public const uint WS_VSCROLL = 0x00200000;
        public const uint WS_HSCROLL = 0x00100000;
        public const uint WS_SYSMENU = 0x00080000;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_GROUP = 0x00020000;
        public const uint WS_TABSTOP = 0x00010000;

        public const uint WS_MINIMIZEBOX = 0x00020000;
        public const uint WS_MAXIMIZEBOX = 0x00010000;

        public const uint WS_TILED = WS_OVERLAPPED;
        public const uint WS_ICONIC = WS_MINIMIZE;
        public const uint WS_SIZEBOX = WS_THICKFRAME;
        public const uint WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;

        // Common Window Styles

        public const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

        public const uint WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;
#pragma warning restore CA1707 // Identifiers should not contain underscores

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct IDirect3D9
        {
            public IntPtr* lpVtbl;
            /*
            0 STDMETHOD(QueryInterface)(THIS_ REFIID riid, void** ppvObj) PURE;
            1 STDMETHOD_(ULONG,AddRef)(THIS) PURE;
            2 STDMETHOD_(ULONG,Release)(THIS) PURE;

            // IDirect3D9 methods
            3 STDMETHOD(RegisterSoftwareDevice)(THIS_ void* pInitializeFunction) PURE;
            4 STDMETHOD_(UINT, GetAdapterCount)(THIS) PURE;
            5 STDMETHOD(GetAdapterIdentifier)(THIS_ UINT Adapter,DWORD Flags,D3DADAPTER_IDENTIFIER9* pIdentifier) PURE;
            6 STDMETHOD_(UINT, GetAdapterModeCount)(THIS_ UINT Adapter,D3DFORMAT Format) PURE;
            7 STDMETHOD(EnumAdapterModes)(THIS_ UINT Adapter,D3DFORMAT Format,UINT Mode,D3DDISPLAYMODE* pMode) PURE;
            8 STDMETHOD(GetAdapterDisplayMode)(THIS_ UINT Adapter,D3DDISPLAYMODE* pMode) PURE;
            9 STDMETHOD(CheckDeviceType)(THIS_ UINT Adapter,D3DDEVTYPE DevType,D3DFORMAT AdapterFormat,D3DFORMAT BackBufferFormat,BOOL bWindowed) PURE;
            0 STDMETHOD(CheckDeviceFormat)(THIS_ UINT Adapter,D3DDEVTYPE DeviceType,D3DFORMAT AdapterFormat,DWORD Usage,D3DRESOURCETYPE RType,D3DFORMAT CheckFormat) PURE;
            1 STDMETHOD(CheckDeviceMultiSampleType)(THIS_ UINT Adapter,D3DDEVTYPE DeviceType,D3DFORMAT SurfaceFormat,BOOL Windowed,D3DMULTISAMPLE_TYPE MultiSampleType,DWORD* pQualityLevels) PURE;
            2 STDMETHOD(CheckDepthStencilMatch)(THIS_ UINT Adapter,D3DDEVTYPE DeviceType,D3DFORMAT AdapterFormat,D3DFORMAT RenderTargetFormat,D3DFORMAT DepthStencilFormat) PURE;
            3 STDMETHOD(CheckDeviceFormatConversion)(THIS_ UINT Adapter,D3DDEVTYPE DeviceType,D3DFORMAT SourceFormat,D3DFORMAT TargetFormat) PURE;
            4 STDMETHOD(GetDeviceCaps)(THIS_ UINT Adapter,D3DDEVTYPE DeviceType,D3DCAPS9* pCaps) PURE;
            5 STDMETHOD_(HMONITOR, GetAdapterMonitor)(THIS_ UINT Adapter) PURE;
            6 STDMETHOD(CreateDevice)(THIS_ UINT Adapter,D3DDEVTYPE DeviceType,HWND hFocusWindow,DWORD BehaviorFlags,D3DPRESENT_PARAMETERS* pPresentationParameters,IDirect3DDevice9** ppReturnedDeviceInterface) PURE;
            */
        }

        private delegate int DelegateIDirect3D9_CreateDevice(IDirect3D9* This, uint Adapter, uint DeviceType, IntPtr hFocusWindow, uint BehaviorFlags, IntPtr pPresentationParameters, IntPtr ppReturnedDeviceInterface);
        // ReSharper disable once UnusedMethodReturnValue.Local
        private static int IDirect3D9_CreateDevice(IDirect3D9* p, uint Adapter, uint DeviceType, IntPtr hFocusWindow, uint BehaviorFlags, D3DPRESENT_PARAMETERS* pPresentationParameters, IDirect3DDevice9** ppReturnedDeviceInterface)
        {
            var lpIDirect3D9_CreateDevice = (DelegateIDirect3D9_CreateDevice)Marshal.GetDelegateForFunctionPointer(p->lpVtbl[16],
                typeof(DelegateIDirect3D9_CreateDevice));
            return lpIDirect3D9_CreateDevice(p, Adapter, DeviceType, hFocusWindow, BehaviorFlags, (IntPtr)pPresentationParameters, (IntPtr)ppReturnedDeviceInterface);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct IDirect3DDevice9
        {
            public IntPtr* lpVtbl;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct ID3DXEffect
        {
            public IntPtr* lpVtbl;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct ID3DXBuffer
        {
            public IntPtr* lpVtbl;
            /*    
            // IUnknown
            STDMETHOD(QueryInterface)(THIS_ REFIID iid, LPVOID *ppv) PURE;
            STDMETHOD_(ULONG, AddRef)(THIS) PURE;
            STDMETHOD_(ULONG, Release)(THIS) PURE;

            // ID3DXBuffer
            STDMETHOD_(LPVOID, GetBufferPointer)(THIS) PURE;
            STDMETHOD_(DWORD, GetBufferSize)(THIS) PURE;*/
        }

        private delegate IntPtr DelegateID3DXBuffer_GetBufferPointer(ID3DXBuffer* This);
        private static IntPtr ID3DXBuffer_GetBufferPointer(ID3DXBuffer* p)
        {
            var lpDelegateID3DXBuffer_GetBufferPointer = (DelegateID3DXBuffer_GetBufferPointer)Marshal.GetDelegateForFunctionPointer(p->lpVtbl[3],
                typeof(DelegateID3DXBuffer_GetBufferPointer));
            var retval = lpDelegateID3DXBuffer_GetBufferPointer(p);
            return retval;
        }

        [DllImport("d3d9.dll", EntryPoint = "Direct3DCreate9", CallingConvention = CallingConvention.StdCall), SuppressUnmanagedCodeSecurity]
        private static extern IDirect3D9* Direct3DCreate9(
            uint SDKVersion
        );

        [DllImport("d3dx9_43.dll", EntryPoint = "D3DXCreateEffectFromFileW", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        private static extern int D3DXCreateEffectFromFile(
            IDirect3DDevice9* pDevice,
            [MarshalAs(UnmanagedType.LPWStr)]string pSrcFile,
            IntPtr pDefines,
            IntPtr pInclude,
            uint Flags,
            IntPtr pPool,
            ID3DXEffect** ppEffect,
            ID3DXBuffer** ppCompilationErrors
        );

        [DllImport("d3dx9_43.dll", CallingConvention = CallingConvention.StdCall), SuppressUnmanagedCodeSecurity]
        private static extern int D3DXDisassembleEffect(
            ID3DXEffect* pEffect,
            [MarshalAs(UnmanagedType.Bool)]bool EnableColorCode,
            ID3DXBuffer** ppDisassembly
        );

        private const ushort D3D_SDK_VERSION = 32;
        private const uint D3DFMT_D24S8 = 75;
        private const uint D3DFMT_X8R8G8B8 = 22;
        private const uint D3DSWAPEFFECT_DISCARD = 1;
        private const uint D3DPRESENT_INTERVAL_IMMEDIATE = 0x80000000u;
        private const uint D3DDEVTYPE_NULLREF = 4;
        private const uint D3DCREATE_SOFTWARE_VERTEXPROCESSING = 0x00000020u;
        private const uint WM_DESTROY = 0x0002;

        [DllImport("User32.dll")]
        private static extern void PostQuitMessage(int nExitCode);

        private static D3DPRESENT_PARAMETERS g_D3DPresentParameters;
        private static IDirect3D9* g_D3D = null;
        private static IDirect3DDevice9* g_D3DDevice = null;
        private static IntPtr g_hWnd = IntPtr.Zero;
        private static WndProc _wndProc;

        private const string g_szClassName = "shadersdecomp548hxs09qxw";
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore MemberCanBePrivate.Local
        // ReSharper restore InconsistentNaming

        private static void Initialize()
        {
            _wndProc = WindowProc;
            var wc = new WNDCLASS
            {
                lpszClassName = g_szClassName,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc),
                hInstance = GetModuleHandle(IntPtr.Zero)
            };

            RegisterClass(ref wc);

            g_hWnd = CreateWindowEx(0, g_szClassName, "Shader Decompiler", WS_OVERLAPPEDWINDOW, 20, 20, 800, 600,
                (IntPtr)0, (IntPtr)0, GetModuleHandle(IntPtr.Zero), (IntPtr)0);

            g_D3D = Direct3DCreate9(D3D_SDK_VERSION);
            g_D3DPresentParameters = new D3DPRESENT_PARAMETERS
            {
                AutoDepthStencilFormat = D3DFMT_D24S8,
                BackBufferCount = 1,
                BackBufferFormat = D3DFMT_X8R8G8B8,
                BackBufferWidth = 32,
                BackBufferHeight = 32,
                EnableAutoDepthStencil = false,
                Flags = 0,
                FullScreen_RefreshRateInHz = 0,
                hDeviceWindow = g_hWnd,
                PresentationInterval = D3DPRESENT_INTERVAL_IMMEDIATE,
                SwapEffect = D3DSWAPEFFECT_DISCARD,
                Windowed = true
            };

            fixed (D3DPRESENT_PARAMETERS* pD3DPresentParams = &g_D3DPresentParameters)
            {
                fixed (IDirect3DDevice9** ppD3DDevice = &g_D3DDevice)
                {
                    IDirect3D9_CreateDevice(g_D3D, 0, D3DDEVTYPE_NULLREF, g_hWnd, D3DCREATE_SOFTWARE_VERTEXPROCESSING, pD3DPresentParams, ppD3DDevice);
                }
            }
        }

        public static void Decompile(string sFileName)
        {
            Initialize();

            ID3DXEffect* pD3DEffect = null;
            ID3DXBuffer* pD3DError = null;
            ID3DXBuffer* pDisassembler = null;

            D3DXCreateEffectFromFile(g_D3DDevice, sFileName, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, &pD3DEffect, &pD3DError);
            D3DXDisassembleEffect(pD3DEffect, false, &pDisassembler);

            var sShaderSource = Marshal.PtrToStringAnsi(ID3DXBuffer_GetBufferPointer(pDisassembler));

            var sOutFile = Path.Combine(Common.OutputPath, "Shaders", "mb.fx");
            if (!Directory.Exists(Path.Combine(Common.OutputPath, "Shaders")))
                Directory.CreateDirectory(Path.Combine(Common.OutputPath, "Shaders"));
            Win32FileWriter.WriteAllText(sOutFile, Header.Shaders + sShaderSource);

            if (pDisassembler != null) Marshal.Release((IntPtr)pDisassembler);
            if (pD3DError != null) Marshal.Release((IntPtr)pD3DError);
            if (pD3DEffect != null) Marshal.Release((IntPtr)pD3DEffect);

            Release();
        }

        private static IntPtr WindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
        {
            switch (uMsg)
            {
                case WM_DESTROY:
                    PostQuitMessage(0);
                    return IntPtr.Zero;
                default:
                    return DefWindowProc(hWnd, uMsg, wParam, lParam);
            }
        }

        private static void Release()
        {
            Marshal.Release((IntPtr)g_D3DDevice);
            Marshal.Release((IntPtr)g_D3D);
            DestroyWindow(g_hWnd);
            UnregisterClass(g_szClassName, GetModuleHandle(IntPtr.Zero));
        }

    }
}
