using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GlassPane.Native
{
    public static class WindowsAPI
    {
        // Constants
        public const int WM_HOTKEY = 0x0312;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_ALT = 0x0001;
        public const int MOD_SHIFT = 0x0004;
        public const int MOD_WIN = 0x0008;
        public const int SW_MAXIMIZE = 3;
        public const int SW_RESTORE = 9;
        public const int SW_SHOW = 5;

        // Window styles
        public const int WS_MAXIMIZEBOX = 0x00010000;
        public const int WS_MINIMIZEBOX = 0x00020000;
        public const int WS_OVERLAPPEDWINDOW = 0x00CF0000;

        // Virtual Desktop COM interfaces
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("A5CD92FF-29BE-454C-8D04-D82879FB3F1B")]
        public interface IVirtualDesktopManager
        {
            [PreserveSig]
            int IsWindowOnCurrentVirtualDesktop(IntPtr hwnd, out bool pfIsOnCurrentDesktop);

            [PreserveSig]
            int GetWindowDesktopId(IntPtr hwnd, out Guid pDesktopId);

            [PreserveSig]
            int MoveWindowToDesktop(IntPtr hwnd, ref Guid desktopId);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("C5E0CDCA-7B6E-11E0-8C39-C8E0E3128F4E")]
        public interface IVirtualDesktopManagerInternal
        {
            [PreserveSig]
            int GetCount(out int pCount);

            [PreserveSig]
            int MoveViewToDesktop(object pView, object pDesktop);

            [PreserveSig]
            int CanViewMoveDesktops(object pView, out bool pfCanViewMoveDesktops);

            [PreserveSig]
            int GetCurrentDesktop(out object pDesktop);

            [PreserveSig]
            int GetDesktops(out object pDesktops);

            [PreserveSig]
            int GetAdjacentDesktop(object pDesktopReference, int uDirection, out object pAdjacentDesktop);

            [PreserveSig]
            int SwitchDesktop(object pDesktop);

            [PreserveSig]
            int CreateDesktop(out object pDesktop);

            [PreserveSig]
            int RemoveDesktop(object pDesktop, object pDesktopFallback);

            [PreserveSig]
            int FindDesktop(ref Guid desktopId, out object pDesktop);
        }

        // P/Invoke declarations
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("ole32.dll")]
        public static extern int CoCreateInstance(ref Guid rclsid, IntPtr pUnkOuter, uint dwClsContext, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        // Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // Constants for SetWindowPos
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_SHOWWINDOW = 0x0040;

        // Process access rights
        public const uint PROCESS_QUERY_INFORMATION = 0x0400;
        public const uint PROCESS_VM_READ = 0x0010;

        // COM constants
        public const uint CLSCTX_LOCAL_SERVER = 0x4;
        public const uint CLSCTX_INPROC_SERVER = 0x1;

        // Virtual Desktop GUIDs
        public static readonly Guid CLSID_ImmersiveShell = new Guid("C2F03A33-21F5-47FA-B4BB-156362A2F239");
        public static readonly Guid IID_IVirtualDesktopManagerInternal = new Guid("C5E0CDCA-7B6E-11E0-8C39-C8E0E3128F4E");
    }
} 