namespace SmartParking.Entities
{
    public class City: BaseEntity
    {
        public int GovernorateId { get; set; }
        public virtual Governorate Governorate { get; set; }
    }
}
