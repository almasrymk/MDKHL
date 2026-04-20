using SmartParking.Enums;

namespace SmartParking.Utilities.StaticValues;

public static class RenterPackageStaticValues
{
    public const string ArabicName = "باقه مستاجر";
    public const string EnglishName = "Renter Package";
    public const decimal Price = 0;
    public const string Description = "This package is assigned to the Renter of the unit and provides them with full access to the parking services.";
    public const bool Published = true;
    public static readonly TimeOnly PublishedFrom = new TimeOnly(00, 00);
    public static readonly TimeOnly PublishedTo = new TimeOnly(00, 00);
    public static readonly DateTime ActivationDate = DateTime.Now;
    public static readonly DateTime DeactivationDate = DateTime.Now.AddYears(100);
    public const SubscriptionStatus PackageStatus = SubscriptionStatus.Active;
    public const int DurationOfPackage = 36500; // 100 years in days
    public const string CustomerCategory = "Renter";
    public const PackageCategoryTypes PackageCategoryType = PackageCategoryTypes.CustomPackage;
    public const Status status = Status.Active;
}
