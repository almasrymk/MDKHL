using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Services.Garages;
public  class GarageManager : DomainService
{
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly IDataFilter _dataFilter;
    public GarageManager(IRepository<Garage, int> garageRepository, IDataFilter dataFilter)
    {
        _garageRepository = garageRepository;
        _dataFilter = dataFilter;
    }
    public async Task<Garage> GetGarageByIdAsync(int garageId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var garage = await _garageRepository.FirstOrDefaultAsync(x => x.Id == garageId && x.Status == Status.Active);
            if (garage == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Garage") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }
            return garage;
        }

    }
    public async Task<bool> ReserverSlotsOfGarageDependsOnCategoryAsync(int garageId, bool hasFreeSlots, CustomerCategoryStatus customer)
    {
        var garage = await GetGarageByIdAsync(garageId);
        bool flag = false;
        if (hasFreeSlots)
        {
            return await Task.FromResult(true);
        }
        switch (customer)
        {
            case CustomerCategoryStatus.Customer:
                if (garage.CurrnetCapacityForCustomers + 1 <= garage.MaxSlotsForCustomers)
                    garage.CurrnetCapacityForCustomers++;
                break;

            case CustomerCategoryStatus.Employee:
                if (garage.CurrnetCapacityEmployees + 1 <= garage.MaxSlotsForEmployess)
                    garage.CurrnetCapacityEmployees++;
                break;

            case CustomerCategoryStatus.Visitor:
                if (garage.CurrnetCapacityForFree + 1 <= garage.MaxSlotsForFree)
                    garage.CurrnetCapacityForFree++;
                break;

            default:
                if (garage.CurrnetCapacityForCustomers + 1 <= garage.MaxSlotsForCustomers)
                    garage.CurrnetCapacityForCustomers++;
                break;

        }
        if (garage.CurrnetCapicity + 1 <= garage.MaxCapacity)
        {
            garage.CurrnetCapicity++;
            flag = true;
        }
        return await Task.FromResult(flag);
    }


    public async Task<bool> FreeSlotsOfGarageDependsOnCategoryAsync(int garageId, bool hasFreeSlots, CustomerCategoryStatus category)
    {
        var garage = await GetGarageByIdAsync(garageId);
        bool flag = false;
        if (hasFreeSlots)
            return await Task.FromResult(true);
        if (category == CustomerCategoryStatus.Customer)
        {
            garage.CurrnetCapacityForCustomers = Math.Max(0, --garage.CurrnetCapacityForCustomers);
            flag = true;

        }
        else if (category == CustomerCategoryStatus.Employee)
        {
            garage.CurrnetCapacityEmployees = Math.Max(0, --garage.CurrnetCapacityEmployees);
            flag = true;
        }
        else
        {
            garage.CurrnetCapacityForFree = Math.Max(0, --garage.CurrnetCapacityForFree);
            flag = true;
        }
        garage.CurrnetCapicity = Math.Max(0, --garage.CurrnetCapicity);
        await _garageRepository.UpdateAsync(garage);
        return flag;
    }
}
