using System.Text.Json.Serialization;

namespace SmartParking.Payments.PaymentIntegrations;

public class GeideaCallbackPayload
{
    [JsonPropertyName("order")]
    public GeideaOrder Order { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; }
}
