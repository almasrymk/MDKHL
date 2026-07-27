using SmartParking.IntegrationUnit.Dtos;
using SmartParking.Results;
using System.Threading.Tasks;

namespace SmartParking.IntegrationUnit;

public interface IIntegrationUnitAppService
{
    Task<Result<BookingDto>> CreateBookingAsync(CreateUpdateBookingDto createUpdateBookingDto);
    Task<Result<BookingDto>> UpdateBookingAsync(CreateUpdateBookingDto dto);
    Task<Result<BookingDto>> CreateBookingV2Async(CreateUpdateBookingV2Dto dto);
    Task<Result<BookingDto>> UpdateBookingV2Async(CreateUpdateBookingV2Dto dto);
}
