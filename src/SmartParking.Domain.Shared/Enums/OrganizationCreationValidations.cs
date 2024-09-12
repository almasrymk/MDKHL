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

    [Description("Unified License Number is used of another Organization")]
    UnifiedLicenseNumUnifiedLicenseNum,

    [Description("Orgaization has been deleted")]
    OrganizationDeleteion,
}
