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
    public KeeperShiftRequestType KeeperShiftRequestType { get; set; }
}
