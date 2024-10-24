namespace SmartParking.Lookups
{
    public class CityDto : BaseDto
    {
        public int GovernorateId { get; set; }
        public int RegionId { get; set; }
		public string GovernorateArabicName { get; set; }
        public string GovernorateEnglishName { get; set; }
        public string GovernorateNotes { get; set; }
    }
}
