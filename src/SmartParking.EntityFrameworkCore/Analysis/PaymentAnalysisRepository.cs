using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Payments;
using SmartParking.EntityFrameworkCore;
using SmartParking.Enums;
using SmartParking.Repositories;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
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

    //public async Task<PagedResult<PaymentUserShift>> GetPaymentCollectedMoneyWithKeeperAndShiftAnalysis(
    //Guid? tenantId,
    //int skipCount, int maxResultCount, string sorting,
    //Expression<Func<PaymentUserShift, bool>> filterRequests)
    //{
    //    sorting = CheckSort(sorting);
    //    var dbContext = await GetDbContextAsync();

    //    var query = from payments in dbContext.Payments
    //                where tenantId == null || payments.TenantId == tenantId
    //                join shiftZone in dbContext.ShiftZones
    //                    on payments.OrganizationId equals shiftZone.OrganizationId
    //                group shiftZone by new
    //                {
    //                    shiftZone.Id,
    //                    shiftZone.ShiftId,
    //                    shiftZone.ZoneId,
    //                    shiftZone.ZoneArName,
    //                    shiftZone.ZoneEnName,
    //                    shiftZone.OrganizationId,
    //                    shiftZone.OrganizationArName,
    //                    shiftZone.OrganizationEnName,
    //                    shiftZone.ShiftManager.ManagerEnName,
    //                    shiftZone.ShiftManager.ManagerUserName,
    //                    shiftZone.ShiftManager.ManagerPhoneNumber,
    //                } into groupedPayments
    //                select new PaymentUserShift
    //                {
    //                    UserName = groupedPayments.Key.ManagerEnName,

    //                    TotalAmount = dbContext.Payments
    //                        .Where(p =>
    //                            p.OrganizationId == groupedPayments.Key.OrganizationId &&
    //                            p.TenantId == tenantId)
    //                        .Sum(p => (decimal?)p.Amount) ?? 0,

    //                    GarageArabicName = groupedPayments.Key.ZoneArName,
    //                    GarageEnglishName = groupedPayments.Key.ZoneEnName,

    //                    OrganizationArabicName = groupedPayments.Key.OrganizationArName,
    //                    OrganizationEnglishName = groupedPayments.Key.OrganizationEnName,

    //                    ShiftArabicName = dbContext.Shifts
    //                        .Where(s => s.Id == groupedPayments.Key.ShiftId)
    //                        .Select(s => s.ArabicName)
    //                        .FirstOrDefault(),
    //                    ShiftEnglishName = dbContext.Shifts
    //                        .Where(s => s.Id == groupedPayments.Key.ShiftId)
    //                        .Select(s => s.EnglishName)
    //                        .FirstOrDefault(),

    //                    StartDate = dbContext.Shifts
    //                        .Where(s => s.Id == groupedPayments.Key.ShiftId)
    //                        .Select(s => s.StartDate)
    //                        .FirstOrDefault(),
    //                    EndDate = dbContext.Shifts
    //                        .Where(s => s.Id == groupedPayments.Key.ShiftId)
    //                        .Select(s => s.EndDate)
    //                        .FirstOrDefault(),
    //                    StartTime = dbContext.Shifts
    //                        .Where(s => s.Id == groupedPayments.Key.ShiftId)
    //                        .Select(s => s.StartTime)
    //                        .FirstOrDefault(),
    //                    EndTime = dbContext.Shifts
    //                        .Where(s => s.Id == groupedPayments.Key.ShiftId)
    //                        .Select(s => s.EndTime)
    //                        .FirstOrDefault(),
    //                };

    //    query = query.Where(filterRequests);
    //    query = query.OrderBy(sorting);

    //    var total = query.Count();
    //    query = query.Skip(skipCount).Take(maxResultCount);

    //    return new PagedResult<PaymentUserShift>
    //    {
    //        PageSize = total,
    //        Queryable = query,
    //    };
    //}


    public async Task<PagedResult<PaymentAnalysis>> GetPaymentForKeeper(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<PaymentAnalysis, bool>> filterRequests)
    {
        var dbContext = await GetDbContextAsync();
        sorting = CheckSort(sorting);
        var query = from payment in dbContext.Payments
                    where tenantId == null ? true : payment.TenantId == tenantId
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


    public async Task<PagedResult<DiscountPaymentAnalysis>> GetReportDiscount(Guid? tenantId,
       int skipCount, int maxResultCount, string sorting,
       Expression<Func<DiscountPaymentAnalysis, bool>> filterRequests, string searchTerm)
    {
        sorting = CheckSort(sorting);
        var dbContext = await GetDbContextAsync();
        var query = from payments in dbContext.Payments
                    where tenantId == null || payments.TenantId == tenantId
                    where (payments.Invoice != null && payments.Invoice.Discount > 0) || (payments.CustomerPackage != null && payments.CustomerPackage.Discount > 0)
                    select new DiscountPaymentAnalysis
                    {
                        Discount = payments != null && payments.Invoice != null ? payments.Invoice.Discount : payments!.CustomerPackage!.Discount,
                        Amount = payments.Amount,
                        Currency = payments.Currency,
                        CustomerArabicName = payments.Customers.ArabicName ?? "NotCustomer",
                        CustomerEnglishName = payments.Customers.IdentityUser.Name ?? "NotCustomer",
                        PlateNumber = payments != null && payments.Invoice != null ? payments.Invoice.PlateNumber : payments!.CustomerPackage!.EnPlateNumber,
                        InvoiceId = payments.InvoiceId,
                        CustomerPackageId = payments.CustomerPackageId,
                        PaymentCategory = payments.PaymentCategory,
                        PaymentDate = payments.CreationTime,
                        PaymentMethod = payments.PaymentMethod,
                        PaymentNumber = payments.PaymentNumber,
                        PaymentStatus = payments.PaymentStatus,
                        TenantId = payments.TenantId,
                        Date = DateOnly.FromDateTime(payments.CreationTime),
                        Time = TimeOnly.FromDateTime(payments.CreationTime),
                    };
        query = query.Where(filterRequests);
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(_ => _.Discount.ToString().Contains(searchTerm));
        query = query.OrderBy(sorting);
        var total = query.Count();
        query = query.Skip(skipCount).Take(maxResultCount);
        return new PagedResult<DiscountPaymentAnalysis>
        {
            PageSize = total,
            Queryable = query,
        };
    }

    private static string CheckSort(string sorting)
    {
        if (sorting.Contains("DESC"))
        {
            var sortingSplit = sorting.Split(' ').ToList();
            sorting = sortingSplit[0] + " descending";
        }

        return sorting;
    }
}
