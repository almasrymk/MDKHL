using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations;

public static class PackageAssignationLogic
{
    public static DateTime PackageActivationTime(DateTime ActivationDateTime , TimeOnly PackageStartTime  , bool IsCustom)
    {
        if (IsCustom)
        {
            return ActivationDateTime + PackageStartTime.ToTimeSpan();                                             
        }
        return DateTime.Now;
    }
    public static DateTime PackageExpirationTime(DateTime ExpirationDateTime , int DurationOfPackage, bool IsCustom)
    {
        if (IsCustom)
        {
            return ExpirationDateTime ;                                             
        }
        return DateTime.Now.AddDays(DurationOfPackage).AddMinutes(60 - DateTime.Now.AddDays(DurationOfPackage).Minute) ;
    }
}
