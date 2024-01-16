using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Launcher.ViewModels;

[JsonConverter(typeof(PresetViewModelConverter))]
public partial class PresetViewModel : ViewModelBase
{
    public PresetViewModel()
    {
        name_ = "Preset";
        gamePath_ = "";
        serverPath_ = "";

        clients_ = new ClientsViewModel();

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

class PresetViewModelConverter : JsonConverter<PresetViewModel>
{
    public override PresetViewModel Read(
         ref Utf8JsonReader reader,
         Type typeToConvert,
         JsonSerializerOptions options)
    {
        PresetViewModel result = new();

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
                throw new JsonException();
            }

            string? propertyName = reader.GetString()?.ToLowerInvariant();
            reader.Read();
            switch (propertyName)
            {
                case "name":
                    result.Name = JsonSerializer.Deserialize<string>(ref reader, options)!;
                    break;
                case "gamepath":
                    result.GamePath = JsonSerializer.Deserialize<string>(ref reader, options)!;
                    break;
                case "serverpath":
                    result.ServerPath = JsonSerializer.Deserialize<string>(ref reader, options)!;
                    break;
                case "clients":
                    result.Clients = JsonSerializer.Deserialize<ClientsViewModel>(ref reader, options)!;
                    break;
                default:
                    throw new JsonException($"Unknown property in PresetViewModel: {propertyName}");
            }
        }
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        PresetViewModel pvm,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Name");
        JsonSerializer.Serialize(writer, pvm.Name, options);

        writer.WritePropertyName("GamePath");
        JsonSerializer.Serialize(writer, pvm.GamePath, options);

        writer.WritePropertyName("ServerPath");
        JsonSerializer.Serialize(writer, pvm.ServerPath, options);

        writer.WritePropertyName("Clients");
        JsonSerializer.Serialize(writer, pvm.Clients, options);

        writer.WriteEndObject();
    }
}