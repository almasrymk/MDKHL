using System.Text.Json.Serialization;

namespace SmartParking.Payments.PaymentIntegrations;

public class GeideaOrder
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }

    [JsonPropertyName("merchantReferenceId")]
    public string MerchantReferenceId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("detailedStatus")]
    public string DetailedStatus { get; set; }

    [JsonPropertyName("updatedDate")]
    public string UpdatedDate { get; set; } // Sent as a timestamp string used for signature
}
