using FluentValidation;
using SmartParking.Lookups.VehicleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Vehicles.VehiclesLookups;

public class CreateUpdateVehicleModelDtoValidator : AbstractValidator<CreateUpdateVehicleModelDto>
{
    public CreateUpdateVehicleModelDtoValidator()
    {
        RuleFor(x=>x.ArabicName).NotEmpty();
        RuleFor(x=>x.EnglishName).NotEmpty();
        RuleFor(x=>x.VehiclesBrandId).NotEmpty();
    }
}
