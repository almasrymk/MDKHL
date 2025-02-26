using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Analysis;

public class PaymentUserShift
{
    public string UserName { get; set; }
    public string ShiftArabicName { get; set; }
    public string ShiftEnglishName { get; set; }
    public decimal TotalAmount { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string OrganizationArabicName { get; set; }
    public string OrganizationEnglishName { get; set; }
    public string GarageArabicName { get; set; }
    public string GarageEnglishName { get; set; }
}
