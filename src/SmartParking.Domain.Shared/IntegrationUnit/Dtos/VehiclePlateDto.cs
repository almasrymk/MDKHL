namespace SmartParking.IntegrationUnit.Dtos;

public class VehiclePlateDto
{
    public string EnPlateNumber { get; set; } = string.Empty;
    public string ArPlateNumber { get; set; } = string.Empty;
    public string EnPlateLetters { get; set; } = string.Empty;
    public string ArPlateLetters { get; set; } = string.Empty;

    public static VehiclePlateDto Parse(string plate)
    {
        return Validations.VehicleValidation.Parse(plate);
    }
}
