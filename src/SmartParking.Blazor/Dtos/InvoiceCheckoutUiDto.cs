namespace SmartParking.Blazor.Dtos;

public class InvoiceCheckoutUiDto
{
    public string InvoiceNumber { get; set; }
    public string TransactionNumber { get; set; }
    public decimal InvoicePrice { get; set; }
    public string CashierName { get; set; }
    public string BranchName { get; set; }
    public string OrganizationAddress { get; set; }
    public string PlateNumber { get; set; }
    public DateTime? EntranceTime { get; set; }
    public DateTime? InvoiceTime { get; set; }
    public bool IsPackage { get; set; }
    public string QrCodeInvoice { get; set; }
    public decimal Vat { get; set; }
    public decimal Discount { get; set; }

}
