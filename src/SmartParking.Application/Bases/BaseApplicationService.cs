using System.Linq;
using Volo.Abp.Application.Services;
using Volo.Abp.Validation;

namespace SmartParking.Bases
{
	public class BaseApplicationService : ApplicationService

	{
		public BaseApplicationService()
		{

		}

		#region helper methods
		public AbpValidationException GetValidationException
					(FluentValidation.Results.ValidationResult validationResult)
		{

			var message = string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage));
			var errors =
				validationResult
				.Errors
				.Select(x => new System.ComponentModel.DataAnnotations.ValidationResult
								(x.ErrorMessage, [x.PropertyName]))
				.ToList();
			return new AbpValidationException(message, errors);
		}
		#endregion helper methods
	}
}
