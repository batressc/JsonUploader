using JsonUploader.Models;
using System.Text.Json;
using static System.Text.Json.JsonElement;

namespace JsonUploader.Queries;

internal class WithoutPromotions {
    private readonly List<JsonInformation> _data;

    public WithoutPromotions(List<JsonInformation> data) {
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
                if (!valueProperties.Any(x => x.Name == "promoCode")) {
                    continue;
                }
                string? promoCode = value.GetProperty("promoCode")[0].GetString();
                if (promoCode is not null && !string.IsNullOrWhiteSpace(promoCode)) {
                    return false;
                }
            }
        }
        return true;
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
                !json.Order.OrderItem.Any(
                    ordItem =>
                        !ordItem.Action.Equals("nochange", StringComparison.CurrentCultureIgnoreCase) &&
                        ordItem.Product.Characteristic.Any(
                            charac =>
                                charac is OrderElementPromo promoItem &&
                                promoItem.Value.Any(val => !string.IsNullOrWhiteSpace(val.PromoCode?.First()))
                        )
                )
        ).ToList();
        return result;
    }
}
