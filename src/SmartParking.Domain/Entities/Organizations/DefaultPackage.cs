using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Organizations;

public class DefaultPackage : BaseEntity
{
    public decimal FirstHourPrice { get; set; }
    public decimal SecoundHourPrice { get; set; }
    public decimal ThirdHourPrice { get; set; }
    public decimal AdditionalHourPrice { get; set; }
    public decimal Fullday { get; set; }
    public bool IsNumberOfHoursExceeded { get; set; }
    public int NumberOfHours { get; set; }
    public bool IsFullDayExceeded { get; set; }
    public TimeOnly ExceededTime { get; set; }
    public int OrgnizationId { get; set; }
    public Status Status { get; set; }
    public virtual Organization Organization { get; set; }

}
