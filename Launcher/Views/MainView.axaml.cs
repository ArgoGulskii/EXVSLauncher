using Avalonia.Controls;
using Avalonia.Input;
using DialogHostAvalonia;
using Launcher.ViewModels;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };

        var json = JsonSerializer.Serialize(Context(), options);
        var configPath = Path.Join(Directory.GetCurrentDirectory(), "launcher.json");
        File.WriteAllText(configPath, json);
        Console.WriteLine($"Wrote config to {configPath}");
    }

    public async void LaunchButton_onPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SaveButton_onPointerPressed(sender, e);

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