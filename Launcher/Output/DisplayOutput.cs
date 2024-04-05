using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using WindowsDisplayAPI;

namespace Launcher.Output;

using HDC = nint;
using HMONITOR = nint;

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;

    public override string ToString()
    {
        return $"{right - left}x{bottom - top}@({left}, {top})";
    }
}

public class DisplayOutput
{
    public DisplayOutput(string name, string devicePath, string? splitName = null)
    {
        DeviceName = name;
        SplitName = splitName;
        DevicePath = devicePath;
    }

    public string DeviceName { get; }
    public string DropDownName
    {
        get
        {
            return SplitName == null ? DeviceName : $"  {DeviceName} ({SplitName})";
        }
    }

    public string? SplitName
    {
        get; set;
    }

    public string DevicePath { get; }

    public DisplayOutput? Parent;

    public RECT Rect;

    public int SplitXIndex;
    public int SplitYIndex;

    public int SplitXCount;
    public int SplitYCount;

    public int Width => Rect.right - Rect.left;
    public int Height => Rect.bottom - Rect.top;

    public int SplitWidth => Width / SplitXCount;
    public int SplitHeight => Height / SplitYCount;

    private static DisplayOutput Split(DisplayOutput original, int x, int y, string splitName)
    {
        var result = new DisplayOutput(original.DeviceName, original.DevicePath, splitName);
        var rect = new RECT();

        rect.left = original.Rect.left + x * original.SplitWidth;
        rect.right = rect.left + original.SplitWidth;
        rect.top = original.Rect.top + y * original.SplitHeight;
        rect.bottom = rect.top + original.SplitHeight;

        result.Rect = rect;
        result.SplitXIndex = x;
        result.SplitYIndex = y;

        result.SplitXCount = 1;
        result.SplitYCount = 1;

        result.Parent = original;
        return result;
    }

    static List<DisplayOutput>? cachedOutputs_;

    public static List<DisplayOutput> EnumerateDisplays()
    {
        lock (typeof(DisplayOutput))
        {
            if (cachedOutputs_ != null) return cachedOutputs_;
            var result = new List<DisplayOutput>();

            foreach (var screen in DisplayScreen.GetScreens())
            {
                var displays = screen.GetDisplays();
                if (displays.Count() != 1)
                {
                    Console.WriteLine($"Skipping screen at {screen.Bounds} because it spans multiple displays");
                    continue;
                }

                var display = displays[0];
                var displayTarget = display.ToPathDisplayTarget();

                var rect = new RECT()
                {
                    left = screen.Bounds.Left,
                    right = screen.Bounds.Right,
                    top = screen.Bounds.Top,
                    bottom = screen.Bounds.Bottom,
                };
                int width = screen.Bounds.Width;
                int height = screen.Bounds.Height;

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

                var displayCandidate = new DisplayOutput(displayTarget.FriendlyName, displayTarget.DevicePath)
                {
                    Rect = rect,
                    SplitXCount = splitX,
                    SplitYCount = splitY,
                };
                result.Add(displayCandidate);

                if (splitX == 2 && splitY == 1)
                {
                    result.Add(Split(displayCandidate, 0, 0, "Left"));
                    result.Add(Split(displayCandidate, 1, 0, "Right"));
                }
                else if (splitX == 2 && splitY == 1)
                {
                    result.Add(Split(displayCandidate, 0, 0, "Top Left"));
                    result.Add(Split(displayCandidate, 1, 0, "Top Right"));
                    result.Add(Split(displayCandidate, 0, 1, "Bottom Left"));
                    result.Add(Split(displayCandidate, 1, 1, "Bottom Right"));
                }
            }

            cachedOutputs_ = result;
            return result;
        }
    }

    private static void CalculateWindowLocationsPerDisplay(List<RECT> output, List<DisplayOutput> displays)
    {
        foreach (var display in displays)
        {
            // TODO: Maintain aspect ratio.
            output.Add(display.Rect);
        }
    }

    private static void CalculateWindowLocationSplit(List<RECT> output, List<DisplayOutput> displays, int xSplit, int ySplit)
    {
        for (int i = 0; i < displays.Count;)
        {
            var display = displays[i];
            if (display.SplitXCount != xSplit || display.SplitYCount != ySplit)
            {
                ++i;
                continue;
            }

            int splitWidth = display.SplitWidth;
            int splitHeight = display.SplitHeight;

            for (int y = 0; y < display.SplitYCount; ++y)
            {
                for (int x = 0; x < display.SplitXCount; ++x)
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

    private static void CalculateWindowLocations2x1(List<RECT> output, List<DisplayOutput> displays)
    {
        var copy = new List<DisplayOutput>(displays);
        CalculateWindowLocationSplit(output, copy, 2, 1);
        CalculateWindowLocationsPerDisplay(output, copy);
    }

    private static void CalculateWindowLocations2x2(List<RECT> output, List<DisplayOutput> displays)
    {
        var copy = new List<DisplayOutput>(displays);
        CalculateWindowLocationSplit(output, copy, 2, 2);
        CalculateWindowLocations2x1(output, copy);
    }

    public static List<RECT>? CalculateWindowLocations(List<DisplayOutput> displays, int windowCount)
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

    public void MoveWindow(HWND handle, bool alwaysOnTop)
    {
        WindowUtils.MoveWindow(handle, Rect, alwaysOnTop);
    }

    public void MoveWindow(Window window, bool alwaysOnTop)
    {
        MoveWindow(window.TryGetPlatformHandle()!.Handle, alwaysOnTop);
    }
}