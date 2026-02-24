using System;

namespace SmartParking.UserShifts;

public class ShiftZoneUserViewDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; }
    public string Role { get; set; } 
    public string Device { get; set; }
    public string Garage { get; set; }
    public string OrganizationEnglishName { get; set; }
    public string OrganizationArabicName { get; set; }
}
