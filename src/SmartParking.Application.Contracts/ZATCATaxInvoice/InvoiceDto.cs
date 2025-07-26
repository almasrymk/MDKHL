using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartParking.ZATCATaxInvoice
{
    public class InvoiceDto
    {

        [Required(ErrorMessage = "Invoice type (Tax = 1, Simplified = 2, TaxDebit = 3, SimplifiedDebit = 4, TaxCredit = 5, SimplifiedCredit = 6, TaxPrepayment = 7, SimplifiedPrepayment = 8) field required")]
        public int InvoiceTypeId { get; set; } = 2;
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public string DealerName { get; set; }
        public string DealerTaxNumber { get; set; }
        public decimal Total { get; set; }//Total invoice included details discount without vat
        //public decimal Vat { get; set; }
        public decimal VatPercentage { get; set; } // 15 or 0 or Will be set
        public decimal DiscountAmount { get; set; } // Discount for total invoice 
        public string Currency { get; set; } = "SAR";
        public decimal ExchangeRate { get; set; } = 1;
        public List<InvoiceDetailDto> InvoiceDetails { get; set; } = new List<InvoiceDetailDto>();
    }
}