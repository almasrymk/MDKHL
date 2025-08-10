namespace SmartParking.ZATCATaxInvoice
{
    public class AddressDto
    {
        public ushort BuildingNumber { get; set; }
        public int PostalCode { get; set; }
        public string NameArabic { get; set; }
        public string CountryName { get; set; } = "";
        public string RegionName { get; set; } = "";
    }
}