using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Shifts;

public class ShiftDto : BaseDto
{
    public string Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int GarageId { get; set; }
    public string  GarageArabicName { get; set; }
    public string  GarageEnglishName { get; set; }
    public string  OrganizationEnglishName { get; set; }
    public string OrganizationArabicName { get; set; }
    public int OrganizationId { get; set; }
    public bool IsFullDay { get; set; }

    public Status Status { get; set; }
}
