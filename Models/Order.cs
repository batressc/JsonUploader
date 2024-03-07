using System.Text.Json.Serialization;

namespace JsonUploader.Models;


//[JsonConverter(typeof(OrderCharacteristicConverter))]
internal class OrderCharacteristic {
    [JsonPropertyName("orderVersion")]
    public string OrderVersion { get; set; } = null!;
    [JsonPropertyName("dueDate")]
    public DateTime? DueDate { get; set; }
}


internal class Order {
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = null!;
    [JsonPropertyName("requestedCompletionDate")]
    public DateTime RequestedCompletionDate { get; set; }
    [JsonPropertyName("characteristic")]
    public List<OrderCharacteristic> Characteristic { get; set; } = null!;
}
