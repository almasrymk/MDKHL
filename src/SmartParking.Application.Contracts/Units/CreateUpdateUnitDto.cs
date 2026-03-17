namespace SmartParking.Units;

public class CreateUpdateUnitDto : CreateUpdateBaseDto
{
    public string Code { get; set; }
    public int OwnerSlots { get; set; }
    public int RenterSlots { get; set; }
    public string Description { get; set; }
    public int OrganizationId { get; set; }
}
