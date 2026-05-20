using Abp.Application.Services;
using iText.Layout.Element;
using SmartParking.UnitPackageIntegrations.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.UnitPackageIntegrations;

public interface IUnitPackageIntegrationGetAppService : IApplicationService
{
    Task<ICollection<UnitPackageIntegrationDto>> GetAllByBookingIdAsync(string bookingId);
    Task<bool> BookingIdExist(string bookingId);
}
