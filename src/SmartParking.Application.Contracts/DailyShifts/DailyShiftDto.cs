using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.DailyShifts;
public class DailyShiftDto : BaseDto
{
    public Guid KeeperId { get; set; }
    public string KeeperName {  get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationNameAr { get; set; }
    public string OrganizationNameEn { get; set; }
    public int GarageId { get; set; }
    public string GarageNameAr {  get; set; }
    public string GarageNameEn { get; set; }
    public DateTime RequestTime { get; set; }
    public int ShiftId { get; set; }
    public string ShiftNameAr { get; set; }
    public string ShiftNameEn { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public KeeperShiftRequestType KeeperShiftRequestType { get; set; }

    public decimal AmountOfCash {  get; set; }
    public decimal AmountOfVisa { get; set; }
}
