using Avalonia.Threading;
using Launcher.Input;
using Launcher.Views.Rebind;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Launcher.ViewModels;

enum LaunchState
{
    Launching,
    PreLaunchBind,
    Launched,
    PostLaunchBind,
    Dead,
}

public class ClientLaunchViewModel : ViewModelBase
{
    public required ClientViewModel ClientInfo;

    public string Name => ClientInfo.Name;
    public bool Enabled => ClientInfo.Enabled;
    public bool Visible => ClientInfo.Enabled && !ClientInfo.Hidden;
    public bool AutoRebind => ClientInfo.AutoRebind;

    private string stateText_ = "Launching";
    public string StateText
    {
        get => stateText_;
        set => this.RaiseAndSetIfChanged(ref stateText_, value);
    }

    private Process? process_;
    private HWND window_;

    private RebindWindow? rebindWindow_;

    public RECT? WindowLocation { get; set; }

    public static ClientLaunchViewModel FromClient(ClientViewModel cvm)
    {
        var result = new ClientLaunchViewModel
        {
            ClientInfo = cvm,
        };

        if (!result.ClientInfo.Enabled)
            result.StateText = "Disabled";

        return result;
    }

    public bool Start(MainViewModel mvm)
    {
        if (!ClientInfo.Enabled) return true;

        // TODO: Write the config file.

        var gamePath = ClientInfo.GamePath ?? "";
        if (gamePath == "") gamePath = mvm.PresetsViewModel.SelectedPreset!.GamePath;

        Process process = new();
        process.StartInfo.FileName = gamePath;
        process.StartInfo.ArgumentList.Add(ClientInfo.Path);
        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(gamePath);
        process.EnableRaisingEvents = true;
        process.Exited += OnProcessExit;

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            StateText = "Failed to launch: " + ex.Message;
            return false;
        }
        StateText = "Launched";
        process_ = process;

        return true;
    }

    public void Kill()
    {
        if (process_ == null) return;
        process_.Kill();
    }

    public void Reap()
    {
        if (process_ == null) return;
        process_.WaitForExit();
        process_ = null;
    }

    public void OnProcessExit(object? sender, EventArgs e)
    {
        Debug.WriteLine("Process exited!");
        StateText = "Exited";

        WindowUtils.ShowTaskbar();
        if (rebindWindow_ != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                rebindWindow_.Close();
                rebindWindow_ = null;
            });
        }
    }

    public void StartRebind()
    {
        rebindWindow_ = new RebindWindow();
        RebindViewModel rebindViewModel_ = new(ClientInfo.Id, rebindWindow_, Path.Combine(ClientInfo.Path, "config.ini"));
        rebindWindow_.DataContext = rebindViewModel_;
        InputManager.Instance.AddRebindWindow(rebindViewModel_);
        rebindWindow_.Show();

        HWND rebindWindowHandle = rebindWindow_.TryGetPlatformHandle()!.Handle;
        WindowUtils.MoveWindow(rebindWindowHandle, (RECT)WindowLocation!, true);
    }

    public async Task<HWND> WaitForWindow()
    {
        if (process_ == null) return 0;

        window_ = await Task.Run(() =>
        {
            // TODO: Timeout?
            while (true)
            {
                var process = process_;
                if (process == null) return 0;
                var list = WindowUtils.GetProcessWindows(process, "nuFoundation.Window");
                if (list.Count != 0) return list.First();
            }
        });
        return window_;
    }

    public void MoveGameWindow()
    {
        if (WindowLocation != null)
        {
            WindowUtils.MoveWindow(window_, (RECT)WindowLocation, false);
        }
        else
        {
            WindowUtils.HideWindow(window_);
        }
    }
}