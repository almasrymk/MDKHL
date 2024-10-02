using FluentValidation;
using SmartParking.Lookups.PlateColor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Vehicles.VehiclesLookups
{
	public class CreateUpdatePlateColorDtoValidator :AbstractValidator<CreateUpdatePlateColorDto>
	{
        public CreateUpdatePlateColorDtoValidator()
        {

			RuleFor(x => x.ArabicName).NotEmpty();
			RuleFor(x => x.EnglishName).NotEmpty();
		}
    }
}
