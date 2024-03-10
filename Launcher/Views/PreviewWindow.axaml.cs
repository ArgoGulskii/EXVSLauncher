using Avalonia.Controls;
using Avalonia.Input;
using Launcher.Output;

namespace Launcher.Views;

public partial class PreviewWindow : Window
{
    public PreviewWindow()
    {
        InitializeComponent();
    }

    public OutputAssignment? Context()
    {
        return (OutputAssignment?)DataContext;
    }

    public void Click(object? sender, PointerPressedEventArgs args)
    {
        Context()!.StopPreview();
    }
}
