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

public interface ICalculationsEndPointsAppService : ITransientDependency 
{
    int CalculateTime(DateTime startTime, DateTime endTime, int FreeMinutes);

    decimal CalculatePriceForExceededHours(int totalHours, Organization organization);

    decimal CalculatePriceForOrganizationIsClosedAndExceededHours(int totalHours, Organization organization, CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime);
    decimal CalculatePriceForOrganizationIsClosed(int totalHours, Organization organization, CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime);
    decimal CalculatePriceHours(Organization organization, int hours);
    decimal CalculatePriceHoursExceededDay(Organization organization, int hours);
    (double, double) CalculateHours(CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime, Organization organization);
    int CalculateTimeDependsOnPackage(DateTime? entranceTime, DateTime? exitTime, CustomerPackageDto packageDto);
    int CalculateTimeForCustomPackage( DateTime? exitTime, CustomerPackageDto packageDto);

}
