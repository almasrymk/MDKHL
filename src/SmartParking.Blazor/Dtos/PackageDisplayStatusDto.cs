using SmartParking.Enums;

namespace SmartParking.Blazor.Dtos;
public class PackageDisplayStatusDto
{
    public CustomerPackageStatus CustomerPackageStatus { get; set; }

    public DateTime? ActivationDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// True = 24-hour package.
    /// </summary>
    public bool? IsPublished { get; set; }

    public TimeOnly? PublishFrom { get; set; }

    public TimeOnly? PublishTo { get; set; }
}
