using Newtonsoft.Json;

namespace SmartParking.ZATCATaxInvoice
{
    public class InvoiceRequestDto
    {
        public InvoiceDto Invoice { get; set; }
        public CompanyProfileDto CompanyProfile { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AddressDto Address { get; set; }
    }
}