using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using ReactiveUI;
using Avalonia.Media;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Linq;
using Launcher.Output;
using Avalonia.Input;
using System.Diagnostics;

namespace Launcher.ViewModels;

[JsonConverter(typeof(SettingsViewModelConverter))]
public partial class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel()
    {
        Rows = new();
    }

    public void StopPreviews()
    {
        foreach (var output in Rows)
        {
            output.StopPreview();
        }
    }

    public void UpdateRows()
    {
        Console.WriteLine($"UpdateRows: count = {Rows.Count}");
        if (Rows.Count == 0) return;
        else if (Rows.Count == 1)
        {
            Rows[0].IsFirst = true;
            Rows[0].IsLast = true;
            return;
        }
        else
        {
            for (int i = 1; i < Rows.Count - 1; ++i)
            {
                Rows[i].IsFirst = false;
                Rows[i].IsLast = false;
            }

            Rows[0].IsFirst = true;
            Rows[0].IsLast = false;
            Rows[Rows.Count - 1].IsFirst = false;
            Rows[Rows.Count - 1].IsLast = true;
        }

        this.RaisePropertyChanged("Rows");
    }

    public void RowAdd()
    {
        Rows.Add(new OutputAssignment());
        UpdateRows();
        this.RaisePropertyChanged("Rows");
    }

    public ObservableCollection<OutputAssignment> Rows { get; set; }

    private int addButtonRow_;
    public int AddButtonRow
    {
        get => addButtonRow_;
        set => this.RaiseAndSetIfChanged(ref addButtonRow_, value);
    }

    private bool addButtonVisible_;
    public bool AddButtonVisible
    {
        get => addButtonVisible_;
        set => this.RaiseAndSetIfChanged(ref addButtonVisible_, value);
    }
}

class SettingsViewModelConverter : JsonConverter<SettingsViewModel>
{
    private bool ReadMapping(out OutputAssignment? row, ref Utf8JsonReader reader)
    {
        row = null;

        reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) return false;

        var displayId = "";
        bool displayHasSplit = false;
        var displaySplitX = 0;
        var displaySplitY = 0;
        var audioId = "";

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException($"Unexpected token type: {reader.TokenType}");

            string propertyName = reader.GetString()!.ToLowerInvariant();
            switch (propertyName)
            {
                case "display":
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException($"Unexpected token type: {reader.TokenType}");
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject) break;
                        if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException($"Unexpected token type: {reader.TokenType}");
                        string displayPropertyName = reader.GetString()!.ToLowerInvariant();
                        switch (displayPropertyName)
                        {
                            case "id":
                                reader.Read();
                                displayId = reader.GetString();
                                break;

                            case "split":
                                reader.Read();
                                if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException($"Unexpected token type: {reader.TokenType}");
                                while (reader.Read())
                                {
                                    if (reader.TokenType == JsonTokenType.EndObject) break;
                                    if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException($"Unexpected token type: {reader.TokenType}");
                                    string splitPropertyName = reader.GetString()!.ToLowerInvariant();
                                    switch (splitPropertyName)
                                    {
                                        case "x":
                                            displayHasSplit = true;
                                            reader.Read();
                                            if (reader.TokenType != JsonTokenType.Number) throw new JsonException($"Unexpected token type: {reader.TokenType}");
                                            displaySplitX = reader.GetInt32();
                                            break;

                                        case "y":
                                            displayHasSplit = true;
                                            reader.Read();
                                            if (reader.TokenType != JsonTokenType.Number) throw new JsonException($"Unexpected token type: {reader.TokenType}");
                                            displaySplitY = reader.GetInt32();
                                            break;

                                        default:
                                            throw new JsonException($"Unexepcted split field: {splitPropertyName}");
                                    }
                                }
                                break;

                            default:
                                throw new JsonException($"Unexepcted display field: {displayPropertyName}");
                        }
                    }
                    break;

                case "audio":
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException($"Unexpected token type: {reader.TokenType}");
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject) break;
                        if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException($"Unexpected token type: {reader.TokenType}");
                        string audioPropertyName = reader.GetString()!.ToLowerInvariant();
                        switch (audioPropertyName)
                        {
                            case "id":
                                reader.Read();
                                audioId = reader.GetString();
                                break;

                            default:
                                throw new JsonException($"Unexepcted display field: {audioPropertyName}");
                        }
                    }
                    break;
            }
        }

        // Validate the row.
        var displayOutputs = DisplayOutput.EnumerateDisplays();
        var audioOutputs = AudioOutput.EnumerateOutputs();

        DisplayOutput? foundDisplay = null;
        int foundDisplayIdx = 0;

        AudioOutput? foundAudio = null;
        int foundAudioIdx = 0;

        for (int i = 0; i < displayOutputs.Count; ++i)
        {
            var displayOutput = displayOutputs[i];
            if (displayOutput.DevicePath != displayId) continue;
            if (displayHasSplit)
            {
                if (displayOutput.Parent == null) continue;
                if (displayOutput.SplitXIndex != displaySplitX) continue;
                if (displayOutput.SplitYIndex != displaySplitY) continue;
            }
            else
            {
                if (displayOutput.Parent != null) continue;
            }
            foundDisplay = displayOutput;
            foundDisplayIdx = i;
            break;
        }

        for (int i = 0; i < audioOutputs.Count; ++i)
        {
            var audioOutput = audioOutputs[i];
            if (audioOutput.DevicePath != audioId) continue;
            foundAudio = audioOutput;
            foundAudioIdx = i;
            break;
        }

        if (foundDisplay == null || foundAudio == null)
        {
            row = null;
        }
        else
        {
            row = new OutputAssignment
            {
                AudioIndex = foundAudioIdx,
                DisplayIndex = foundDisplayIdx,
            };
        }
        return true;
    }

    public override SettingsViewModel Read(
         ref Utf8JsonReader reader,
         Type typeToConvert,
         JsonSerializerOptions options)
    {
        SettingsViewModel result = new();

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return result;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }

            string propertyName = reader.GetString()!.ToLowerInvariant();
            switch (propertyName)
            {
                case "mappings":
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.StartArray)
                    {
                        throw new JsonException($"Unexpected token type: {reader.TokenType}");
                    }

                    OutputAssignment? row;
                    while (ReadMapping(out row, ref reader))
                    {
                        if (row != null)
                            result.Rows.Add(row);
                    }

                    if (reader.TokenType != JsonTokenType.EndArray)
                    {
                        throw new JsonException($"Unexpected token type: {reader.TokenType}");
                    }
                    break;

                default:
                    throw new JsonException($"Unknown property in SettingsViewModel: {propertyName}");
            }
        }
        result.UpdateRows();
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        SettingsViewModel svm,
        JsonSerializerOptions options)
    {
        var displayOutputs = DisplayOutput.EnumerateDisplays();
        var audioOutputs = AudioOutput.EnumerateOutputs();
        writer.WriteStartObject();
        writer.WriteStartArray("Mappings");
        foreach (var row in svm.Rows)
        {
            writer.WriteStartObject();

            writer.WriteStartObject("Display");
            var display = displayOutputs[row.DisplayIndex];
            writer.WriteString("Id", display.DevicePath);
            if (display.Parent != null)
            {
                writer.WriteStartObject("Split");
                writer.WriteNumber("X", display.SplitXIndex);
                writer.WriteNumber("Y", display.SplitYIndex);
                writer.WriteEndObject();
            }
            writer.WriteEndObject();

            writer.WriteStartObject("Audio");
            var audio = audioOutputs[row.AudioIndex];
            writer.WriteString("Id", audio.DevicePath);
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}