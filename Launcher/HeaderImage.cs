using Avalonia.Media.Imaging;
using System;
using System.IO;

namespace Launcher;

internal class HeaderImage
{
    private static Bitmap? cachedBitmap_ = GetOnce();
    private static Bitmap? GetOnce()
    {
        var imagePath = Path.Join(System.AppContext.BaseDirectory, "header.png");
        if (Path.Exists(imagePath))
        {
            Console.WriteLine($"Found header image at {imagePath}");
            return new Bitmap(imagePath);
        }

        Console.WriteLine($"Failed to find header image at {imagePath}");
        return null;
    }

    public static Bitmap? Get()
    {
        return cachedBitmap_;
    }
}