using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SmartParking.Repositories
{
    public class GarageAnalysisRepository
        : EfCoreRepository<SmartParkingDbContext, Garage, int>,
          IGarageAnalysisRepository
    {
        private readonly IDataFilter _dataFilter;

        public GarageAnalysisRepository(
            IDbContextProvider<SmartParkingDbContext> dbContextProvider, IDataFilter dataFilter)
            : base(dbContextProvider)
        {
            this._dataFilter = dataFilter;
        }

        public async Task<PagedResult<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisReportAsync(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<GarageTransactionAnalysis, bool>> filterRequests)
        {

            var dbContext = await GetDbContextAsync();

            if (sorting.Contains("DESC"))
            {
                var sortingSplit = sorting.Split(' ').ToList();
                sorting = sortingSplit[0] + " descending";
            }


            var query = from garageTransaction in dbContext.GarageTransactions
                        where tenantId == null || garageTransaction.TenantId == tenantId
                        join vehicle in dbContext.Vehicles
                            on garageTransaction.PlateNumber equals vehicle.EnPlateNumber
                            into vehicleJoin1
                        from v1 in vehicleJoin1.DefaultIfEmpty()
                        join vehicle2 in dbContext.Vehicles
                            on garageTransaction.PlateNumber equals vehicle2.ArPlateNumber
                            into vehicleJoin2
                        from v2 in vehicleJoin2.DefaultIfEmpty()
                        orderby garageTransaction.Garage.NameEn
                        select new GarageTransactionAnalysis
                        {
                            GarageTransaction = garageTransaction,
                            CustomerId = (v1 == null) ? (v2 == null) ? Guid.Empty : v2.CustomerId : (Guid)v1.CustomerId,
                            PlateNumber = garageTransaction.PlateNumber,
                            AccurateTime = garageTransaction.AccurateTime,
                            CameraType = garageTransaction.CameraType,
                            GarageName = garageTransaction.Garage.NameEn,
                            OrganizationName = garageTransaction.Organization.EnglishName,
                            ArabicName = (v1 == null) ? (v2 == null) ? "غير معرف" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                            UserName = (v1 == null) ? (v2 == null) ? "Unknown" : v2.Customer.IdentityUser.UserName : v1.Customer.IdentityUser.UserName

                        };
            query = query.Where(filterRequests);
            query = query.OrderBy(sorting);
            var total = query.Count();
            query = query.Skip(skipCount).Take(maxResultCount);

            return new PagedResult<GarageTransactionAnalysis>
            {
                PageSize = total,
                Queryable = query,
            };

        }
        /// <summary>
        /// Gets the garages with transactions in a given date range.
        /// </summary>
        public async Task<PagedResult<GarageAnalysis>> GetGarageReportAsync(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<GarageAnalysis, bool>> filterRequests)
        {
            var dbContext = await GetDbContextAsync();

            if (sorting.Contains("DESC"))
            {
                var sortingSplit = sorting.Split(' ').ToList();
                sorting = sortingSplit[0] + " descending";
            }

            var query = from garage in dbContext.Garages
                        where tenantId == null || garage.TenantId == tenantId
                        join garageTransaction in dbContext.GarageTransactions
                        .Include(x=>x.Garage)
                        on garage.Id equals garageTransaction.GarageId
                        orderby garage.Id
                        select new GarageAnalysis
                        {
                            Garage = garage,
                            
                        };

            query = query.Where(filterRequests);
            query = query.OrderBy(sorting);
            var total = query.Count();
            query = query.Skip(skipCount).Take(maxResultCount);

            return new PagedResult<GarageAnalysis>
            {
                PageSize = total,
                Queryable = query,
            };
        }

      
    }
}
