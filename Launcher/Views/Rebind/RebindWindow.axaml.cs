using Avalonia.Controls;
using Launcher.Input;
using Launcher.ViewModels;

namespace Launcher.Views.Rebind;

public partial class RebindWindow : Window
{
    public RebindWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is RebindViewModel rebindViewModel)
        {
            InputManager.Instance.RemoveRebindWindow(rebindViewModel);
        }

        base.OnClosing(e);
    }
}