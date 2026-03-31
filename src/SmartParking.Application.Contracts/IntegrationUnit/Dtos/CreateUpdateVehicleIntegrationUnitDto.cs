using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.IntegrationUnit.Dtos;

public class CreateUpdateVehicleIntegrationUnitDto
{
    public string ArPlateLetters { get; set; } = "";
    public string EnPlateLetters { get; set; } = "";
    public string ArPlateNumber { get; set; } = "";
    public string EnPlateNumber { get; set; } = "";
    public string ModelYear { get; set; }
    public string ModelName { get; set; }
    public string BrandName { get; set; }
    public string PlateColorName { get; set; }
    public string LicenceTypeName { get; set; }
    public Guid CustomerId { get; private set; }
    public void SetCustomerId(Guid customerId)
    {
        CustomerId = customerId;
    }
    public CreateUpdateVehicleDto MapToVehicleDto(
    CreateUpdateVehicleIntegrationUnitDto input,
    int modelId,
    int brandId,
    int plateColorId,
    int licenceTypeId,
    Guid? creatorId = null)
    {
        return new CreateUpdateVehicleDto
        {
            VehicleModelId = modelId,
            VehicleBrandId = brandId,
            PlateColorId = plateColorId,
            LicenceTypeId = licenceTypeId,

            ArPlateLetters = input.ArPlateLetters,
            EnPlateLetters = input.EnPlateLetters,
            ArPlateNumber = input.ArPlateNumber,
            EnPlateNumber = input.EnPlateNumber,

            ModelYear = input.ModelYear,
            CustomerId = input.CustomerId,

            BelongToCompany = false,
            CompanyName = null,

            CreatorId = creatorId,
            LastModifierId = null,
            LastModificationTime = null
        };
    }

}
