using SmartParking.Enums;

namespace SmartParking.Utilities.StaticValues;

public class ProNightStaticValues
{
    public const string ArabicName = "باقه برونايت";
    public const string EnglishName = "Pronight Package";
    public const decimal Price = 0;
    public const string Description = "This package is for renters";
    public const bool Published = true;
    public static readonly TimeOnly PublishedFrom = new TimeOnly(00, 00);
    public static readonly TimeOnly PublishedTo = new TimeOnly(00, 00);
    public static readonly DateTime ActivationDate = DateTime.Now;
    public static readonly DateTime DeactivationDate = DateTime.Now.AddYears(100);
    public const SubscriptionStatus PackageStatus = SubscriptionStatus.Active;
    public const int DurationOfPackage = 36500; // 100 years in days
    public const CustomerCategoryStatus CustomerCategory = CustomerCategoryStatus.None;
    public const PackageCategoryTypes PackageCategoryType = PackageCategoryTypes.CustomPackage;
    public const Status status = Status.Active;
}
