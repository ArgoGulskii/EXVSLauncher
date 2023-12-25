using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;

namespace Launcher.Desktop;

class ConsoleMirror : StreamWriter
{
    public ConsoleMirror(TextWriter original) : base("launcher.log")
    {
        orig = original;
    }

    public override Encoding Encoding { get { return Encoding.UTF8; } }

    public override void Write(string value)
    {
        orig.Write(value);
        base.Write(value);
    }

    public override void WriteLine(string value)
    {
        orig.WriteLine(value);
        base.Write(DateTime.Now.ToString("HH:mm:ss "));
        base.WriteLine(value);
        base.Flush();
    }

    TextWriter orig;
}

class Program
{
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AllocConsole();
        Console.SetOut(new ConsoleMirror(Console.Out));
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}