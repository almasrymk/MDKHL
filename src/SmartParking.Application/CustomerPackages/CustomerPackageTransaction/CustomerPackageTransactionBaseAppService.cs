using Microsoft.EntityFrameworkCore;
using SmartParking.CustomerPackages.CustomerPackageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

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
        var accurateTime = garageTransactionDto.AccurateTime.Value;
        var timeOfDay = TimeOnly.FromTimeSpan(accurateTime.TimeOfDay);

        query = query.Where(x =>
            x.OrganizationId == garageTransactionDto.OrganizationID &&
            x.CustomerPackageStatus == CustomerPackageStatus.Active &&
            x.ActivationDateOfPackage.HasValue &&
            x.ExpiryDate != null &&
            x.ActivationDateOfPackage.Value.Date <= accurateTime.Date &&
            x.ExpiryDate.Date >= accurateTime.Date &&
            ((x.IsCustomPackage && x.PackageId == null) || (!x.IsCustomPackage && x.PackageId != null))
        );

        // Apply identification filters (plate or ticket)
        query = ApplyIdentificationFilters(query, garageTransactionDto);

        // Apply time-of-day restrictions
        query = ApplyTimeOfDayFilters(query, timeOfDay);

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

    /// <summary>
    /// Applies time-of-day restrictions with 24-hour access and midnight crossing support
    /// </summary>
    protected virtual IQueryable<CustomerPackage> ApplyTimeOfDayFilters(
        IQueryable<CustomerPackage> query,
        TimeOnly timeOfDay)
    {
        return query.Where(x =>
            // 24-hour access - no time restrictions
            x.publishedOfPackage == true
            ||
            // Time-restricted access
            (
                x.PublishFromOfPackage.HasValue &&
                x.PublishToOfPackage.HasValue &&
                (
                    // Normal hours (doesn't cross midnight)
                    (x.PublishFromOfPackage.Value <= x.PublishToOfPackage.Value &&
                     x.PublishFromOfPackage.Value <= timeOfDay &&
                     x.PublishToOfPackage.Value >= timeOfDay)
                    ||
                    // Midnight crossing (e.g., 22:00 to 02:00)
                    (x.PublishFromOfPackage.Value > x.PublishToOfPackage.Value &&
                     (x.PublishFromOfPackage.Value <= timeOfDay ||
                      x.PublishToOfPackage.Value >= timeOfDay))
                )
            )
        );
    }
}