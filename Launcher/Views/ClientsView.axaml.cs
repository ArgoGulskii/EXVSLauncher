using Avalonia.Controls;
using Avalonia.Input;
using Launcher.ViewModels;
using System;

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

    public void OnPointerPressed_Add(object? sender, PointerPressedEventArgs e)
    {
        var ctx = Context();
        if (ctx == null)
            return;

        ctx.Clients.Add(new ClientViewModel());
        ctx.SelectedClientIndex = ctx.Clients.Count - 1;
    }

    public void OnPointerPressed_Remove(object? sender, PointerPressedEventArgs e)
    {
        var ctx = Context()!;

        if (ctx.SelectedClientIndex < 0 || ctx.SelectedClientIndex >= ctx.Clients.Count)
            return;

        int origIndex = ctx.SelectedClientIndex;
        if (ctx.SelectedClientIndex > 0)
            --ctx.SelectedClientIndex;
        ctx.Clients.RemoveAt(origIndex);
    }
}