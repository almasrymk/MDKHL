namespace SmartParking.IntegrationUnit.Dtos;

public class CreateUpdateBookingV2Dto : CreateUpdateBookingDto
{
    public string ProviderName { get; set; } = string.Empty;
    public string? ProviderArabicName { get; set; }
}
