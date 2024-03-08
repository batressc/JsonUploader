using JsonUploader.Models;
using System.Text.Json;
using static System.Text.Json.JsonElement;

namespace JsonUploader.Queries;

internal class OnlyMainSetupBox {
    private readonly List<JsonInformation> _data;

    public OnlyMainSetupBox(List<JsonInformation> data) {
        _data = data;
    }

    private bool Filter(JsonInformation record) {
        string[] validProducts = ["920086", "920022"];
        JsonElement jsonRecord = record.JsonObject.RootElement;
        if (jsonRecord.ValueKind != JsonValueKind.Object) {
            return false;
        }
        JsonElement orderItems = jsonRecord.GetProperty("orderItem");
        if (orderItems.ValueKind != JsonValueKind.Array) {
            return false;
        }
        bool onlyMainSetupBox = false;
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
                if (productCode is null || productAction is null || (productAction == "delete" && validProducts.Contains(productCode))) {
                    continue;
                }
                switch (productCode) {
                    case "920086":
                        onlyMainSetupBox = true;
                        break;
                    case "920022":
                        return false;
                }
            }
        }
        return onlyMainSetupBox;
    }

    public List<JsonInformation> ExecuteQuery() {
        List<JsonInformation> resultado = _data
            .Where(Filter)
            .ToList();
        return resultado;
    }
}
