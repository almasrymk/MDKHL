namespace SmartParking.Payments;

public enum InvoicePaymentResult
{
    Started,
    AlreadyPaid,
    PaymentInProgress,
    Completed,
    Failed
}
