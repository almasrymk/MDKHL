using SmartParking.IntegrationUnit.Dtos;
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.IntegrationUnit;

public interface IIntegrationHandler : IApplicationService
{
    public Task<Result<BookingDto>> HandleBookingAsync(CreateUpdateBookingDto input);
}
