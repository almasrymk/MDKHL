namespace SmartParking.Organizations;

public class OrganizationSettingsDto
{
    public bool UserAccess { get; set; }
    public bool VehcileAccess { get; set; }
    public bool UseShifts { get; set; }
    public int OwnerSlots { get; set; }
    public int RentalSlots { get; set; }
    public bool IsSlotsEditable { get; set; }

}
