using FluentValidation;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.VehicleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Vehicles.VehiclesLookups
{
	public class CreateUpdateLicenceTypeDtoValidator : AbstractValidator<CreateUpdateLicenceTypeDto>
	{
        public CreateUpdateLicenceTypeDtoValidator()
        {
			RuleFor(x => x.ArabicName).NotEmpty();
			RuleFor(x => x.EnglishName).NotEmpty();
			RuleFor(x => x.PlateColorId).NotEmpty();
		}
    }
}
