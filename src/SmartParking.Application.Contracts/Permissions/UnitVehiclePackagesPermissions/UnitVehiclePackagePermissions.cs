namespace SmartParking.Permissions.UnitVehiclePackagesPermissions;

public class UnitVehiclePackagePermissions
{
    public const string PropertyName = ".UnitVehiclePackages";
    public const string GroupName = SmartParkingPermissions.GroupName;
    public const string Default = GroupName + PropertyName;
    public const string View = Default + SmartParkingPermissions.View;
}
