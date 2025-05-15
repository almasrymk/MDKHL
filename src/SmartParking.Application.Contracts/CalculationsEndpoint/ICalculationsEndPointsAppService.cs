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
    /// Calculates the price for exceeded hours.
    /// </summary>
    /// <param name="totalHours">The total number of hours.</param>
    /// <param name="organization">The organization.</param>
    /// <returns>The calculated price.</returns>
    decimal CalculatePriceForExceededHours(int totalHours, Organization organization);

    /// <summary>
    /// Calculates the price for an organization that is closed and has exceeded hours.
    /// </summary>
    /// <param name="organization">The organization.</param>
    /// <param name="exitTime">The exit time.</param>
    /// <param name="entranceTime">The entrance time.</param>
    /// <returns>The calculated price.</returns>
    decimal CalculatePriceForOrganizationIsClosedAndExceededHours(Organization organization, DateTime exitTime, DateTime entranceTime);

    /// <summary>
    /// Calculates the price for an organization that is closed.
    /// </summary>
    /// <param name="totalHours">The total number of hours.</param>
    /// <param name="organization">The organization.</param>
    /// <param name="exitTime">The exit time.</param>
    /// <param name="entranceTime">The entrance time.</param>
    /// <returns>The calculated price.</returns>
    decimal CalculatePriceForOrganizationIsClosed(int totalHours, Organization organization, DateTime exitTime, DateTime entranceTime);

    /// <summary>
    /// Calculates the price for hours.
    /// </summary>
    /// <param name="organization">The organization.</param>
    /// <param name="hours">The number of hours.</param>
    /// <returns>The calculated price.</returns>
    decimal CalculatePriceHours(Organization organization, int hours);

    /// <summary>
    /// Calculates the price for hours exceeded in a day.
    /// </summary>
    /// <param name="organization">The organization.</param>
    /// <param name="hours">The number of hours.</param>
    /// <returns>The calculated price.</returns>
    decimal CalculatePriceHoursExceededDay(Organization organization, int hours);

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
}
