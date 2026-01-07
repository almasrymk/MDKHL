using System;

namespace SmartParking.Payments.PaymentIntegrations;

public class HyperPayResultDto
{
    public string Id { get; set; } = default!;
    public string PaymentType { get; set; } = default!;
    public string Amount { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public HyperPayResultDetails Result { get; set; } = default!;
    public string PaymentBrand { get; set; } = default!;
}
public class HyperPayResultDetails
{
    public string Code { get; set; } = default!;
    public string Description { get; set; } = default!;
}