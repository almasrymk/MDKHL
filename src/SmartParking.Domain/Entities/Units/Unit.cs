namespace SmartParking.Entities.Units;

public class Unit : BaseEntity
{
    public string Code { get; set; }
    public int OwnerSlots { get; set; }
    public int RenterSlots { get; set; }
    public string Description { get; set; }
    public int OrganizationId { get; set; }
}
