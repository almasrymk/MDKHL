namespace SmartParking.ZATCATaxInvoice
{
    public class InvoiceDetailDto
    {
        public int Id { get; set; }
        public string UnitName { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal ItemPrice { get; set; } // Item price before item discount 
        public decimal Discount { get; set; }
        public decimal Net { get; set; } // Net with out vat value
        public decimal Vat { get; set; }
        public decimal VatPercentage { get; set; } //15
    }
}