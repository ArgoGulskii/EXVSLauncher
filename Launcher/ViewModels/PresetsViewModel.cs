using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Launcher.ViewModels;

[JsonConverter(typeof(PresetsViewModelConverter))]
public partial class PresetsViewModel : ViewModelBase
{
    public PresetsViewModel()
    {
        Presets = new ObservableCollection<PresetViewModel>();
        selectedPresetIndex_ = -1;
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

class PresetsViewModelConverter : JsonConverter<PresetsViewModel>
{
    public override PresetsViewModel Read(
         ref Utf8JsonReader reader,
         Type typeToConvert,
         JsonSerializerOptions options)
    {
        PresetsViewModel result = new();

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        reader.Read();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            result.Presets.Add(JsonSerializer.Deserialize<PresetViewModel>(ref reader, options)!);
            reader.Read();
        }
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        PresetsViewModel pvm,
        JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var preset in pvm.Presets)
        {
            JsonSerializer.Serialize(writer, preset, options);
        }

        writer.WriteEndArray();
    }
}