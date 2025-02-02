using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations;

public static class PackageValidations
{
    public static bool CheckPackageTimeValiditity(TimeOnly? From, TimeOnly? To) {
        
        if(From.Value==To.Value) return true;
        return false;
    
    }
    public static bool CheckOrganiztionPackageTimeValiditity(TimeOnly? From, TimeOnly? To)
    {

        if (From.Value >= To.Value )
            return false;
        return true;

    }
    public static bool GetOrganizationIsCloseAfterTimeValidity(bool isClose, TimeOnly? from, TimeOnly? to)
    {
        var IsClose = (isClose, from, to) switch
        {
            (_, null, null) or (_, null, not null) or (_, not null, null) => false,
            (_, not null, not null) => CheckOrganiztionPackageTimeValiditity(from,to)
        };
        return IsClose;
    }

}
