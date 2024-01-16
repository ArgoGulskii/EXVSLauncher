using ReactiveUI;

namespace Launcher.ViewModels;

[JsonConverter(typeof(MainViewModelConverter))]
public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        PresetsViewModel = new PresetsViewModel();
        SettingsViewModel = new SettingsViewModel();
    }

    private bool launchViewDialogVisible_ = false;
    public bool LaunchViewDialogVisible
    {
        get => launchViewDialogVisible_;
        set
        {
            this.RaiseAndSetIfChanged(ref launchViewDialogVisible_, value);
        }
    }

    private LaunchViewModel? launchViewModel_ = null;
    public LaunchViewModel? LaunchViewModel
    {
        get => launchViewModel_;
        set
        {
            LaunchViewDialogVisible = false;
            this.RaiseAndSetIfChanged(ref launchViewModel_, value);
        }
    }

    public PresetsViewModel PresetsViewModel { get; set; }
    public SettingsViewModel SettingsViewModel { get; set; }
}