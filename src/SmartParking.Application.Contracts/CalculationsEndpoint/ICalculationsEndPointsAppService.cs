using SmartParking.CustomerPackages;
using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace SmartParking.CalculationsEndpoint;


/// <summary>
/// Interface for calculations end points application service.
/// </summary>
public interface ICalculationsEndPointsAppService : ITransientDependency
{
    /// <summary>
    /// Calculates the time between a start and end time, taking into account free minutes.
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <param name="FreeMinutes">The number of free minutes.</param>
    /// <returns>The calculated time.</returns>
    int CalculateTime(DateTime startTime, DateTime endTime, int FreeMinutes);

    /// <summary>
    /// Calculates the hours between an exit and entrance time, taking into account closing hours.
    /// </summary>
    /// <param name="exitTime">The exit time.</param>
    /// <param name="entrenceTime">The entrance time.</param>
    /// <param name="ClosingHoursStart">The start of closing hours.</param>
    /// <param name="ClosingHoursEnd">The end of closing hours.</param>
    /// <returns>A tuple containing the special hours and normal hours.</returns>
    (double, double) CalculateHours(DateTime exitTime, DateTime entrenceTime, TimeOnly ClosingHoursStart, TimeOnly ClosingHoursEnd);

    /// <summary>
    /// Calculates the time depends on a package.
    /// </summary>
    /// <param name="entranceTime">The entrance time.</param>
    /// <param name="exitTime">The exit time.</param>
    /// <param name="packageDto">The package DTO.</param>
    /// <returns>The calculated time.</returns>
    int CalculateTimeDependsOnPackage(DateTime? entranceTime, DateTime? exitTime, CustomerPackageDto packageDto);

    /// <summary>
    /// Calculates the time for a custom package.
    /// </summary>
    /// <param name="exitTime">The exit time.</param>
    /// <param name="packageDto">The package DTO.</param>
    /// <returns>The calculated time.</returns>
    int CalculateTimeForCustomPackage(DateTime? exitTime, CustomerPackageDto packageDto);

    /// <summary>
    /// Method used to calculate invoice depends on Organization Configurations
    /// </summary>
    /// <param name="entranceTime">entrance time</param>
    /// <param name="totalHours">total hours of staying</param>
    /// <param name="exitTime">exit time</param>
    /// <param name="organization">organization</param>
    /// <returns>price of invoice</returns>
    decimal CalculateInvoiceCases(DateTime entranceTime, int totalHours, DateTime exitTime, Organization organization);
    
    decimal CalculationForRegisteredCustomer(DateTime? exitTime, DateTime? entranceTime, Organization organization, CustomerPackageDto? packageDto, int totalHours);


}
