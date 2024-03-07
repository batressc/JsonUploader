using JsonUploader.Models;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

internal class OrderCharacteristicConverter : JsonConverter<OrderCharacteristic> {
    public override OrderCharacteristic Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var orderCharacteristic = new OrderCharacteristic();

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                break;
            }

            if (reader.TokenType == JsonTokenType.PropertyName) {
                var propertyName = reader.GetString();
                reader.Read();
                switch (propertyName) {
                    case "orderVersion":
                        orderCharacteristic.OrderVersion = reader.GetString() ?? string.Empty;
                        break;
                    case "dueDate":
                        string? dueDateString = reader.GetString();
                        if (dueDateString is null) {
                            orderCharacteristic.DueDate = null;
                            break;
                        } 
                        if (DateTime.TryParseExact(dueDateString, "yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dueDate)) {
                            orderCharacteristic.DueDate = dueDate;
                        }
                        break;
                }
            }
        }

        return orderCharacteristic;
    }

    public override void Write(Utf8JsonWriter writer, OrderCharacteristic value, JsonSerializerOptions options) {
        writer.WriteStartArray("characteristic");

        writer.WriteStartObject();
        writer.WriteString("name", "orderVersion");
        writer.WriteStartArray("value");
        writer.WriteStringValue(value.OrderVersion);
        writer.WriteEndArray();
        writer.WriteEndObject();

        writer.WriteStartObject();
        writer.WriteString("name", "dueDate");
        writer.WriteStartArray("value");
        if (value.DueDate.HasValue) {
            writer.WriteStringValue(value.DueDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"));
        } else {
            writer.WriteNullValue();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();

        writer.WriteEndArray();
    }
}
