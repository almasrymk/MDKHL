using SmartParking.IntegrationUnit.Dtos;
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SmartParking.IntegrationUnit;

public interface IIntegrationHandler : IScopedDependency
{
    public Task<Result<BookingDto>> HandleBookingAsync(CreateUpdateBookingDto input);
}
