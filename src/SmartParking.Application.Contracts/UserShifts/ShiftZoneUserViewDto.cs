using System;
using System.Collections.Generic;

namespace SmartParking.UserShifts;

public class ShiftZoneUserViewDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }

    // Single device for Staff; kept for backward-compat
    public string Device { get; set; }

    // All devices under the zone — populated only when Role == "Manager"
    public List<string> Devices { get; set; } = new();

    public string Garage { get; set; }
    public string OrganizationEnglishName { get; set; }
    public string OrganizationArabicName { get; set; }
}