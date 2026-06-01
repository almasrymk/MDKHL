using SmartParking.IntegrationUnit;
using SmartParking.IntegrationUnit.Dtos;
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.IntegrationUnits;

public class IntegrationHandler : ApplicationService, IIntegrationHandler
{
    private readonly IIntegrationUnitAppService _integrationUnitAppService;

    public IntegrationHandler(IIntegrationUnitAppService integrationUnitAppService)
    {
        _integrationUnitAppService = integrationUnitAppService;
    }

    public async Task<Result<BookingDto>> HandleBookingAsync(CreateUpdateBookingDto input)
    {
        if (input.CustomerId is not null)
        {
            var updateResult = await _integrationUnitAppService.UpdateBookingAsync(input);
            if (!updateResult.IsSuccess)
            {
                return Result.Error();
            }
            return Result.Success<BookingDto>(updateResult);
        }

        var result = await _integrationUnitAppService.CreateBookingAsync(input);
        if (!result.IsSuccess)
        {
            return Result.Error();
        }
        return Result.Success<BookingDto>(result);
    }
}
