using Avalonia.Controls;
using Avalonia.Input;
using DialogHostAvalonia;
using Launcher.ViewModels;
using System;
using System.Diagnostics;

namespace Launcher.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
    MainViewModel? Context()
    {
        return (MainViewModel?)DataContext;
    }

    public void SaveButton_onPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Debug.WriteLine("TODO: Save button pressed");
    }

    public async void LaunchButton_onPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var launchViewModel = LaunchViewModel.FromClients(Context()!);
        var view = new Launcher.Views.LaunchView
        {
            DataContext = launchViewModel,
        };
        var dialogResult = DialogHost.Show(view);
        await launchViewModel.Start();
        await dialogResult;
    }
}