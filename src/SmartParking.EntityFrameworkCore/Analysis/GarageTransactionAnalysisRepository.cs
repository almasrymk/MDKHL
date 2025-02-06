using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Vehicles;
using SmartParking.EntityFrameworkCore;
using SmartParking.Enums;
using SmartParking.Repositories;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

namespace SmartParking.Analysis;

public class GarageTransactionAnalysisRepository
    : EfCoreRepository<SmartParkingDbContext, GarageTransaction, int>,
      IGarageTransactionAnalysisRepository
{
    private readonly IDataFilter _dataFilter;

    public GarageTransactionAnalysisRepository(
        IDbContextProvider<SmartParkingDbContext> dbContextProvider, IDataFilter dataFilter)
        : base(dbContextProvider)
    {
        this._dataFilter = dataFilter;
    }

    public async Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysis(CustomerCategoryStatus category,Guid? tenantId)
    {
        var dbContext = await GetDbContextAsync();

        var query = from garageTransaction in dbContext.GarageTransactions
                    where tenantId == null || garageTransaction.TenantId == tenantId
                    where garageTransaction.CustomerCategory == category
                    join vehicle in dbContext.Vehicles
                        on garageTransaction.PlateNumber equals vehicle.EnPlateNumber
                        into vehicleJoin1
                    from v1 in vehicleJoin1.DefaultIfEmpty()
                    join vehicle2 in dbContext.Vehicles
                        on garageTransaction.PlateNumber equals vehicle2.ArPlateNumber
                        into vehicleJoin2
                    from v2 in vehicleJoin2.DefaultIfEmpty()
                    
                    select new GarageTransactionAnalysis
                    {
                        GarageTransaction = garageTransaction,
                        CustomerId = (v1 == null) ? (v2 == null) ? Guid.Empty : v2.CustomerId : (Guid)v1.CustomerId ,
                        PlateNumber = garageTransaction.PlateNumber,
                        AccurateTime = garageTransaction.AccurateTime,
                        CameraType = garageTransaction.CameraType,
                        GarageName = garageTransaction.Garage.NameEn,
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        ArabicName = (v1 == null) ? (v2 == null) ? "غير معرف" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                        UserName = (v1 == null) ? (v2 == null) ? "Unknown" : v2.Customer.IdentityUser.UserName : v1.Customer.IdentityUser.UserName

                    };
        return await query.ToListAsync();
    }

  
    public async Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisPlate(string PlateNumber)
    {
        var dbContext = await GetDbContextAsync();

        var query = from garageTransaction in dbContext.GarageTransactions
                    where garageTransaction.PlateNumber == PlateNumber

                    join vehicle1 in dbContext.Vehicles
                    .Include(x => x.Customer).ThenInclude(x => x.IdentityUser)
                        on garageTransaction.PlateNumber equals vehicle1.EnPlateNumber
                        into vehicleJoin1
                    from v1 in vehicleJoin1.DefaultIfEmpty()

                    join vehicle2 in dbContext.Vehicles
                    .Include(x => x.Customer)
                        on garageTransaction.PlateNumber equals vehicle2.ArPlateNumber
                        into vehicleJoin2
                    from v2 in vehicleJoin2.DefaultIfEmpty()

                    select new GarageTransactionAnalysis
                    {
                        PlateNumber = garageTransaction.PlateNumber,
                        AccurateTime = garageTransaction.AccurateTime,
                        CameraType = garageTransaction.CameraType,
                        GarageName = garageTransaction.Garage.NameEn,
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        ArabicName = (v1 == null) ? (v2 == null) ? "غير معرف" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                        UserName = (v1 == null) ? (v2 == null) ? "Unknown" : v2.Customer.IdentityUser.UserName : v1.Customer.IdentityUser.UserName,
                        CustomerId = (v1 == null) ? (v2 == null) ? Guid.Empty : v2.CustomerId : (Guid)v1.CustomerId

                    };

        return await query.ToListAsync();
    }
    public async Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisCustomerId(Guid CustomerId)
    {
        var dbContext = await GetDbContextAsync();

        var query = from garageTransaction in dbContext.GarageTransactions

                    join vehicle1 in dbContext.Vehicles
                    .Include(x => x.Customer).ThenInclude(x => x.IdentityUser)
                        on garageTransaction.PlateNumber equals vehicle1.EnPlateNumber
                        into vehicleJoin1
                    from v1 in vehicleJoin1.DefaultIfEmpty()

                    join vehicle2 in dbContext.Vehicles
                    .Include(x => x.Customer)
                        on garageTransaction.PlateNumber equals vehicle2.ArPlateNumber
                        into vehicleJoin2
                    from v2 in vehicleJoin2.DefaultIfEmpty()


                    where (v1.Customer != null && v1.CustomerId == CustomerId) ||
                     (v2.Customer != null && v2.Customer.Id == CustomerId)

                    select new GarageTransactionAnalysis
                    {
                        Customer = (v1.Customer == null )? (v2.Customer ==null) ? null:v2.Customer:v1.Customer ,
                        Vehicle = v1??v2,
                        GarageTransaction = garageTransaction,
                        PlateNumber = garageTransaction.PlateNumber,
                        AccurateTime = garageTransaction.AccurateTime,
                        CameraType = garageTransaction.CameraType,
                        GarageName = garageTransaction.Garage.NameEn,
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        ArabicName = (v1 == null) ? (v2 == null) ? "غير معرف" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                        UserName = (v1 == null) ? (v2 == null) ? "Unknown" : v2.Customer.IdentityUser.UserName : v1.Customer.IdentityUser.UserName,
                        CustomerId = (v1 == null) ? (v2 == null) ? Guid.Empty : v2.CustomerId : (Guid)v1.CustomerId

                    };

        return await query.ToListAsync();
    }
    public async Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisDate(Guid? tenantId)
    {
        var dbContext = await GetDbContextAsync();

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

                    select new GarageTransactionAnalysis
                    {
                        PlateNumber = garageTransaction.PlateNumber,
                        AccurateTime = garageTransaction.AccurateTime,
                        CameraType = garageTransaction.CameraType,
                        GarageName = garageTransaction.Garage.NameEn,
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        ArabicName = (v1 == null) ? (v2 == null) ? "غير معرف" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                        UserName = (v1 == null) ? (v2 == null) ? "Unknown" : v2.Customer.IdentityUser.UserName : v1.Customer.IdentityUser.UserName,
                        CustomerId = (v1 == null) ? (v2 == null) ? Guid.Empty : v2.CustomerId : (Guid)v1.CustomerId
                    };

        return await query.ToListAsync();
    }


    public async Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisDateCustomerId(Guid CustomerId, DateTime startDate, DateTime endDate)
    {
        var dbContext = await GetDbContextAsync();

        var query = from garageTransaction in dbContext.GarageTransactions

                    join vehicle1 in dbContext.Vehicles
                    .Include(x => x.Customer).ThenInclude(x => x.IdentityUser)
                        on garageTransaction.PlateNumber equals vehicle1.EnPlateNumber
                        into vehicleJoin1
                    from v1 in vehicleJoin1.DefaultIfEmpty()

                    join vehicle2 in dbContext.Vehicles
                    .Include(x => x.Customer)
                        on garageTransaction.PlateNumber equals vehicle2.ArPlateNumber
                        into vehicleJoin2
                    from v2 in vehicleJoin2.DefaultIfEmpty()


                    where (v1.Customer != null && v1.CustomerId == CustomerId) ||
                     (v2.Customer != null && v2.Customer.Id == CustomerId)
                     && garageTransaction.AccurateTime.Value.Date >= startDate.Date
                          && garageTransaction.AccurateTime.Value.Date <= endDate.Date

                    select new GarageTransactionAnalysis
                    {
                        Customer = (v1.Customer == null) ? (v2.Customer == null) ? null : v2.Customer : v1.Customer,
                        Vehicle = v1,
                        GarageTransaction = garageTransaction,
                        PlateNumber = garageTransaction.PlateNumber,
                        AccurateTime = garageTransaction.AccurateTime,
                        CameraType = garageTransaction.CameraType,
                        GarageName = garageTransaction.Garage.NameEn,
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        ArabicName = (v1 == null) ? (v2 == null) ? "غير معرف" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                        UserName = (v1 == null) ? (v2 == null) ? "Unknown" : v2.Customer.IdentityUser.UserName : v1.Customer.IdentityUser.UserName,
                        CustomerId = (v1 == null) ? (v2 == null) ? Guid.Empty : v2.CustomerId : (Guid)v1.CustomerId
                    };

        return await query.ToListAsync();
    }

    //GetGarageTransactionsAnalysisDateCustomerName

    public async Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisDateCustomerName(string UserName, DateTime startDate, DateTime endDate, CustomerCategoryStatus category, Guid? tenantId)
    {
        var dbContext = await GetDbContextAsync();

        var query = from garageTransaction in dbContext.GarageTransactions

                    join vehicle1 in dbContext.Vehicles
                    .Include(x => x.Customer).ThenInclude(x => x.IdentityUser)
                        on garageTransaction.PlateNumber equals vehicle1.EnPlateNumber
                        into vehicleJoin1
                    from v1 in vehicleJoin1.DefaultIfEmpty()

                    join vehicle2 in dbContext.Vehicles
                    .Include(x => x.Customer)
                        on garageTransaction.PlateNumber equals vehicle2.ArPlateNumber
                        into vehicleJoin2
                    from v2 in vehicleJoin2.DefaultIfEmpty()


                    where (v1.Customer != null &&   v1.Customer.IdentityUser.UserName == UserName) ||
                     (v2.Customer != null &&   v2.Customer.IdentityUser.UserName == UserName)
                     && garageTransaction.AccurateTime.Value.Date >= startDate.Date
                          && garageTransaction.AccurateTime.Value.Date <= endDate.Date
                         && garageTransaction.CustomerCategory == category
                    where tenantId == null || garageTransaction.TenantId == tenantId


                    select new GarageTransactionAnalysis
                    {
                        Customer = (v1.Customer == null) ? (v2.Customer == null) ? null : v2.Customer : v1.Customer,
                        Vehicle = v1??v2,
                        GarageTransaction = garageTransaction,
                        PlateNumber = garageTransaction.PlateNumber,
                        AccurateTime = garageTransaction.AccurateTime,
                        CameraType = garageTransaction.CameraType,
                        GarageName = garageTransaction.Garage.NameEn,
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        ArabicName = (v1 == null) ? (v2 == null) ? "غير معرف" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                        UserName = (v1 == null) ? (v2 == null) ? "Unknown" : v2.Customer.IdentityUser.UserName : v1.Customer.IdentityUser.UserName,
                        CustomerId = (v1 == null) ? (v2 == null) ? Guid.Empty : v2.CustomerId : (Guid)v1.CustomerId
                    };

        return await query.ToListAsync();
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
}
