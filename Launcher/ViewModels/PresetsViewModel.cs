using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace Launcher.ViewModels;

public partial class PresetsViewModel : ViewModelBase
{
    public PresetsViewModel()
    {
        Presets = new ObservableCollection<PresetViewModel>
        {
            new PresetViewModel {
                Name = "Multibox",
                Clients = new ClientsViewModel
                {
                    Clients = new ObservableCollection<ClientViewModel>
                    {
                        new ClientViewModel
                        {
                            Name = "LM",
                            Path = @"C:\GXX10JPN27\storage\lm",
                            Hidden = true,
                            Enabled = true,
                            AutoRebind = false,
                            Mode = 1,
                            NetworkMode = 1,
                            NetworkValue = "10.10.220.5"
                        },
                        new ClientViewModel
                        {
                            Name = "PCB 1",
                            Path = @"C:\GXX10JPN27\storage\pcb1",
                            Hidden = false,
                            Enabled = true,
                            AutoRebind = true,
                            Mode = 0,
                            NetworkMode = 1,
                            NetworkValue = "10.10.220.1"
                        },
                        new ClientViewModel
                        {
                            Name = "PCB 2",
                            Path = @"C:\GXX10JPN27\storage\pcb2",
                            Hidden = false,
                            Enabled = true,
                            AutoRebind = true,
                            Mode = 0,
                            NetworkMode = 1,
                            NetworkValue = "10.10.220.2"
                        },
                        new ClientViewModel
                        {
                            Name = "PCB 3",
                            Path = @"C:\GXX10JPN27\storage\pcb3",
                            Hidden = false,
                            Enabled = true,
                            AutoRebind = true,
                            Mode = 0,
                            NetworkMode = 1,
                            NetworkValue = "10.10.220.3"
                        },
                        new ClientViewModel
                        {
                            Name = "PCB 4",
                            Path = @"C:\GXX10JPN27\storage\pcb4",
                            Hidden = false,
                            Enabled = true,
                            AutoRebind = true,
                            Mode = 0,
                            NetworkMode = 1,
                            NetworkValue = "10.10.220.4"
                        },
                    },
                },
            },
            new PresetViewModel
            {
                Name = "Radmin Client",
                Clients = new ClientsViewModel
                {
                    Clients = new ObservableCollection<ClientViewModel>
                    {
                        new ClientViewModel
                        {
                            Name = "Radmin Client",
                            Path = @"C:\GXX10JPN27\storage\radmin",
                            Hidden = false,
                            Enabled = true,
                            AutoRebind = false,
                            Mode = 0,
                            NetworkMode = 0,
                            NetworkValue = "Radmin VPN"
                        },
                    },
                },
            },
        };

        selectedPreset = this
            .WhenAnyValue(x => x.SelectedPresetIndex)
            .Select(idx => Presets.ElementAtOrDefault(idx))
            .ToProperty(this, x => x.SelectedPreset);
    }

    public ObservableCollection<PresetViewModel> Presets { get; set; }

    public int selectedPresetIndex_;
    public int SelectedPresetIndex
    {
        get => selectedPresetIndex_;
        set => this.RaiseAndSetIfChanged(ref selectedPresetIndex_, value);
    }

    readonly ObservableAsPropertyHelper<PresetViewModel?> selectedPreset;
    public PresetViewModel? SelectedPreset => selectedPreset.Value;
}