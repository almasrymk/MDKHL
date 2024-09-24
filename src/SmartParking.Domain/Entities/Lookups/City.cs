using SmartParking.Enums;

namespace SmartParking.Entities;

public class City: BaseEntity
{
    public int EmaraaId { get; set; }

    public Status Status { get; set; }
    public int GovernorateId { get; set; }

}
