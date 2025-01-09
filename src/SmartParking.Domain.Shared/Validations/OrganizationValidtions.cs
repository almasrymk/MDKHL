using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations;

public static class OrganizationValidtions
{
    public static int? FreeMinutesAndFirstHourRelation(decimal FirstHour)
    {
        return FirstHour>0?null:0;

    }
}
