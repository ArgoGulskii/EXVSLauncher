using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Launcher.ViewModels;

public partial class LaunchViewModel : ViewModelBase
{
    public LaunchViewModel()
    {
        Clients = [];
    }

    public static LaunchViewModel FromClients(MainViewModel main)
    {
        var clients = main.PresetsViewModel.SelectedPreset!.Clients.Clients.Select(cvm => ClientLaunchViewModel.FromClient(cvm));
        var result = new LaunchViewModel()
        {
            Main = main,
            Clients = new ObservableCollection<ClientLaunchViewModel>(clients),
        };

        return result;
    }

    public required MainViewModel Main;

    public ObservableCollection<ClientLaunchViewModel> Clients { get; set; }

    public static Process? FindServer(string serverPath)
    {
        var fullPath = Path.GetFullPath(serverPath);
        var serverName = Path.GetFileNameWithoutExtension(serverPath);
        var processes = Process.GetProcessesByName(serverName);

        var serverProcs = processes.Where(process =>
        {
            return Path.GetFullPath(process.MainModule!.FileName) == fullPath;
        });

        return serverProcs.FirstOrDefault();
    }

    public async Task Start()
    {
        int visibleCount = 0;
        foreach (var client in Clients)
        {
            if (client.ClientInfo.Enabled && !client.ClientInfo.Hidden) ++visibleCount;
        }

        var displays = DisplayUtils.EnumerateDisplays();
        var splits = DisplayUtils.CalculateWindowLocations(displays, visibleCount);
        if (splits == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Failed to launch, not enough places to place game windows (need {visibleCount})", ButtonEnum.Ok);
            await box.ShowAsync();
            return;
        }

        // Start the server if it's not already running.
        var serverPath = Main.PresetsViewModel.SelectedPreset.ServerPath;
        if (serverPath == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Server path not specified in preset", ButtonEnum.Ok);
            await box.ShowAsync();
            return;
        }

        if (FindServer(serverPath) == null)
        {
            Process process = new();
            process.StartInfo.FileName = serverPath;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(serverPath);
            process.Start();
        }

        var splitIdx = 0;
        foreach (var client in Clients)
        {
            if (client.ClientInfo.Enabled && !client.ClientInfo.Hidden)
            {
                client.ClientInfo.Id = splitIdx + 1;
                client.WindowLocation = splits[splitIdx++];
            }
            else
            {
                client.WindowLocation = null;
            }
        }

        List<Task> tasks = [];
        WindowUtils.HideTaskbar();
        foreach (var client in Clients)
        {
            async Task lambda()
            {
                if (!client.Enabled) return;

                if (client.Visible && client.AutoRebind)
                {
                    client.StartRebind();
                }

                if (!client.Start(Main)) return;
                await client.WaitForWindow();
                client.MoveGameWindow();
            }
            tasks.Add(lambda());
        }

        await Task.WhenAll(tasks);
    }

    public void Stop()
    {
        foreach (var client in Clients)
        {
            client.Kill();
        }

        foreach (var client in Clients)
        {
            client.Reap();
        }

        Main.LaunchViewModel = null;
    }
}