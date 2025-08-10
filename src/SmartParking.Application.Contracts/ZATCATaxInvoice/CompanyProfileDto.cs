namespace SmartParking.ZATCATaxInvoice
{
    public class CompanyProfileDto
    {
        public string EnglishName { get; set; }
        public string TaxVat { get; set; }
        public string CRN { get; set; }
        public string BuildingNumber { get; set; }
        public string UserNameZATCA { get; set; }
        public string PasswordZATCA { get; set; }
        public string Address { get; set; }
        public string Governorate { get; set; } = "";
        public int PostalCode { get; set; } = 0;
        public string Country { get; set; } = "";
        public string Region { get; set; } = "";
    }
}