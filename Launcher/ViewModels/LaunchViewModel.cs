using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Launcher.Output;
using Launcher.Utils;

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

    private Process? serverProcess_;
    private CardQueue? cReaderQueue_;

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
        Main.SettingsViewModel.StopPreviews();

        int visibleCount = 0;
        foreach (var client in Clients)
        {
            if (client.ClientInfo.Enabled && !client.ClientInfo.Hidden) ++visibleCount;
        }

        var assignments = Main.SettingsViewModel.Rows;
        for (int i = 0; i < assignments.Count; ++i)
        {
            var assignment = assignments[i];
            Console.WriteLine($"Window assignment {i}: display = {assignment.Display.DropDownName}, audio = {assignment.Audio.Name}");
        }

        if (assignments.Count < visibleCount)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Failed to launch, not enough places to place game windows (need {visibleCount}, have {assignments.Count})", ButtonEnum.Ok);
            await box.ShowAsync();
            return;
        }

        // Start the server if it's not already running.
        var serverPath = Main.PresetsViewModel.SelectedPreset!.ServerPath;
        if (serverPath == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Server path not specified in preset", ButtonEnum.Ok);
            await box.ShowAsync();
            return;
        }

        serverProcess_ = FindServer(serverPath);
        if (serverProcess_ == null)
        {
            serverProcess_ = new();
            serverProcess_.StartInfo.FileName = serverPath;
            serverProcess_.StartInfo.WorkingDirectory = Path.GetDirectoryName(serverPath);
            try
            {
                serverProcess_.Start();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Failed to start server: {ex.Message}", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
        }

        var outputIdx = 0;
        foreach (var client in Clients)
        {
            if (client.ClientInfo.Enabled && !client.ClientInfo.Hidden)
            {
                client.ClientInfo.Id = outputIdx + 1;
                client.Output = assignments[outputIdx];
                ++outputIdx;
            }
            else
            {
                client.Output = null;
            }
        }


        // Create the shared card reader instance for all rebind windows.
        // If no card reader is found, we set the queue to null.
        ulong CARD_TIMEOUT = 5000;
        cReaderQueue_ = new CardQueue(CARD_TIMEOUT);
        bool success = cReaderQueue_.Setup();
        if (!success)
        {
            Console.WriteLine("CardReader setup failed, disabling reader.");
            cReaderQueue_ = null;
        }
        else
        {
            Console.WriteLine("CardReader ready!");
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
                    client.StartRebind(cReaderQueue_);
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
        try
        {
            if (cReaderQueue_ != null)
            {
                cReaderQueue_.Kill();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to kill card reader thread: {ex.Message}");
        }

        try
        {
            if (serverProcess_ != null)
                serverProcess_.Kill();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to kill server process: {ex.Message}");
        }

        serverProcess_ = null;

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