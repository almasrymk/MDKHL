namespace SmartParking.PDF;

public record UnitPackageLocalizationData(
    string CurrentPath,
    string QRCodePath,
    string UnitPackageNumber,
    string SimplifiedTaxInvoice,
    string HeaderVatReceiptPayment,
    string Vats,
    string Sar,
    string CashierNo,
    string OrganizationName,
    string OrganizationAddress,
    string UnitCode,
    string SubscriptionDate,
    string ExpirationDate,
    string Subscription,
    string Duration,
    string Days,
    string TotalPreVAT,
    string TotalIncludeVAT,
    string Discount,
    string After,
    string UnitPackageNumberView,
    decimal Price,
    decimal PaymentPrice,
    string PaymentNumber
);
