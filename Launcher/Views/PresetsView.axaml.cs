using Avalonia.Controls;
using Avalonia.Input;
using Launcher.ViewModels;
using System;

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

    public void OnPointerPressed_Add(object? sender, PointerPressedEventArgs e)
    {
        var ctx = Context()!;
        ctx.Presets.Add(new PresetViewModel());
        ctx.SelectedPresetIndex = ctx.Presets.Count - 1;
    }

    public void OnPointerPressed_Remove(object? sender, PointerPressedEventArgs e)
    {
        var ctx = Context()!;

        if (ctx.SelectedPresetIndex < 0 || ctx.SelectedPresetIndex >= ctx.Presets.Count)
            return;

        ctx.Presets.RemoveAt(ctx.SelectedPresetIndex);
        if (ctx.SelectedPresetIndex >= ctx.Presets.Count)
            ctx.SelectedPresetIndex = ctx.Presets.Count - 1;
    }
}