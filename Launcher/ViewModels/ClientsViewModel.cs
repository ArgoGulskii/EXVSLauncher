using ReactiveUI;
using System.Collections.ObjectModel;

namespace Launcher.ViewModels;

public partial class ClientsViewModel : ViewModelBase
{
    public ClientsViewModel()
    {
        Clients = new ObservableCollection<ClientViewModel>
        {
            new ClientViewModel { Name = "LM", Path = @"C:\GXX10JPN27\storage\lm", Hidden = true, Enabled = true },
            new ClientViewModel { Name = "PCB 1", Path = @"C:\GXX10JPN27\storage\pcb1", Hidden = false, Enabled = true },
            new ClientViewModel { Name = "PCB 2", Path = @"C:\GXX10JPN27\storage\pcb2", Hidden = false, Enabled = true },
            new ClientViewModel { Name = "PCB 3", Path = @"C:\GXX10JPN27\storage\pcb3", Hidden = false, Enabled = true },
            new ClientViewModel { Name = "PCB 4", Path = @"C:\GXX10JPN27\storage\pcb4", Hidden = false, Enabled = true },
        };
    }

    public ObservableCollection<ClientViewModel> Clients { get; set; }

    int selectedClientIndex_;
    public int SelectedClientIndex
    {
        get => selectedClientIndex_;
        set => this.RaiseAndSetIfChanged(ref selectedClientIndex_, value);
    }
}