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
}
