using System;

namespace SmartParking.Mobile.Keepers.Dtos;

public class UnlinkedTransactionViewDto
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string? PlateNumber { get; set; }
    public DateTime? SnapTime { get; set; }
    public DateTime? AccurateTime { get; set; }
    public string? VehiclePicture { get; set; }
    public bool IsIn { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationArabicName { get; set; } = string.Empty;
    public string OrganizationEnglishName { get; set; } = string.Empty;
    public int GarageId { get; set; }
    public string GarageArabicName { get; set; } = string.Empty;
    public string GarageEnglishName { get; set; } = string.Empty;
    public bool NoTransaction { get; set; } = false;
}
