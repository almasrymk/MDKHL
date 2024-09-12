using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Enums;

public enum CustomerCategoryValidations
{
    [Description("Can't Delete Default Category for Organization")]
    DeleteDefaultForOrganization,


    [Description("Can't Change Default from True to False for Organization")]

    DefaultIsTrueAndUpdatedToBeFalse,

}
