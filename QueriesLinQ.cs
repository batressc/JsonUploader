using JsonUploader.Models;
using System.Globalization;

namespace JsonUploader;

internal class QueriesLinQ {
    private readonly List<JsonInformation> _data;

    public QueriesLinQ(List<JsonInformation> data) {
        _data = data;
    }

    // 1. Obtener todas las órdenes filtrando por rango de fechas
    public List<JsonInformation> OrdersByRange(DateTime startDate, DateTime endDate) {
        List<JsonInformation> result = _data.Where(
            x => !string.IsNullOrWhiteSpace(x.Order?.RequestedCompletionDate) &&
            DateTime.TryParseExact(x.Order.RequestedCompletionDate, "yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate) &&
            parsedDate.Date >= startDate.Date && parsedDate.Date <= endDate.Date
        ).ToList();
        return result;
    }

    // 2. Obtener todos los componentes removidos por orden
    public List<ComponentRemovedDto> ComponentsRemoved() {
        List<ComponentRemovedDto> result = _data.Where(json => json.Order is not null)
            .Select(x => new ComponentRemovedDto(
                x.FileName,
                x.Content,
                x.Order!.OrderItem
                    .Where(ordItem => !ordItem.Action.Equals("nochange", StringComparison.CurrentCultureIgnoreCase))
                    .SelectMany(prod => prod.Product.Characteristic.Where(charac => charac is OrderElementProduct))
                    .Cast<OrderElementProduct>()
                    .SelectMany(prodItem => prodItem.Value.Where(val => val.Action.First().Equals("delete", StringComparison.CurrentCultureIgnoreCase)))
                    .SelectMany(valProd => valProd.ProductCode)
                    .ToList()
            ))
            .Where(x => x.ProductsRemoved.Any())
            .ToList();
        return result;
    }

    // 6. Obtener todas las órdenes que remuevan dry loop
    public List<JsonInformation> DryLoopRemoved() {
        List<JsonInformation> result = _data.Where(
            json =>
                json.Order is not null &&
                json.Order.OrderItem.Any(
                    ordItem =>
                        ordItem.Action.Equals("delete", StringComparison.CurrentCultureIgnoreCase) &&
                        ordItem.Product.Characteristic.Any(
                            charac =>
                                charac is OrderElementProduct prodItem &&
                                prodItem.Value.Any(
                                    val => val.Action.First().Equals("delete", StringComparison.CurrentCultureIgnoreCase) &&
                                    val.ProductCode.First().Equals("132804")
                                )
                        )
                )
        ).ToList();
        return result;
    }

    // 7. Obtener todas las órdenes que tengan solo un additional STB
    public List<JsonInformation> OneAdditionalSetupBox() {
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

    // 8. Obtener todas las órdenes que tengan no tengan additional STB pero tengan STB principal
    public List<JsonInformation> OnlyMainSetupBox() {
        List<JsonInformation> result = _data.Where(json => json.Order is not null)
            .Select(data => new {
                Data = data,
                Products = data.Order!.OrderItem
                    .Where(ordItem => !ordItem.Action.Equals("nochange", StringComparison.CurrentCultureIgnoreCase))
                    .SelectMany(prod => prod.Product.Characteristic.Where(charac => charac is OrderElementProduct))
                    .Cast<OrderElementProduct>()
                    .SelectMany(prodItem => prodItem.Value.Where(val => !val.Action.First().Equals("delete", StringComparison.CurrentCultureIgnoreCase)))
                    .SelectMany(valProd => valProd.ProductCode)
                    .Where(prodCode => prodCode == "920086" || prodCode == "920022")
                    .ToList()
            })
            .Where(filter => filter.Products.Count == 1 && filter.Products.Any(x => x == "920086"))
            .Select(x => x.Data)
            .ToList();
        return result;
    }

    // 9. Obtener las órdenes que no tengan promociones
    public List<JsonInformation> WithoutPromotions() {
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
