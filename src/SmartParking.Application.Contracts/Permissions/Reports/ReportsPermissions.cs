using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.Reports;

public class ReportsPermissions
{
    public const string PropertyName = ".Reports";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string IT = Default + ".IT";
    public const string SecurityManager = Default + ".SecurityManager";
    public const string FinancialManagement = Default + ".FinancialManagement";
}
