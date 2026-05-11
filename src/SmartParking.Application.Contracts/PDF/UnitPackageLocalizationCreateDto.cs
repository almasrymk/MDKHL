namespace SmartParking.PDF;

public class UnitPackageLocalizationCreateDto : BaseLocalizationCreateDto
{
    public string SubscriptionTimeKey { get; set; } = "SubscriptionTime";
    public string ExpirationTimeKey { get; set; } = "ExpirationTime";

    public string DurationKey { get; set; } = "Duration";
    public string DaysKey { get; set; } = "Days";
    public string UnitCodeKey { get; set; } = "UnitCode";
}
