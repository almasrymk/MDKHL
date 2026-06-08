namespace SmartParking.Payments.PaymentIntegrations;

/// <summary>
/// Discriminator for the payment gateway DI factory.
/// Used as the key in the <c>Func&lt;GatewayKey, IPaymentGateway&gt;</c> factory delegate.
/// </summary>
public enum GatewayKey
{
    Oppwa,
    Geidea,
    Paymob
}
