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
				.WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));

			RuleFor(x => x.EnglishName)
				.NotEmpty()
				.WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
                .MaximumLength(100)
				.Matches(@"^[a-zA-Z][a-zA-Z0-9/.,\\]*(\s[a-zA-Z0-9/.,\\]+)*$")
                .WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));

            RuleFor(x => x.Description)
			   .NotEmpty()
			   .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
			   .MaximumLength(500);

			RuleFor(x => x.PackageTypeId)
				.NotEmpty()
				.WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
				.GreaterThan(0)
				.WithMessage(LocalizationHelper.GetLocalizedString("PackageTypeValidation"));



			RuleFor(x => x.Price)
				.NotEmpty()
				.WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
				.GreaterThan(-1)
				.WithMessage(LocalizationHelper.GetLocalizedString("PriceValidation"))
				.PrecisionScale(18, 2, false)
				.WithMessage(LocalizationHelper.GetLocalizedString("PriceRangeValidation"));

            RuleFor(x => x.PublishFrom)
				.NotEmpty().When(x => !x.Published)
				.WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

            RuleFor(x => x.PublishTo)
				.NotEmpty().When(x => !x.Published)
				.WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));
    
            RuleFor(x => x.DurationOfPackage).NotEmpty().GreaterThanOrEqualTo(1).WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));
            ;

        }


		//private bool BeAValidTime(TimeOnly time)
		//{
		//	var startOfDay = new TimeOnly(12, 0);  // 12:00 PM (noon)
		//	var endOfDay = new TimeOnly(23, 59);   // 23:59 PM (considering midnight as the end of the day)
		//	return time >= startOfDay && time <= endOfDay;
		//}
	}
}
