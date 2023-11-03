using Avalonia.Controls;
using Avalonia.Input;
using Launcher.ViewModels;
using System;

namespace Launcher.Views;

public partial class LaunchView : UserControl
{
    public LaunchView()
    {
        InitializeComponent();
    }

    LaunchViewModel? Context()
    {
        return (LaunchViewModel?)DataContext;
    }

    public void OnPointerPressed_Stop(object? sender, PointerPressedEventArgs e)
    {
        Context()!.Stop();
    }
}