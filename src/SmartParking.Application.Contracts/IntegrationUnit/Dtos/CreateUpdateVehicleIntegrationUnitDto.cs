using SmartParking.Vehicles;
using SmartParking.Validations;
using System;
using System.Text.RegularExpressions;

namespace SmartParking.IntegrationUnit.Dtos;

public class CreateUpdateVehicleIntegrationUnitDto
{
    public string PlateNumber { get; set; }
    public string licenseType { get; set; }
    //public string ArPlateLetters { get; set; } = "";
    //public string EnPlateLetters { get; set; } = "";
    //public string ArPlateNumber { get; set; } = "";
    //public string EnPlateNumber { get; set; } = "";
    //public string ModelYear { get; set; }
    //public string ModelName { get; set; }
    //public string BrandName { get; set; }
    //public string PlateColorName { get; set; }
    //public string LicenceTypeName { get; set; }
    //public Guid CustomerId { get; private set; }
    //public void SetCustomerId(Guid customerId)
    //{
    //    CustomerId = customerId;
    //}

    public VehiclePlateDto SeparateAndTranslatePlateNumbers()
    {
        if (string.IsNullOrEmpty(PlateNumber))
        {
            return null;
        }

        var numbersOnly = Regex.Replace(PlateNumber, @"[^\d٠-٩]", "");
        var lettersOnly = Regex.Replace(PlateNumber, @"[\d٠-٩]", "").Trim();

        if (string.IsNullOrEmpty(numbersOnly))
        {
            return null;
        }

        try
        {
            var (englishNumbers, arabicNumbers) = VehicleValidation.HandleKeyUp(numbersOnly);
            var (englishLetters, arabicLetters) = VehicleValidation.HandleKeyUpLetters(lettersOnly);
            VehiclePlateDto dto = new VehiclePlateDto
            {
                ArPlateLetters = arabicLetters,
                ArPlateNumber = arabicNumbers,
                EnPlateLetters = englishLetters,
                EnPlateNumber = englishNumbers
            };
            return dto;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public CreateUpdateVehicleDto MapToVehicleDto(
    CreateUpdateVehicleIntegrationUnitDto input,
    VehicleOptionsDto vehicleOptions,
    Guid? creatorId = null)
    {
        var dto = SeparateAndTranslatePlateNumbers();
        return new CreateUpdateVehicleDto
        {
            VehicleModelId = vehicleOptions.ModelId,
            VehicleBrandId = vehicleOptions.BrandId,
            PlateColorId = vehicleOptions.PlateColorId,
            LicenceTypeId = vehicleOptions.LicenceTypeId,

            ArPlateLetters = dto.ArPlateLetters,
            EnPlateLetters = dto.EnPlateLetters,
            ArPlateNumber = dto.ArPlateNumber,
            EnPlateNumber = dto.EnPlateNumber,

            ModelYear = vehicleOptions.ModelYear,
            CustomerId = vehicleOptions.CustomerId,

            BelongToCompany = false,
            CompanyName = null,

            CreatorId = creatorId,
            LastModifierId = null,
            LastModificationTime = null
        };
    }

}
