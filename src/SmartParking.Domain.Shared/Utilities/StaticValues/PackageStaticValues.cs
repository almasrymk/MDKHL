using SmartParking.Enums;

namespace SmartParking.Utilities.StaticValues;

public static class PackageStaticValues
{
    public const string ArabicName = "باقه مالك";
    public const string EnglishName = "Owner Package";
    public const decimal Price = 0;
    public const string Description = "This package is assigned to the owner of the unit and provides them with full access to the parking services.";
    public const bool Published = true;
    public static readonly TimeOnly PublishedFrom = new TimeOnly(00, 00);
    public static readonly TimeOnly PublishedTo = new TimeOnly(00, 00);
    public static readonly DateTime ActivationDate = DateTime.Now;
    public static readonly DateTime DeactivationDate = DateTime.Now.AddYears(100);
    public const SubscriptionStatus PackageStatus = SubscriptionStatus.Active;
    public const int DurationOfPackage = 36500; // 100 years in days
    public const string CustomerCategory = "Owner";
    public const PackageCategoryTypes PackageCategoryType = PackageCategoryTypes.CustomPackage;
    public const Status status = Status.Active;
}

