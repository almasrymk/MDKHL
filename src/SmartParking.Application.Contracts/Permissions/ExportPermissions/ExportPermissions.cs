using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.ExportPermissions;

public static class ExportPermissions
{
    public const string PropertyName = ".Export";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string CSV = Default + SmartParkingPermissions.ExportCSV;
    public const string Excel = Default + SmartParkingPermissions.ExportExcel;
    public const string PDF = Default + SmartParkingPermissions.ExportPDF;
}
