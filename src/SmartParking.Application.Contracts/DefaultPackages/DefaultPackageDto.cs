using SmartParking.Enums;
using System;

namespace SmartParking.DefaultPackages;

public class DefaultPackageDto :BaseDto
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
    public Status Status { get; set; }
}
