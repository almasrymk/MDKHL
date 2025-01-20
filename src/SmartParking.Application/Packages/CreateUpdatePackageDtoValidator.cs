using FluentValidation;
using System;
using System.Data;

namespace SmartParking.Packages
{
	public  class CreateUpdatePackageDtoValidator : AbstractValidator<CreateUpdatePackageDto>
	{
        public CreateUpdatePackageDtoValidator()
        {
            RuleFor(x => x.ArabicName)
				.NotEmpty().MaximumLength(100)
				.Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
				.WithMessage("The Arabic name should only contains Arabic letters, digits, punctuation marks, and spaces.");

			RuleFor(x => x.EnglishName)
				.NotEmpty().MaximumLength(100)
				.Matches(@"^[a-zA-Z][a-zA-Z0-9/.,\\]*(\s[a-zA-Z0-9/.,\\]+)*$")
				.WithMessage("The Englis name should only contains English letters, digits, punctuation marks, and spaces.");

			RuleFor(x => x.Description)
			   .NotEmpty().MaximumLength(500);

			RuleFor(x => x.PackageTypeId)
				.NotEmpty()
				.GreaterThan(0)
				.WithMessage("Package Type is invalid");

			//RuleFor(x => x.Status)
			//	.NotEmpty()
			//	.WithMessage("Status is invalid");

			RuleFor(x => x.Price)
				.GreaterThan(-1)
				.WithMessage("Price mustnt be negative.")
				.PrecisionScale(18, 2, false)
				.WithMessage("Price must not have more than two decimal places and must be less than 18 digits.");

			RuleFor(x => x.PublishFrom)		
			.LessThan(x => x.PublishTo)
			.When( x => !x.Published )
			.WithMessage("Start time must be before to the end time.");
		////.Must(BeAValidTime).WithMessage("Start time is not a valid time.");

			RuleFor(x => x.PublishTo);
				//.Must(BeAValidTime).WithMessage("End time is not a valid time.");

			RuleFor(x => x.DurationOfPackage).NotEmpty().GreaterThanOrEqualTo(1);

		}


		//private bool BeAValidTime(TimeOnly time)
		//{
		//	var startOfDay = new TimeOnly(12, 0);  // 12:00 PM (noon)
		//	var endOfDay = new TimeOnly(23, 59);   // 23:59 PM (considering midnight as the end of the day)
		//	return time >= startOfDay && time <= endOfDay;
		//}
	}
}
