using Avalonia;
using Avalonia.Controls;
using DynamicData;
using Launcher;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Launcher;

using HDC = System.IntPtr;
using HMONITOR = System.IntPtr;

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}

internal class DisplayCandidate
{
    public HMONITOR Display;
    public RECT Rect;
    public int SplitX;
    public int SplitY;

    public int Width => Rect.right - Rect.left;
    public int Height => Rect.bottom - Rect.top;

    public int SplitWidth => Width / SplitX;
    public int SplitHeight => Height / SplitY;
}

internal class DisplayUtils
{
    private delegate bool MonitorEnumProc(HMONITOR monitor, HDC hdc, ref RECT rect, System.IntPtr dwData);

    [DllImport("USER32.DLL")]
    private static extern int EnumDisplayMonitors(HDC hdc, System.IntPtr clipRect, MonitorEnumProc proc, System.IntPtr dwData);

    private static bool EnumDisplayMonitorCallback(HMONITOR monitor, HDC hdc, ref RECT rect, System.IntPtr dwData)
    {
        var handle = GCHandle.FromIntPtr(dwData);
        var result = (List<DisplayCandidate>)handle.Target!;
        int width = rect.right - rect.left;
        int height = rect.bottom - rect.top;

        int splitX = 0;
        int splitY = 0;

        if (width / 32 * 9 >= height)
        {
            // 2x1 split
            splitX = 2;
            splitY = 1;
        }
        else if (width >= 3840 && height >= 2160)
        {
            // 2x2 split
            splitX = 2;
            splitY = 2;
        }

        Debug.WriteLine($"Found monitor: {width}x{height} @ {rect.left}, {rect.top} (split = {splitX}, {splitY})");

        var display = new DisplayCandidate
        {
            Display = monitor,
            Rect = rect,
            SplitX = splitX,
            SplitY = splitY,
        };
        result.Add(display);
        return true;
    }

    public static List<DisplayCandidate> EnumerateDisplays()
    {
        var result = new List<DisplayCandidate>();
        var handle = GCHandle.Alloc(result);
        int rc = EnumDisplayMonitors(0, 0, EnumDisplayMonitorCallback, GCHandle.ToIntPtr(handle));
        handle.Free();

        return result;
    }

    private static void CalculateWindowLocationsPerDisplay(List<RECT> output, List<DisplayCandidate> displays)
    {
        foreach (var display in displays)
        {
            // TODO: Maintain aspect ratio.
            output.Add(display.Rect);
        }
    }

    private static void CalculateWindowLocationSplit(List<RECT> output, List<DisplayCandidate> displays, int xSplit, int ySplit)
    {
        for (int i = 0; i < displays.Count;)
        {
            var display = displays[i];
            if (display.SplitX != xSplit || display.SplitY != ySplit)
            {
                ++i;
                continue;
            }

            int splitWidth = display.SplitWidth;
            int splitHeight = display.SplitHeight;

            for (int y = 0; y < display.SplitY; ++y)
            {
                for (int x = 0; x < display.SplitX; ++x)
                {
                    // TODO: Make the RECT have an aspect ratio of 16:9.
                    RECT rect = new RECT
                    {
                        left = display.Rect.left + x * splitWidth,
                        top = display.Rect.top + y * splitHeight,
                        right = display.Rect.left + (x + 1) * splitWidth,
                        bottom = display.Rect.top + (y + 1) * splitHeight,
                    };

                    output.Add(rect);
                }
            }

            displays.RemoveAt(i);
        }
    }

    private static void CalculateWindowLocations2x1(List<RECT> output, List<DisplayCandidate> displays)
    {
        var copy = new List<DisplayCandidate>(displays);
        CalculateWindowLocationSplit(output, copy, 2, 1);
        CalculateWindowLocationsPerDisplay(output, copy);
    }

    private static void CalculateWindowLocations2x2(List<RECT> output, List<DisplayCandidate> displays)
    {
        var copy = new List<DisplayCandidate>(displays);
        CalculateWindowLocationSplit(output, copy, 2, 2);
        CalculateWindowLocations2x1(output, copy);
    }

    public static List<RECT>? CalculateWindowLocations(List<DisplayCandidate> displays, int windowCount)
    {
        var output = new List<RECT>();
        CalculateWindowLocationsPerDisplay(output, displays);
        if (output.Count >= windowCount) return output;

        output.Clear();
        CalculateWindowLocations2x1(output, displays);
        if (output.Count >= windowCount) return output;

        output.Clear();
        CalculateWindowLocations2x2(output, displays);
        if (output.Count >= windowCount) return output;

        return null;
    }
}