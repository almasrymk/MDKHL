using Polly;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Shifts;
using SmartParking.EntityFrameworkCore;
using SmartParking.Repositories;
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

namespace SmartParking.Analysis;
public class ShiftAnalysisRepository : EfCoreRepository<SmartParkingDbContext, KeeperShift, int>,IShiftAnalysisRepository
{
    private readonly IDataFilter _dataFilter;
    public ShiftAnalysisRepository(
        IDbContextProvider<SmartParkingDbContext> dbContextProvider, IDataFilter dataFilter)
        : base(dbContextProvider)
    {
        this._dataFilter = dataFilter;
    }

    public async Task<PagedResult<ShiftAnalysis>> ReportNumberOfTransactionsWhileKeeperShift(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<ShiftAnalysis, bool>> filterRequests)
    {
        var dbContext = await GetDbContextAsync();
        if (sorting.Contains("DESC"))
        {
            var sortingSplit = sorting.Split(' ').ToList();
            sorting = sortingSplit[0] + " descending";
        }
        var query = from keeperShift in dbContext.KeeperShifts
                    where tenantId.HasValue ? keeperShift.TenantId == tenantId : true 
                    join garage in dbContext.Garages
                        on keeperShift.GarageId equals garage.Id
                    join user in dbContext.Users
                    on keeperShift.KeeperId equals user.Id
                    join org in dbContext.Organizations
                    on keeperShift.OrganizationId equals org.Id
                    join shift in dbContext.Shifts
                    on keeperShift.ShiftId equals shift.Id
                    select new ShiftAnalysis
                    {
                        KeeperId = keeperShift.KeeperId,
                        ShiftId = keeperShift.ShiftId,
                        KeeperName = user.Name,
                        GarageId = keeperShift.GarageId,
                        GarageNameAr = garage.NameAr, 
                        GarageNameEn = garage.NameEn,
                        OrganizationId = keeperShift.OrganizationId,
                        OrganizationNameAr = org.ArabicName, 
                        OrganizationNameEn = org.EnglishName,
                        ShiftNameAr = shift.ArabicName,
                        ShiftNameEn = shift.EnglishName,
                        StartDate = keeperShift.StartDate, 
                        EndDate = keeperShift.EndDate,
                        StartTime = keeperShift.StartTime,
                        EndTime = keeperShift.EndTime,
                        NumberOfTransaction = dbContext.GarageTransactions
                            .Where(_ =>
                                DateOnly.FromDateTime((DateTime)_.AccurateTime!) >= keeperShift.StartDate &&
                                DateOnly.FromDateTime((DateTime)_.AccurateTime) <= keeperShift.EndDate &&
                                TimeOnly.FromDateTime((DateTime)_.AccurateTime) >= keeperShift.StartTime &&
                                TimeOnly.FromDateTime((DateTime)_.AccurateTime) <= keeperShift.EndTime
                            )
                            .Count()
                    };
        query = query.Where(filterRequests);
        query = query.OrderBy(sorting);
        var total = query.Count();
        query = query.Skip(skipCount).Take(maxResultCount);

        return new PagedResult<ShiftAnalysis>
        {
            PageSize = total,
            Queryable = query,
        };
    }


    //public async Task<PagedResult<ShiftAnalysis>> ReportForTotalAmountWhileKeeperShift(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<ShiftAnalysis, bool>> filterRequests)
    //{
    //    var dbContext = await GetDbContextAsync();
    //    var query = from keeperShift in dbContext.KeeperShifts
    //                join payment in dbContext.Payments
    //                on keeperShift.KeeperId equals payment.CreatorId
    //                where
    //              payment. >= keeperShift.StartDate 
    //              && payment.PaymentDateTime.Date <= keeperShift.EndDate
    //              && payment.Date.TimeOfDay >= keeperShift.StartTime.ToTimeSpan() 
    //              && payment.PaymentDateTime.TimeOfDay <= keeperShift.EndTime.ToTimeSpan()
    //                group {}
                    
    //                select new ShiftAnalysis
    //                {

    //                };


        
    //    return new PagedResult<ShiftAnalysis>
    //    {

    //    };
    //}
}
