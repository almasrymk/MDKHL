using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.CheckoutPermissions;

public static class CheckoutPermissions
{
    public const string PropertyName = ".Checkout";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string Confirm = Default + SmartParkingPermissions.ConfirmPaying;
    public const string OpenGate = Default + SmartParkingPermissions.OpenGate;
    public const string Discount = Default + SmartParkingPermissions.Discount;
}
