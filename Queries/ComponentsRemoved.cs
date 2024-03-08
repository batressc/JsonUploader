using JsonUploader.Models;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Text.Json.JsonElement;

namespace JsonUploader.Queries;

internal class ComponentsRemoved {
    private readonly List<JsonInformation> _data;

    public ComponentsRemoved(List<JsonInformation> data) {
        _data = data;
    }

    private List<string> GetProductsRemoved(JsonElement orderItems) {
        List<string> productsRemoved = [];
        foreach (JsonElement item in orderItems.EnumerateArray()) {
            JsonElement action = item.GetProperty("action");
            if (action.ValueKind != JsonValueKind.String || action.GetString() == "noChange") {
                continue;
            }
            JsonElement product = item.GetProperty("product");
            if (product.ValueKind != JsonValueKind.Object) {
                continue;
            }
            JsonElement characteristics = product.GetProperty("characteristic");
            if (characteristics.ValueKind != JsonValueKind.Array) {
                continue;
            }
            foreach (JsonElement charItem in characteristics.EnumerateArray()) {
                JsonElement value = charItem.GetProperty("value")[0];
                if (value.ValueKind != JsonValueKind.Object) {
                    continue;
                }
                ObjectEnumerator valueProperties = value.EnumerateObject();
                if (!valueProperties.Any(x => x.Name == "productCode") || !valueProperties.Any(x => x.Name == "action")) {
                    continue;
                }
                string? productCode = value.GetProperty("productCode")[0].GetString();
                string? productAction = value.GetProperty("action")[0].GetString();
                if (productCode is null || productAction is null || productAction != "delete") {
                    continue;
                }
                productsRemoved.Add(productCode);
            }
        }
        return productsRemoved;
    }

    private bool Filter(JsonInformation record) {
        JsonElement jsonRecord = record.JsonObject.RootElement;
        if (jsonRecord.ValueKind != JsonValueKind.Object) {
            return false;
        }
        JsonElement orderItems = jsonRecord.GetProperty("orderItem");
        if (orderItems.ValueKind != JsonValueKind.Array) {
            return false;
        }
        var productsRemoved = GetProductsRemoved(orderItems);
        return productsRemoved.Any();
    }

    private ComponentRemovedDto? Materialization(JsonInformation record) {
        JsonElement jsonRecord = record.JsonObject.RootElement;
        if (jsonRecord.ValueKind != JsonValueKind.Object) {
            return null;
        }
        JsonElement orderItems = jsonRecord.GetProperty("orderItem");
        if (orderItems.ValueKind != JsonValueKind.Array) {
            return null;
        }
        var productsRemoved = GetProductsRemoved(orderItems);
        if (!productsRemoved.Any()) {
            return null;
        }
        return new ComponentRemovedDto(record.FileName, record.Content, productsRemoved);
    }

    public List<ComponentRemovedDto?> ExecuteQuery() {
        List<ComponentRemovedDto?> resultado = _data
            .Where(Filter)
            .Select(Materialization)
            .ToList();
        return resultado;
    }
}
