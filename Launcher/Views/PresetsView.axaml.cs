using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Launcher.ViewModels;

namespace Launcher.Views;

public partial class PresetsView : UserControl
{
    public PresetsView()
    {
        InitializeComponent();
    }

    PresetsViewModel? Context()
    {
        return (PresetsViewModel?)DataContext;
    }

    public void SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Awful hack to reuse TabStrip as buttons.
        // TODO: Manually reimplement the TabStrip styling.
        TabStrip strip = (TabStrip)sender!;
        switch (strip.SelectedIndex)
        {
            case 0: return;
            case 1:
                OnPointerPressed_Add(null, null);
                break;
            case 2:
                OnPointerPressed_Remove(null, null);
                break;
        }

        strip.SelectedIndex = 0;
    }

    public void OnPointerPressed_Add(object? sender, PointerPressedEventArgs? e)
    {
        var ctx = Context()!;
        ctx.Presets.Add(new PresetViewModel());
        ctx.SelectedPresetIndex = ctx.Presets.Count - 1;
    }

    public void OnPointerPressed_Remove(object? sender, PointerPressedEventArgs? e)
    {
        var ctx = Context()!;

        if (ctx.SelectedPresetIndex < 0 || ctx.SelectedPresetIndex >= ctx.Presets.Count)
            return;

        int origIndex = ctx.SelectedPresetIndex;
        if (ctx.SelectedPresetIndex > 0)
            --ctx.SelectedPresetIndex;
        ctx.Presets.RemoveAt(origIndex);
    }
}