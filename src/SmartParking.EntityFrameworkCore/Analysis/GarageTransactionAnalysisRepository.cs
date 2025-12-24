using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.EntityFrameworkCore;
using SmartParking.Enums;
using SmartParking.Migrations;
using SmartParking.Repositories;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Volo.Abp.TenantManagement;
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
                        GarageNameAr = garageTransaction.Garage.NameAr,
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        OrganizationNameAr = garageTransaction.Organization.ArabicName,
                        ArabicName = (v1 == null) ? (v2 == null) ? "غير معرف" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                        UserName = (v1 == null) ? (v2 == null) ? "Unknown" : v2.Customer.IdentityUser.UserName : v1.Customer.IdentityUser.UserName

                    };
        return await query.ToListAsync();
    }

  
    public async Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisPlate(string PlateNumber, Guid? tenantId)
    {
        var dbContext = await GetDbContextAsync();

        var query = from garageTransaction in dbContext.GarageTransactions
                    where tenantId == null || garageTransaction.TenantId == tenantId
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
                        GarageNameAr = garageTransaction.Garage.NameAr,
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        OrganizationNameAr = garageTransaction.Organization.ArabicName,
                        ArabicName = (v1 == null) ? (v2 == null) ? "غير معرف" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                        UserName = (v1 == null) ? (v2 == null) ? "Unknown" : v2.Customer.IdentityUser.UserName : v1.Customer.IdentityUser.UserName,
                        CustomerId = (v1 == null) ? (v2 == null) ? Guid.Empty : v2.CustomerId : (Guid)v1.CustomerId,
                        GarageTransaction = garageTransaction,
                        ShiftArabicName = dbContext.Shifts
                                            .Where(_ => ( _.StartDate <= DateOnly.FromDateTime(garageTransaction.AccurateTime.Value) && _.EndDate >= DateOnly.FromDateTime(garageTransaction.AccurateTime.Value))&&
                                                  (_.StartTime <= TimeOnly.FromDateTime(garageTransaction.AccurateTime.Value) && _.EndTime >= TimeOnly.FromDateTime(garageTransaction.AccurateTime.Value)) )
                                            .Select(_ => _.ArabicName)
                                            .FirstOrDefault(),                       
                        ShiftEnglishName = dbContext.Shifts
                                            .Where(_ => ( _.StartDate <= DateOnly.FromDateTime(garageTransaction.AccurateTime.Value) && _.EndDate >= DateOnly.FromDateTime(garageTransaction.AccurateTime.Value))&&
                                                  (_.StartTime <= TimeOnly.FromDateTime(garageTransaction.AccurateTime.Value) && _.EndTime >= TimeOnly.FromDateTime(garageTransaction.AccurateTime.Value)) )
                                            .Select(_ => _.EnglishName)
                                            .FirstOrDefault(),
                        CreatorName = dbContext.Users.Where(_ => _.Id == garageTransaction.CreatorId).Select(_ => _.Name).FirstOrDefault(),
                        CreationDate = v1 == null ? v2 == null ? null : DateOnly.FromDateTime(v2.CreationTime) : DateOnly.FromDateTime(v1.CreationTime),
                        CreationTime = v1 == null ? v2 == null ? null : TimeOnly.FromDateTime(v2.CreationTime) : TimeOnly.FromDateTime(v1.CreationTime),
                        

                    };

        return await query.ToListAsync();
    }
    public async Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisCustomerId(Guid CustomerId,Guid? tenantId)
    {
        var dbContext = await GetDbContextAsync();

        var query = from garageTransaction in dbContext.GarageTransactions
                    where tenantId == null || garageTransaction.TenantId == tenantId
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
                        GarageNameAr = garageTransaction.Garage.NameAr,
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        OrganizationNameAr = garageTransaction.Organization.ArabicName,
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
                        GarageNameAr = garageTransaction.Garage.NameAr , 
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        OrganizationNameAr = garageTransaction.Organization.ArabicName,
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
                        GarageNameAr = garageTransaction.Garage.NameAr , 
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        OrganizationNameAr = garageTransaction.Organization.ArabicName,
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
                        GarageNameAr = garageTransaction.Garage.NameAr, 
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        OrganizationNameAr = garageTransaction.Organization.ArabicName,
                        ArabicName = (v1 == null) ? (v2 == null) ? "غير معرف" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                        UserName = (v1 == null) ? (v2 == null) ? "Unknown" : v2.Customer.IdentityUser.Name : v1.Customer.IdentityUser.Name,
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
                        GarageNameAr = garageTransaction.Garage.NameAr,
                        OrganizationName = garageTransaction.Organization.EnglishName,
                        OrganizationNameAr = garageTransaction.Organization.ArabicName,
                        ArabicName = (v1 == null) ? (v2 == null) ? "غير معرف" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                        UserName = (v1 == null) ? (v2 == null) ? "Unknown" : v2.Customer.IdentityUser.Name : v1.Customer.IdentityUser.Name,
                        TransactionTime = TimeOnly.FromDateTime(garageTransaction.AccurateTime.Value),
                        TransactionDate = DateOnly.FromDateTime(garageTransaction.AccurateTime.Value),
                        TicketId = garageTransaction.TicketId

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

    public async Task<List<CancelGarageTransactionAnalysis>> GetCanceledGarageTransactionsAnalysisReportAsync(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<CancelGarageTransactionAnalysis, bool>> filterRequests)
    {
        using (_dataFilter.Disable<ISoftDelete>())
        {
            var dbContext = await GetDbContextAsync();
            var query =
                    from garageTransaction in dbContext.GarageTransactions
                    where garageTransaction.IsDeleted == true
                          && (tenantId == null || garageTransaction.TenantId == tenantId)
                    join invoice in dbContext.Invoices
                        on garageTransaction.Id equals invoice.GarageTransactionExitId
                    join user in dbContext.Users
                        on garageTransaction.DeleterId equals user.Id
                    join vehicle in dbContext.Vehicles
                        on garageTransaction.PlateNumber equals vehicle.EnPlateNumber
                        into vehicleJoin1
                    from v1 in vehicleJoin1.DefaultIfEmpty()
                    join vehicle2 in dbContext.Vehicles
                        on garageTransaction.PlateNumber equals vehicle2.ArPlateNumber
                        into vehicleJoin2
                    from v2 in vehicleJoin2.DefaultIfEmpty()
                    join shift in dbContext.Shifts
                        on 1 equals 1 into shiftJoin
                                from shift in shiftJoin
                                    .Where(s =>
                                        s.StartDate <= DateOnly.FromDateTime(garageTransaction.DeletionTime!.Value) &&
                                        s.EndDate >= DateOnly.FromDateTime(garageTransaction.DeletionTime.Value) &&
                                        s.StartTime <= TimeOnly.FromDateTime(garageTransaction.DeletionTime.Value) &&
                                        s.EndTime >= TimeOnly.FromDateTime(garageTransaction.DeletionTime.Value))
                                    .DefaultIfEmpty()
                    select new CancelGarageTransactionAnalysis
                    {
                        CashierName = user.Name,

                        CustomerNameAr = invoice.CustomerArName != "غير معرف"? invoice.CustomerArName : (v1!=null)?v1.Customer.ArabicName: (v2 != null) ? v2.Customer.ArabicName : "غير معرف",
                        CustomerNameEn = invoice.CustomerEnName != "unknown" ? invoice.CustomerEnName : (v1 != null) ? v1.Customer.IdentityUser.Name : (v2 != null) ? v2.Customer.IdentityUser.Name : "unknown",

                        ShiftArabicName = shift.ArabicName,
                        ShiftEnglishName = shift.EnglishName,

                        TransactionTime = TimeOnly.FromDateTime(garageTransaction.AccurateTime!.Value),
                        TransactionDate = DateOnly.FromDateTime(garageTransaction.AccurateTime!.Value),

                        PlateNumber = garageTransaction.PlateNumber,
                        Duration = invoice.TotalHours,
                        Price = invoice.InvoicePrice
                    };
            query = query.Where(filterRequests);
            query = query.OrderBy(sorting);
            //var total = query.Count();
            //query = query.Skip(skipCount).Take(maxResultCount);
            return await query.ToListAsync() ;
        }

    }

    public async Task<PagedResult<QRTransactionAnalysis>> GetQRTransactionAnalysesAsync(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<QRTransactionAnalysis, bool>> filterRequests)
    {
        using (_dataFilter.Disable<ISoftDelete>())
        {
            var dbContext = await GetDbContextAsync();
            var query =
                from garageTransaction in dbContext.GarageTransactions
                where
                    (tenantId == null || garageTransaction.TenantId == tenantId)
                    && garageTransaction.TicketId != null
                    && garageTransaction.IsDeleted == false
                join customerpackages in dbContext.CustomerPackages
                    on garageTransaction.TicketId equals customerpackages.PackageCode
                select new QRTransactionAnalysis
                {
                    PlateNumber = garageTransaction.PlateNumber,
                    TransactionDate = DateOnly.FromDateTime(garageTransaction.AccurateTime!.Value),
                    TransactionTime = TimeOnly.FromDateTime(garageTransaction.AccurateTime.Value),

                    IsCarBelongToPackage =(customerpackages.ArPlateNumber == garageTransaction.PlateNumber 
                                           || customerpackages.EnPlateNumber == garageTransaction.PlateNumber ) ? YesNoEnum.Yes : YesNoEnum.No,

                    PackageCode = garageTransaction.TicketId,
                    PackagePlateNumberAr = customerpackages.ArPlateNumber,
                    PackagePlateNumberEn = customerpackages.EnPlateNumber,
                    CustomerNameAr = customerpackages.Customer.ArabicName,
                    CustomerNameEn = customerpackages.Customer.IdentityUser.Name,
                    CustomerPackageStatus = customerpackages.IsDeleted ? CustomerPackageStatus.Deleted :
                                                garageTransaction.AccurateTime < customerpackages.ActivationDateOfPackage ? CustomerPackageStatus.InActive :
                                                    garageTransaction.AccurateTime < customerpackages.ExpiryDate ? CustomerPackageStatus.Active : CustomerPackageStatus.Expired,
                    PackageNameAr = customerpackages.PackageArabicName,
                    PackageNameEn = customerpackages.PackageEnglishName,
                    CameraType = garageTransaction.CameraType
                };

            query = query.Where(filterRequests);
            query = query.OrderBy(sorting);
            var total = query.Count();
            query = query.Skip(skipCount).Take(maxResultCount);
            return new PagedResult<QRTransactionAnalysis>
            {
                Queryable = query,
                PageSize = total
            };
        }
    }
}
