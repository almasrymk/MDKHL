namespace SmartParking.Lookups
{
    public class CreateUpdateGovernorateDto : CreateUpdateBaseDto
    {
        public int CountryId { get; set; }
        public string EnglishCapital { get; set; }
        public string ArabicCapital { get; set; }
    }
}
