using System;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    public static MainViewModel Default()
    {
        MainViewModel result = new()
        {
            PresetsViewModel = new PresetsViewModel()
            {
                Presets = [
                    new PresetViewModel()
                    {
                        Name = "Radmin Client",
                        GamePath = "C:\\GXX10JPN27\\vsac25_Release.exe",
                        ServerPath = "C:\\GXX10JPN27\\Server\\Server.exe",
                        Clients = new ClientsViewModel()
                        {
                            Clients = [
                                new ClientViewModel()
                                {
                                    Name = "Radmin",
                                    Mode = 0,
                                    NetworkMode = 0,
                                    NetworkValue = "Radmin VPN",
                                    Path = "C:\\GXX10JPN27\\storage\\radmin",
                                }
                            ]
                        },
                    },
                    new PresetViewModel()
                    {
                        Name = "Multibox",
                        GamePath = "C:\\GXX10JPN27\\vsac25_Release.exe",
                        ServerPath = "C:\\GXX10JPN27\\Server\\Server.exe",
                        Clients = new ClientsViewModel()
                        {
                            Clients = [
                                new ClientViewModel()
                                {
                                    Name = "LM",
                                    Hidden = true,
                                    Mode = 1,
                                    NetworkMode = 1,
                                    NetworkValue = "10.10.220.10",
                                    Path = "C:\\GXX10JPN27\\storage\\lm",
                                },
                                new ClientViewModel()
                                {
                                    Name = "PCB1",
                                    AutoRebind = true,
                                    Mode = 0,
                                    NetworkMode = 1,
                                    NetworkValue = "10.10.220.11",
                                    Path = "C:\\GXX10JPN27\\storage\\pcb1",
                                },
                                new ClientViewModel()
                                {
                                    Name = "PCB2",
                                    AutoRebind = true,
                                    Mode = 0,
                                    NetworkMode = 1,
                                    NetworkValue = "10.10.220.12",
                                    Path = "C:\\GXX10JPN27\\storage\\pcb2",
                                },
                                new ClientViewModel()
                                {
                                    Name = "PCB3",
                                    AutoRebind = true,
                                    Mode = 0,
                                    NetworkMode = 1,
                                    NetworkValue = "10.10.220.13",
                                    Path = "C:\\GXX10JPN27\\storage\\pcb3",
                                },
                                new ClientViewModel()
                                {
                                    Name = "PCB4",
                                    AutoRebind = true,
                                    Mode = 0,
                                    NetworkMode = 1,
                                    NetworkValue = "10.10.220.14",
                                    Path = "C:\\GXX10JPN27\\storage\\pcb4",
                                },
                            ]
                        },
                    }
                ]
            },
            SettingsViewModel = new SettingsViewModel(),
        };
        result.PresetsViewModel.SelectedPresetIndex = 0;
        return result;
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

class MainViewModelConverter : JsonConverter<MainViewModel>
{
    public override MainViewModel Read(
         ref Utf8JsonReader reader,
         Type typeToConvert,
         JsonSerializerOptions options)
    {
        MainViewModel result = new();
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        int selectedPreset = -1;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? propertyName = reader.GetString()?.ToLowerInvariant();
            reader.Read();
            switch (propertyName)
            {
                case "selectedpreset":
                    selectedPreset = JsonSerializer.Deserialize<int>(ref reader, options)!;
                    break;
                case "presets":
                    result.PresetsViewModel = JsonSerializer.Deserialize<PresetsViewModel>(ref reader, options)!;
                    break;
                case "settings":
                    result.SettingsViewModel = JsonSerializer.Deserialize<SettingsViewModel>(ref reader, options)!;
                    break;
                default:
                    throw new JsonException($"Unknown property in MainViewModel: {propertyName}");
            }
        }
        result.PresetsViewModel.SelectedPresetIndex = selectedPreset;
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        MainViewModel mvm,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();


        writer.WritePropertyName("SelectedPreset");
        JsonSerializer.Serialize(writer, mvm.PresetsViewModel.SelectedPresetIndex, options);

        writer.WritePropertyName("Presets");
        JsonSerializer.Serialize(writer, mvm.PresetsViewModel, options);

        writer.WritePropertyName("Settings");
        JsonSerializer.Serialize(writer, mvm.SettingsViewModel, options);

        writer.WriteEndObject();
    }
}