using System.Drawing;
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
            return new Bitmap(imagePath);
        }
        return null;
    }

    public static Bitmap? Get()
    {
        return cachedBitmap_;
    }
}