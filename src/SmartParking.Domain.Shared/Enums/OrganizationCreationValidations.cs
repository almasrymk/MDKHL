using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Enums;

public enum OrganizationCreationValidations
{

    [Description("Commercial Registration Number is used of another Organization")]
    CommercialRegistrationNumber,

    [Description("Orgaization has been deleted")]
    OrganizationDeleteion,
}
