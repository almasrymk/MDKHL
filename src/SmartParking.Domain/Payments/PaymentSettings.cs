namespace SmartParking.Payments;

public class PaymentSettings
{
    public PaymentModeSettings TestMode { get; set; } = new();
    public PaymentModeSettings LiveMode { get; set; } = new();
}
