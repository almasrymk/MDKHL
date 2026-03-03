using System;

namespace SmartParking.Authentication;

public class AssignStaffDataDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string EnglishName { get; set; }
    public string PhoneNumber { get; set; }
}
