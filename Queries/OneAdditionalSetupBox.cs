using JsonUploader.Models;
using System.Text.Json;
using static System.Text.Json.JsonElement;

namespace JsonUploader.Queries;

internal class OneAdditionalSetupBox {
    private readonly List<JsonInformation> _data;

    public OneAdditionalSetupBox(List<JsonInformation> data) {
        _data = data;
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
        int additionalSetupBoxes = 0;
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
                if (productCode is null || productAction is null || productAction != "add" || productCode != "920022") {
                    continue;
                }
                additionalSetupBoxes++;
            }
        }
        return additionalSetupBoxes == 1;
    }

    public List<JsonInformation> ExecuteQuery() {
        List<JsonInformation> resultado = _data
            .Where(Filter)
            .ToList();
        return resultado;
    }

    public List<JsonInformation> ExecuteLinQ() {
        List<JsonInformation> result = _data.Where(
            json =>
                json.Order is not null &&
                json.Order.OrderItem
                    .Where(ordItem => !ordItem.Action.Equals("nochange", StringComparison.CurrentCultureIgnoreCase))
                    .SelectMany(prod => prod.Product.Characteristic.Where(charac => charac is OrderElementProduct))
                    .Cast<OrderElementProduct>()
                    .SelectMany(prodItem => prodItem.Value.Where(val => val.Action.First().Equals("add", StringComparison.CurrentCultureIgnoreCase)))
                    .SelectMany(valProd => valProd.ProductCode)
                    .Where(prodCode => prodCode == "920022")
                    .Count() == 1
        ).ToList();
        return result;
    }
}
