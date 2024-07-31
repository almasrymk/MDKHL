namespace SmartParking.Entities
{
    public class City: BaseEntity
    {
        public int GovernorateId { get; set; }
        public Governorate Governorate { get; set; }
    }
}
