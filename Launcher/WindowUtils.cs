global using HWND = System.IntPtr;

using Avalonia;
using Avalonia.Styling;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Runtime.InteropServices;

using System.Text;
using System;
using System.Globalization;
using Launcher.Output;

namespace Launcher;

internal class WindowUtils
{
    private delegate bool EnumWindowsProc(HWND hWnd, System.IntPtr lParam);

    public delegate bool WindowFilter(HWND hwnd);

    [DllImport("USER32.DLL")]
    private static extern bool EnumWindows(EnumWindowsProc enumFunc, System.IntPtr lParam);

    [DllImport("USER32.DLL")]
    private static extern int FindWindow(string className, string windowText);

    [DllImport("USER32.DLL")]
    private static extern int ShowWindow(HWND hWnd, int command);

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 1;

    [DllImport("USER32.DLL")]
    private static extern int GetWindowThreadProcessId(HWND hWnd, ref int lpdwProcessId);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowLongA(HWND hWnd, int nIndex);

    [DllImport("USER32.DLL")]
    private static extern int SetWindowLongA(HWND hWnd, int nIndex, int dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_CAPTION = 0x00C00000;
    private const int WS_MAXIMIZEBOX = 0x00010000;
    private const int WS_MINIMIZEBOX = 0x00020000;
    private const int WS_THICKFRAME = 0x00040000;
    private const int WS_SYSMENU = 0x00080000;

    [DllImport("USER32.DLL")]
    private static extern bool SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

    private const HWND HWND_BOTTOM = 1;
    private const HWND HWND_TOP = 0;
    private const HWND HWND_TOPMOST = -1;
    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOOWNERZORDER = 0x0200;
    private const int SWP_SHOWWINDOW = 0x0040;
    private const int SWP_HIDEWINDOW = 0x0080;

    [DllImport("USER32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
    static extern int GetClassName(HWND hWnd, StringBuilder lpClassName, int nMaxCount);

    public static List<HWND> GetWindows(WindowFilter filter)
    {
        var result = new List<HWND>();
        EnumWindows(delegate (HWND hwnd, System.IntPtr lParam)
        {
            if (filter(hwnd))
            {
                result.Add(hwnd);
            }
            return true;
        }, 0);
        return result;
    }

    public static List<HWND> GetWindows()
    {
        var result = GetWindows(hwnd => true);
        return result;
    }

    public static List<HWND> GetProcessWindows(Process process, string? className = null)
    {
        var result = GetWindows(hwnd =>
        {
            int processId = 0;
            int threadId = GetWindowThreadProcessId(hwnd, ref processId);
            if (process.Id != processId) return false;

            if (className == null) return true;

            StringBuilder windowClassName = new StringBuilder(256);
            int rc = GetClassName(hwnd, windowClassName, windowClassName.Capacity);
            if (rc == 0) return false;
            windowClassName.Length = rc;
            return windowClassName.ToString() == className;
        });
        return result;
    }

    public static void MoveWindow(HWND window, RECT location, bool alwaysOnTop)
    {
        // TODO: Instead of making windows always on top, monitor the foreground window with SetWinEventHook
        //       and reposition whenever a game window becomes foreground.
        HWND insertAfter = alwaysOnTop ? HWND_TOPMOST : HWND_TOP;
        int style = GetWindowLongA(window, GWL_STYLE);
        style &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);
        SetWindowLongA(window, GWL_STYLE, style);
        SetWindowPos(window, insertAfter, location.left, location.top, location.right - location.left, location.bottom - location.top, SWP_SHOWWINDOW);
    }

    public static void HideWindow(HWND window)
    {
        SetWindowPos(window, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
    }

    private static HWND GetTaskbar()
    {
        return FindWindow("Shell_TrayWnd", "");
    }

    private static HWND GetSecondaryTaskbar()
    {
        return FindWindow("Shell_TrayWnd", "");
    }

    public static void HideTaskbar()
    {
        ShowWindow(GetTaskbar(), SW_HIDE);

        var secondary = GetSecondaryTaskbar();
        if (secondary != 0) ShowWindow(secondary, SW_HIDE);
    }

    public static void ShowTaskbar()
    {
        ShowWindow(GetTaskbar(), SW_SHOW);

        var secondary = GetSecondaryTaskbar();
        if (secondary != 0) ShowWindow(secondary, SW_SHOW);
    }
}