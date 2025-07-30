using Abp.Application.Services;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Garages;
using System.Threading.Tasks;

namespace SmartParking.Utilities;
public interface IGarageUtilityAppService : IApplicationService
{
    Task<Garage> GetGarageByIdAsync(int garageId);
    Task<bool> ReserverSlotsOfGarageDependsOnCategoryAsync(int garageId, bool hasFreeSlots, CustomerCategoryStatus customer);
    Task<bool> FreeSlotsOfGarageDependsOnCategoryAsync(int garageId, bool hasFreeSlots, CustomerCategoryStatus category);
}
