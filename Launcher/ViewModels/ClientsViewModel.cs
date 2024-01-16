using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Launcher.ViewModels;

[JsonConverter(typeof(ClientsViewModelConverter))]
public partial class ClientsViewModel : ViewModelBase
{
    public ClientsViewModel()
    {
        Clients = new ObservableCollection<ClientViewModel>();
    }

    public ObservableCollection<ClientViewModel> Clients { get; set; }

    int selectedClientIndex_;
    public int SelectedClientIndex
    {
        get => selectedClientIndex_;
        set => this.RaiseAndSetIfChanged(ref selectedClientIndex_, value);
    }
}

class ClientsViewModelConverter : JsonConverter<ClientsViewModel>
{
    public override ClientsViewModel Read(
         ref Utf8JsonReader reader,
         Type typeToConvert,
         JsonSerializerOptions options)
    {
        ClientsViewModel result = new();

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        reader.Read();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            result.Clients.Add(JsonSerializer.Deserialize<ClientViewModel>(ref reader, options)!);
            reader.Read();
        }
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        ClientsViewModel cvm,
        JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var preset in cvm.Clients)
        {
            JsonSerializer.Serialize(writer, preset, options);
        }

        writer.WriteEndArray();
    }
}