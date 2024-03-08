using JsonUploader.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonUploader.CustomConverters;

internal class IOrderElementConverter : JsonConverter<IOrderElement?> {
    public override IOrderElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token");
        }

        using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
        JsonElement rootElement = jsonDocument.RootElement;
        if (rootElement.TryGetProperty("name", out JsonElement nameProperty) && rootElement.TryGetProperty("value", out JsonElement valueProperty)) {
            string? nameValue = nameProperty.GetString();
            if (string.IsNullOrWhiteSpace(nameValue)) {
                return null;
            }
            if (int.TryParse(nameValue, out _)) {
                return JsonSerializer.Deserialize<OrderElementProduct>(rootElement.GetRawText(), options);
            }
            if (nameValue!.ToLower().Contains("promo")) {
                return JsonSerializer.Deserialize<OrderElementPromo>(rootElement.GetRawText(), options);
            }
            if (valueProperty.ValueKind == JsonValueKind.Array) {
                JsonElement valueContent = valueProperty[0];
                if (valueContent.ValueKind == JsonValueKind.String) { 
                    return JsonSerializer.Deserialize<OrderElementString>(rootElement.GetRawText(), options);
                }
            }

        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer, IOrderElement? value, JsonSerializerOptions options) {
        if (value == null) {
            writer.WriteNullValue();
        } else {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}

