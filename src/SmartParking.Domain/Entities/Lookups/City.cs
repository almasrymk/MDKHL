using SmartParking.Enums;

namespace SmartParking.Entities;

public class City: BaseEntity
{
    public int RegionId { get; set; }

    public Status Status { get; set; }
    public int GovernorateId { get; set; }
    public virtual Governorate Governorate { get; set; }

}
