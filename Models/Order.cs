using JsonUploader.CustomConverters;
using System.Text.Json.Serialization;

namespace JsonUploader.Models;

[JsonConverter(typeof(IOrderElementConverter))]
internal abstract class IOrderElement {
    public string Name { get; set; } = null!;
}

internal abstract class IOrderElementValue<TValue> : IOrderElement {
    public IEnumerable<TValue> Value { get; set; } = null!;
}

internal class OrderElementString : IOrderElementValue<string> {
}


internal class ValueProduct {
    public IEnumerable<string> Action { get; set; } = null!;
    public IEnumerable<string> ProductCode { get; set; } = null!;
}

internal class OrderElementProduct : IOrderElementValue<ValueProduct> {
}

internal class ValuePromo {
    public IEnumerable<string> Action { get; set; } = null!;
    public IEnumerable<string> PromoCode { get; set; } = null!;
    public IEnumerable<string>? PromoDate { get; set; }
}

internal class OrderElementPromo : IOrderElementValue<ValuePromo> {
}

internal class OrderItemProductProductSpecification {
    public string Id { get; set; } = null!;
}

internal class OrderItemProduct {
    public string? Id { get; set; } = null!;
    public OrderItemProductProductSpecification? ProductSpecification { get; set; }
    public IEnumerable<IOrderElement?> Characteristic { get; set; } = [];
}

internal class OrderItem {
    public string Id { get; set; } = null!;
    public string Action { get; set; } = null!;
    public OrderItemProduct Product { get; set; } = null!;
}

internal class Order {
    public string ExternalId { get; set; } = null!;
    public string RequestedCompletionDate { get; set; } = null!;
    public IEnumerable<OrderElementString> Characteristic { get; set; } = [];
    public IEnumerable<OrderItem> OrderItem { get; set; } = [];
}
