using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.EntityFrameworkCore;
using SmartParking.Enums;
using SmartParking.Migrations;
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

public class PaymentAnalysisRepository
     : EfCoreRepository<SmartParkingDbContext, Payment, Guid>,
      IPaymentAnalysisRepository
{
    private readonly IDataFilter _dataFilter;

    public PaymentAnalysisRepository(
        IDbContextProvider<SmartParkingDbContext> dbContextProvider, IDataFilter dataFilter)
        : base(dbContextProvider)
    {
        this._dataFilter = dataFilter;
    }

   
    public async Task<PagedResult<PaymentUserShift>> GetPaymentCollectedMoneyWithKeeperAndShiftAnalysis(Guid? tenantId, 
        int skipCount, int maxResultCount, string sorting, 
        Expression<Func<PaymentUserShift, bool>> filterRequests)
    {
        if (sorting.Contains("DESC"))
        {
            var sortingSplit = sorting.Split(' ').ToList();
            sorting = sortingSplit[0] + " descending";
        }
        var dbContext = await GetDbContextAsync();
        var query = from payments in dbContext.Payments
                    where tenantId == null || payments.TenantId == tenantId
                    join userShift in dbContext.KeeperShifts on payments.CreatorId equals userShift.KeeperId
    
                    select new PaymentUserShift
                    {
                        UserName = dbContext.Users.Where(u => u.Id == userShift.KeeperId)
                                          .Select(u => u.Name)
                                          .FirstOrDefault(),
                        TotalAmount = dbContext.Payments
                                .Where(p => p.CreationTime.Date >= userShift.StartDate.ToDateTime(new TimeOnly(0, 0))&&
                                       p.CreationTime.Date <= userShift.EndDate.ToDateTime(new TimeOnly(23, 59))
                                )
                                .Sum(x => (decimal?)x.Amount) ?? 0, 
                        EndDate = userShift.EndDate,
                        StartDate = userShift.StartDate,
                        EndTime = userShift.EndTime,
                        GarageArabicName = dbContext.Garages.Where(g => g.Id == userShift.GarageId).Select(g => g.NameAr).FirstOrDefault(),
                        GarageEnglishName = dbContext.Garages.Where(g => g.Id == userShift.GarageId).Select(g => g.NameEn).FirstOrDefault(),
                        OrganizationArabicName = dbContext.Organizations.Where(g => g.Id == userShift.OrganizationId).Select(g => g.ArabicName).FirstOrDefault(),
                        OrganizationEnglishName = dbContext.Organizations.Where(g => g.Id == userShift.OrganizationId).Select(g => g.EnglishName).FirstOrDefault(),
                        ShiftArabicName = dbContext.Shifts.Where(g => g.Id == userShift.ShiftId).Select(g => g.ArabicName).FirstOrDefault(),
                        ShiftEnglishName = dbContext.Shifts.Where(g => g.Id == userShift.ShiftId).Select(g => g.EnglishName).FirstOrDefault(),
                        StartTime = userShift.StartTime

                    };
        query = query.Where(filterRequests);
        query = query.OrderBy(sorting);
        var total = query.Count();
        query = query.Skip(skipCount).Take(maxResultCount);
        return new PagedResult<PaymentUserShift>
        {
            PageSize = total,
            Queryable = query,
        };
    }
    
    
    public async Task<PagedResult<PaymentAnalysis>> GetPaymentForKeeper(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<PaymentAnalysis, bool>> filterRequests)
    {
        var dbContext = await GetDbContextAsync();
        if (sorting.Contains("DESC"))
        {
            var sortingSplit = sorting.Split(' ').ToList();
            sorting = sortingSplit[0] + " descending";
        }
        var query = from payment in dbContext.Payments
                    where tenantId == null? true : payment.TenantId == tenantId
                    from user in dbContext.Users
                    where payment.CreatorId == user.Id
                    from roleId in user.Roles.Select(r => r.RoleId) 
                    join role in dbContext.Roles 
                        on roleId equals role.Id 
                    where role.Name == RolesType.Keeper.ToString() 
                    group payment by new
                    {
                        
                        payment.CreatorId,
                        Day = DateTime.Now.Date,
                        Month = new { payment.CreationTime.Year, payment.CreationTime.Month }
                    } into g
                    select new PaymentAnalysis
                    {
                        Payment = g.FirstOrDefault(),
                        KeeperName = dbContext.Users.Where(u => u.Id == g.Key.CreatorId)
                                          .Select(u => u.Name)
                                          .FirstOrDefault(),
                        AmountDaily = g.Where(x => g.Key.Day.DayOfYear == DateTime.Now.DayOfYear).Sum(x => (double)x.Amount),
                        AmountMonthly = g.Where(x => g.Key.Month.Year == DateTime.Now.Year &&
                                                      g.Key.Month.Month == DateTime.Now.Month).Sum(x => (double)x.Amount),
                        Day = g.Key.Day.ToString("yyyy-MM-dd"),
                        Month = $"{g.Key.Month.Year}-{g.Key.Month.Month:D2}"
                    };
        query = query.Where(filterRequests);
        query = query.OrderBy(sorting);
        var total = query.Count();
        query = query.Skip(skipCount).Take(maxResultCount);
        return new PagedResult<PaymentAnalysis>
        {
            PageSize = total,
            Queryable = query,
        };
    }

   


}
