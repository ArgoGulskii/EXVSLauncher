using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Launcher.Input;
using Launcher.ViewModels;
using Launcher.Views;
using Launcher.Views.Rebind;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Launcher;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        MainViewModel? mainVM = null;

        if (File.Exists("launcher.json"))
        {
            var json = File.ReadAllText("launcher.json");
            mainVM = JsonSerializer.Deserialize<MainViewModel>(json);
        }
        else
        {
            mainVM = MainViewModel.Default();
        }

        if (!Design.IsDesignMode) InputManager.Instance.Start();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVM,
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = mainVM,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}