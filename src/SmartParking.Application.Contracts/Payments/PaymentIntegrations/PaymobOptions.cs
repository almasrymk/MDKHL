namespace SmartParking.Payments.PaymentIntegrations;

public class PaymobOptions
{
    public const string SectionName = "Providers:Paymob";

    public string Mode { get; set; } = "Test";

    public PaymobEnvironmentOptions Test { get; set; } = new();
    public PaymobEnvironmentOptions Live { get; set; } = new();
}

public class PaymobEnvironmentOptions
{
    public string BaseUrl { get; set; } = "https://accept.paymob.com/api";
    public string ApiKey { get; set; } = string.Empty;
    public string IntegrationId { get; set; } = string.Empty;
    public string IframeId { get; set; } = string.Empty;
    public string Currency { get; set; } = "SAR";
}
