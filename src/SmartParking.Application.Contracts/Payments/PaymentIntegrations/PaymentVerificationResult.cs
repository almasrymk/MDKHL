using System;

namespace SmartParking.Payments.PaymentIntegrations;

public class PaymentVerificationResult
{
    public bool IsSuccess { get; }
    public Guid? PaymentId { get; }
    public string? ErrorMessage { get; }

    private PaymentVerificationResult(bool isSuccess, Guid? paymentId, string? errorMessage)
    {
        IsSuccess = isSuccess;
        PaymentId = paymentId;
        ErrorMessage = errorMessage;
    }

    public static PaymentVerificationResult Success(Guid paymentId)
        => new(true, paymentId, null);

    public static PaymentVerificationResult Failed(string? errorMessage = null)
        => new(false, null, errorMessage);
}