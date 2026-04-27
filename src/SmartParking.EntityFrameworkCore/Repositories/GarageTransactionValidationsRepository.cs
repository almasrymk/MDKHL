using Microsoft.EntityFrameworkCore;
using SmartParking.Data.Models;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.GarageTransactions;
using SmartParking.EntityFrameworkCore;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SmartParking.Repositories;

public class GarageTransactionValidationsRepository : EfCoreRepository<SmartParkingDbContext, GarageTransaction, int>,
          IGarageTransactionValidationsRepository
{
    public GarageTransactionValidationsRepository(IDbContextProvider<SmartParkingDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<List<CustomerPackageTransactionDto>> CountOfVehicleRelatedToCustomerPackageParkingInGarage(Guid customerId , int organizationId)
    {
        var accurateTime = DateTime.Now;
        var dbContext = await GetDbContextAsync();

        var query =
      from cp in dbContext.CustomerPackages
      join gt in dbContext.GarageTransactions on 1 equals 1
      where cp.CustomerId == customerId
            && cp.OrganizationId == organizationId
            && cp.CustomerPackageStatus == CustomerPackageStatus.Active
            && cp.ActivationDateOfPackage.HasValue
            && cp.ActivationDateOfPackage.Value.Date <= accurateTime.Date
            && cp.ExpiryDate.Date >= accurateTime.Date

            && gt.CameraType == WatcherType.Entry
            && gt.IsIn
            && gt.OrganizationID == organizationId
            && gt.PlateNumber != null
            && (
                  (cp.ArPlateNumber != null && gt.PlateNumber == cp.ArPlateNumber) ||
                  (cp.EnPlateNumber != null && gt.PlateNumber == cp.EnPlateNumber)
               )
      select new CustomerPackageTransactionDto
            {
                CustomerPackageId = cp.Id,
                CustomerId = cp.CustomerId,
                PlateNumber = gt.PlateNumber,
                AccurateTime=gt.AccurateTime,
                CustomerPackageStatus = cp.CustomerPackageStatus,
                TransactionId = gt.Id,


            };

        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<UnitOccupancyDto>> GetCountOfVehiclesInsideByUnitAsync(Guid customerId, int organizationId)
    {
        var accurateTime = DateTime.Now;
        var dbContext = await GetDbContextAsync();

        var query = from uvp in dbContext.UnitVehiclePackges
                    join gt in dbContext.GarageTransactions on organizationId equals gt.OrganizationID

                    where uvp.ActivationDate <= accurateTime
                    && uvp.DectivationDate >= accurateTime
                    && uvp.PackageCode == gt.TicketId
                    && gt.IsIn == true

                    select new
                    {
                        uvp.UnitCode,
                        uvp.EnPlateNumber
                    };

        var result = await query
            .Distinct()
            .GroupBy(x => x.UnitCode)
            .Select(g => new UnitOccupancyDto
            {
                UnitCode = g.Key,
                VehicleCount = g.Count()
            }).ToListAsync();

        return result;
    }
}
