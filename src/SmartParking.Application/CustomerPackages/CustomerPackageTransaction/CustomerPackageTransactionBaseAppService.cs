using Microsoft.EntityFrameworkCore;
using SmartParking.CustomerPackages.CustomerPackageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactions;
using System;
using SmartParking.Extensions;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using SmartParking.CustomerPackages.Dtos;

namespace SmartParking.CustomerPackages.CustomerPackageTransaction;

[RemoteService(false)]
public abstract class CustomerPackageTransactionBaseAppService : SmartParkingAppService, ICustomerPackageTransactionActionsAppService
{
    protected readonly IDataFilter DataFilter;
    protected readonly IRepository<CustomerPackage, int> Repository;

    protected CustomerPackageTransactionBaseAppService(
        IRepository<CustomerPackage, int> repository,
        IDataFilter dataFilter)
    {
        Repository = repository;
        DataFilter = dataFilter;
    }

    /// <summary>
    /// Abstract method to be implemented by derived classes to determine if they can handle the transaction
    /// </summary>
    public abstract bool CanHandle(GarageTransactionDto garageTransactionDto);

    /// <summary>
    /// Shared logic to get one active customer package based on the transaction details
    /// </summary>
    public virtual async Task<CustomerPackageTransactionDto> GetOneActiveCustomerPackage(GarageTransactionDto garageTransactionDto)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            if (!ValidateGarageTransactionDto(garageTransactionDto))
            {
                return null!;
            }

            var query = await Repository.GetQueryableAsync();
            query = ApplyActivePackageFilters(query, garageTransactionDto);

            var customerPackage = await query.FirstOrDefaultAsync();
            return ObjectMapper.Map<CustomerPackage, CustomerPackageTransactionDto>(customerPackage);
        }
    }

    /// <summary>
    /// Validates the garage transaction DTO - can be overridden by derived classes for specific validation
    /// </summary>
    protected virtual bool ValidateGarageTransactionDto(GarageTransactionDto garageTransactionDto)
    {
        return garageTransactionDto?.AccurateTime != null;
    }

    /// <summary>
    /// Applies all filters to find active packages - core shared logic
    /// </summary>
    protected virtual IQueryable<CustomerPackage> ApplyActivePackageFilters(
        IQueryable<CustomerPackage> query,
        GarageTransactionDto garageTransactionDto)
    {

        //var accurateTime = garageTransactionDto.AccurateTime.Value;
        //var timeOfDay = TimeOnly.FromTimeSpan(accurateTime.TimeOfDay);
       query = query.GetLastActivePackage(garageTransactionDto.OrganizationID);

        // Apply identification filters (plate or ticket)
        query = ApplyIdentificationFilters(query, garageTransactionDto);



        return query;
    }

    /// <summary>
    /// Applies plate number or ticket ID filters - can be overridden by derived classes
    /// </summary>
    protected virtual IQueryable<CustomerPackage> ApplyIdentificationFilters(
        IQueryable<CustomerPackage> query,
        GarageTransactionDto garageTransactionDto)
    {
        // By default, support both plate and ticket matching
        return query.Where(x =>
            (!string.IsNullOrEmpty(garageTransactionDto.PlateNumber) &&
             (x.ArPlateNumber == garageTransactionDto.PlateNumber ||
              x.EnPlateNumber == garageTransactionDto.PlateNumber))
            ||
            (!string.IsNullOrEmpty(garageTransactionDto.TicketId) &&
             x.PackageCode == garageTransactionDto.TicketId)
        );
    }

   
}