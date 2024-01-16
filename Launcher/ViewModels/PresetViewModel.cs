using ReactiveUI;
using System.Reactive.Linq;

namespace Launcher.ViewModels;

public partial class PresetViewModel : ViewModelBase
{
    public PresetViewModel()
    {
        name_ = "Multibox";
        clients_ = new ClientsViewModel
        {
            Clients = new ObservableCollection<ClientViewModel>
            {
                new ClientViewModel { Name = "LM", Path = @"C:\GXX10JPN27\storage\lm", Hidden = true, Enabled = true },
                new ClientViewModel { Name = "PCB 1", Path = @"C:\GXX10JPN27\storage\pcb1", Hidden = false, Enabled = true },
                new ClientViewModel { Name = "PCB 2", Path = @"C:\GXX10JPN27\storage\pcb2", Hidden = false, Enabled = true },
                new ClientViewModel { Name = "PCB 3", Path = @"C:\GXX10JPN27\storage\pcb3", Hidden = false, Enabled = true },
                new ClientViewModel { Name = "PCB 4", Path = @"C:\GXX10JPN27\storage\pcb4", Hidden = false, Enabled = true },
            },
        };

        headerName = this
            .WhenAnyValue(x => x.Name)
            .Select(name => name.Length == 0 ? "?" : name)
            .ToProperty(this, x => x.HeaderName);
    }

    private string name_;
    public string Name
    {
        get => name_;
        set => this.RaiseAndSetIfChanged(ref name_, value);
    }

    readonly ObservableAsPropertyHelper<string> headerName;
    public string HeaderName => headerName.Value;

    private ClientsViewModel clients_;
    public ClientsViewModel Clients
    {
        get => clients_;
        set => this.RaiseAndSetIfChanged(ref clients_, value);
    }

    private string gamePath_;
    public string GamePath
    {
        get => gamePath_;
        set => this.RaiseAndSetIfChanged(ref gamePath_, value);
    }

    private string serverPath_;
    public string ServerPath
    {
        get => serverPath_;
        set => this.RaiseAndSetIfChanged(ref serverPath_, value);
    }
}