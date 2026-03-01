namespace SmartParking.Organizations;

public class CreateUpdateOrganizationSettingsDto
{
    public int OrganizationId { get; set; }
    public bool UserAccess { get; set; }
    public bool VehcileAccess { get; set; }
    public bool UseShifts { get; set; }

}
