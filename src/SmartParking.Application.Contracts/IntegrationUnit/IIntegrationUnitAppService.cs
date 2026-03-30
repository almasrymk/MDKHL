using SmartParking.Results;
using System.Threading.Tasks;

namespace SmartParking.IntegrationUnit;

public interface IIntegrationUnitAppService
{
    Task<Result<BookingDto>> CreateBookingAsync(CreateUpdateBookingDto createUpdateBookingDto);
    Task<Result<BookingDto>> UpdateBookingAsync(CreateUpdateBookingDto dto);
}
