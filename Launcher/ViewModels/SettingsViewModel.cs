using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Launcher.ViewModels;

[JsonConverter(typeof(SettingsViewModelConverter))]
public partial class SettingsViewModel : ViewModelBase
{
}

class SettingsViewModelConverter : JsonConverter<SettingsViewModel>
{
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

        reader.Read();

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

            string propertyName = reader.GetString()!.ToLowerInvariant();
            switch (propertyName)
            {
                default:
                    throw new JsonException($"Unknown property in SettingsViewModel: {propertyName}");
            }

        }
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        SettingsViewModel mvm,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteEndObject();
    }
}