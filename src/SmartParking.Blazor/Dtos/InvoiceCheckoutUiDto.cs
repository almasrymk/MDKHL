namespace SmartParking.Blazor.Dtos;

public class InvoiceCheckoutUiDto
{
    public int? InvoiceId { get; set; }
    public string InvoiceNumber { get; set; }
    public string PaymentNumber { get; set; }
    public decimal InvoicePrice { get; set; }
    public string CashierName { get; set; }
    public string BranchName { get; set; }
    public string OrganizationAddress { get; set; }
    public string PlateNumber { get; set; }
    public DateTime? EntranceTime { get; set; }
    public DateTime? InvoiceTime { get; set; }
    public bool IsPackage { get; set; }
    public string QrCodeInvoice { get; set; }
    public double Vat { get; set; }
    public decimal Discount { get; set; }
    public string OrganizationPicture {  get; set; }
}
