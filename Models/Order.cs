namespace JsonUploader.Models;

internal abstract class IOrderElement<TValue> {
    public string Name { get; set; } = null!;
    public IEnumerable<TValue> Value { get; set; } = null!;
}

internal class OrderElementString : IOrderElement<string> {
}


internal class ValueProduct {
    public IEnumerable<string> Action { get; set; } = null!;
    public IEnumerable<string> ProductCode { get; set; } = null!;
}

internal class OrderElementProduct : IOrderElement<ValueProduct> {
}

internal class ValuePromo {
    public IEnumerable<string> Action { get; set; } = null!;
    public IEnumerable<string> PromoCode { get; set; } = null!;
    public IEnumerable<string>? PromoDate { get; set; }
}

internal class OrderElementPromo : IOrderElement<ValuePromo> {
}



internal class Order {
    public string ExternalId { get; set; } = null!;
    public string RequestedCompletionDate { get; set; } = null!;
    public IEnumerable<OrderElementString> Characteristic { get; set; } = []; 
}
