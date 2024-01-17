using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Launcher.ViewModels;

namespace Launcher.Views;

public partial class ClientsView : UserControl
{
    public ClientsView()
    {
        InitializeComponent();
    }

    ClientsViewModel? Context()
    {
        return (ClientsViewModel?)DataContext;
    }

    public void SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Awful hack to reuse TabStrip as buttons.
        // TODO: Manually reimplement the TabStrip styling.
        TabStrip strip = (TabStrip)sender!;
        switch (strip.SelectedIndex)
        {
            case 0: return;
            case 1:
                OnPointerPressed_Add(null, null);
                break;
            case 2:
                OnPointerPressed_Remove(null, null);
                break;
        }

        strip.SelectedIndex = 0;
    }

    public void OnPointerPressed_Add(object? sender, PointerPressedEventArgs? e)
    {
        var ctx = Context();
        if (ctx == null)
            return;
        ctx.Clients.Add(new ClientViewModel());
        ctx.SelectedClientIndex = ctx.Clients.Count - 1;
    }

    public void OnPointerPressed_Remove(object? sender, PointerPressedEventArgs? e)
    {
        var ctx = Context();
        if (ctx == null)
            return;

        if (ctx.SelectedClientIndex < 0 || ctx.SelectedClientIndex >= ctx.Clients.Count)
            return;

        int origIndex = ctx.SelectedClientIndex;
        if (ctx.SelectedClientIndex > 0)
            --ctx.SelectedClientIndex;
        ctx.Clients.RemoveAt(origIndex);
    }
}