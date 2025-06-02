using FluentValidation;
using SmartParking.Localization;
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
				.WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
                .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
				.WithMessage("The Arabic name should only contains Arabic letters, digits, punctuation marks, and spaces.");

			RuleFor(x => x.EnglishName)
				.NotEmpty()
				.WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
                .MaximumLength(100)
				.Matches(@"^[a-zA-Z][a-zA-Z0-9/.,\\]*(\s[a-zA-Z0-9/.,\\]+)*$")
				.WithMessage("The Englis name should only contains English letters, digits, punctuation marks, and spaces.");

			RuleFor(x => x.Description)
			   .NotEmpty()
			   .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
			   .MaximumLength(500);

			RuleFor(x => x.PackageTypeId)
				.NotEmpty()
				.WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
                .GreaterThan(0)
				.WithMessage("Package Type is invalid");


			RuleFor(x => x.Price)
				.NotEmpty()
				.WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
                .GreaterThan(-1)
				.WithMessage("Price mustnt be negative.")
				.PrecisionScale(18, 2, false)
				.WithMessage("Price must not have more than two decimal places and must be less than 18 digits.");
            RuleFor(x => x.PublishFrom)
				.NotEmpty().When(x => !x.Published)
				.WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

            RuleFor(x => x.PublishTo)
				.NotEmpty().When(x => !x.Published)
				.WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));
    
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
