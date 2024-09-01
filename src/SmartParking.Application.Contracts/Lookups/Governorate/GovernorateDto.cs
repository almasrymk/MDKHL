namespace SmartParking.Lookups
{
    public class GovernorateDto : BaseDto
    {
        public int CountryId { get; set; }
        public string CountryArabicName { get; set; }
        public string CountryEnglishName { get; set; }
        public string CountryNotes { get; set; }
        public string EnglishCapital { get; set; }
        public string ArabicCapital { get; set; }
    }
}
