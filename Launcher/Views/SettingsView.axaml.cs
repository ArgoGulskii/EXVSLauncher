using Avalonia.Controls;
using Avalonia.Input;
using Launcher.Output;
using Launcher.ViewModels;
using System;

namespace Launcher.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    SettingsViewModel? Context()
    {
        return (SettingsViewModel?)DataContext;
    }

    private void RowUp(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var button = (Button)sender!;
        var output = (OutputAssignment)button.DataContext!;

        var rows = Context()!.Rows;
        int idx = rows.IndexOf(output);
        rows.Move(idx, idx - 1);
        Context()!.UpdateRows();
    }

    private void RowDown(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var button = (Button)sender!;
        var output = (OutputAssignment)button.DataContext!;

        var rows = Context()!.Rows;
        int idx = rows.IndexOf(output);
        rows.Move(idx, idx + 1);
        Context()!.UpdateRows();
    }

    private void RowPreview(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var button = (Button)sender!;

        foreach (var output in Context().Rows)
        {
            output.StopPreview();
        }

        var previewedOutput = (OutputAssignment)button.DataContext!;
        previewedOutput.StartPreview();
    }

    private void RowDelete(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var button = (Button)sender!;
        var output = (OutputAssignment)button.DataContext!;

        var rows = Context()!.Rows;
        rows.Remove(output);
        Context()!.UpdateRows();
    }

    private void RowAdd(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Context()!.RowAdd();
    }
}