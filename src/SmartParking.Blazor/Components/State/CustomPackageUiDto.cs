namespace SmartParking.Blazor.Components.State;

public class CustomPackageUiDto
{
    public DateTime? ActivationDate { get; set; }
    public DateTime? DectivationDate { get; set; }
    public TimeSpan? startTime { get; set; }
    public TimeSpan? endTime { get; set; }
    public int? DurationOfPackage { get; set; }
    public decimal? PriceOfPackage { get; set; }
    public decimal? PriceOfPackagePerDay { get; set; }
    public decimal? CountOfDays { get; set; }
}
