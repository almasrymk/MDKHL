using System;

namespace SmartParking.Payments;

public class MobilePaymentSessionDto
{
    public Guid PaymentId { get; set; }
    public string SessionId { get; set; }
    public string SessionUrl { get; set; }
    public string MerchantReferenceId { get; set; }
    public string ExpiresAt { get; set; }
    public object GatewayResponse { get; set; }
}
