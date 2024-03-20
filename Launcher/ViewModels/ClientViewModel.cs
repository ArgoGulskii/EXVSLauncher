using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Launcher.ViewModels;

[JsonConverter(typeof(ClientViewModelConverter))]
public partial class ClientViewModel : ViewModelBase
{
    public ClientViewModel()
    {
        name_ = "";
        Mode = 0;
        NetworkMode = 0;
        NetworkValue = "";
        Path = "";
        Hidden = false;
        Enabled = true;
        AutoRebind = false;

        DefaultCard = "";
        ServerIP = "localhost";
        ServerPort = "80";

        Id = 0;

        headerName = this
            .WhenAnyValue(x => x.Name)
            .Select(name => name.Length == 0 ? "?" : name)
            .ToProperty(this, x => x.HeaderName);
    }

    readonly ObservableAsPropertyHelper<string> headerName;
    public string HeaderName => headerName.Value;

    private string name_;
    public string Name
    {
        get => name_;
        set => this.RaiseAndSetIfChanged(ref name_, value);
    }

    public int Mode { get; set; }

    public int NetworkMode { get; set; }
    public string NetworkValue { get; set; }

    public string Path { get; set; }

    public string? GamePath { get; set; }

    private bool hidden_;
    public bool Hidden
    {
        get => hidden_;
        set => this.RaiseAndSetIfChanged(ref hidden_, value);
    }

    public bool Enabled { get; set; }
    public bool AutoRebind { get; set; }

    public string DefaultCard { get; set; }
    public string ServerIP { get; set; }
    public string ServerPort { get; set; }


    // Metadata for launching, not used for configuration
    public int Id { get; set; }
}


class ClientViewModelConverter : JsonConverter<ClientViewModel>
{
    public override ClientViewModel Read(
         ref Utf8JsonReader reader,
         Type typeToConvert,
         JsonSerializerOptions options)
    {
        ClientViewModel result = new();

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
                case "mode":
                    result.Mode = JsonSerializer.Deserialize<int>(ref reader, options);
                    break;
                case "networkmode":
                    result.NetworkMode = JsonSerializer.Deserialize<int>(ref reader, options);
                    break;
                case "networkvalue":
                    result.NetworkValue = JsonSerializer.Deserialize<string>(ref reader, options)!;
                    break;
                case "path":
                    result.Path = JsonSerializer.Deserialize<string>(ref reader, options)!;
                    break;
                case "gamepath":
                    result.GamePath = JsonSerializer.Deserialize<string>(ref reader, options)!;
                    break;
                case "hidden":
                    result.Hidden = JsonSerializer.Deserialize<bool>(ref reader, options);
                    break;
                case "enabled":
                    result.Enabled = JsonSerializer.Deserialize<bool>(ref reader, options);
                    break;
                case "autorebind":
                    result.AutoRebind = JsonSerializer.Deserialize<bool>(ref reader, options);
                    break;
                case "defaultcard":
                    result.DefaultCard = JsonSerializer.Deserialize<string>(ref reader, options);
                    break;
                case "serverip":
                    result.ServerIP = JsonSerializer.Deserialize<string>(ref reader, options);
                    break;
                case "serverport":
                    result.ServerPort = JsonSerializer.Deserialize<string>(ref reader, options);
                    break;
                default:
                    throw new JsonException($"Unknown property in ClientViewModel: {propertyName}");
            }
        }
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        ClientViewModel mvm,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Name");
        JsonSerializer.Serialize(writer, mvm.Name, options);

        writer.WritePropertyName("Mode");
        JsonSerializer.Serialize(writer, mvm.Mode, options);

        writer.WritePropertyName("NetworkMode");
        JsonSerializer.Serialize(writer, mvm.NetworkMode, options);

        writer.WritePropertyName("NetworkValue");
        JsonSerializer.Serialize(writer, mvm.NetworkValue, options);

        writer.WritePropertyName("Path");
        JsonSerializer.Serialize(writer, mvm.Path, options);

        writer.WritePropertyName("GamePath");
        JsonSerializer.Serialize(writer, mvm.GamePath, options);

        writer.WritePropertyName("Hidden");
        JsonSerializer.Serialize(writer, mvm.Hidden, options);

        writer.WritePropertyName("Enabled");
        JsonSerializer.Serialize(writer, mvm.Enabled, options);

        writer.WritePropertyName("AutoRebind");
        JsonSerializer.Serialize(writer, mvm.AutoRebind, options);

        writer.WritePropertyName("DefaultCard");
        JsonSerializer.Serialize(writer, mvm.DefaultCard, options);

        writer.WritePropertyName("ServerIP");
        JsonSerializer.Serialize(writer, mvm.ServerIP, options);

        writer.WritePropertyName("ServerPort");
        JsonSerializer.Serialize(writer, mvm.ServerPort, options);

        writer.WriteEndObject();
    }
}