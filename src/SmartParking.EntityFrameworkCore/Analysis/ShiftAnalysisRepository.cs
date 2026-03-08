using SmartParking.Entities.Analysis;
using SmartParking.Entities.Shifts;
using SmartParking.EntityFrameworkCore;
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

public class ShiftAnalysisRepository : EfCoreRepository<SmartParkingDbContext, KeeperShift, int>, IShiftAnalysisRepository
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
        var query = from staff in dbContext.ShiftStaffDetails
                    join details in dbContext.ShiftDetails
                        on staff.ShiftDetailsId equals details.Id
                    join zone in dbContext.ShiftZones
                        on details.ShiftZoneId equals zone.Id

                    // Optional Tenant filtering
                    where tenantId.HasValue ? staff.TenantId == tenantId : true

                    select new ShiftAnalysis
                    {
                        KeeperId = staff.StaffId,
                        KeeperName = staff.StaffEnName,

                        ShiftId = zone.ShiftId,
                        ShiftNameAr = zone.ShiftArabicName,
                        ShiftNameEn = zone.ShiftEnglishName,

                        GarageId = zone.ZoneId,
                        GarageNameAr = zone.ZoneArName,
                        GarageNameEn = zone.ZoneEnName,

                        OrganizationId = zone.OrganizationId,
                        OrganizationNameAr = zone.OrganizationArName,
                        OrganizationNameEn = zone.OrganizationEnName,

                        StartDate = zone.StartDate,
                        EndDate = zone.EndDate,
                        StartTime = zone.StartTime,
                        EndTime = zone.EndTime,

                        NumberOfTransaction = dbContext.GarageTransactions
                        .Where(t =>

                            // 3. Date Range Match
                            DateOnly.FromDateTime((DateTime)t.AccurateTime!) >= zone.StartDate &&
                            DateOnly.FromDateTime((DateTime)t.AccurateTime) <= zone.EndDate &&

                            // 4. Time Logic (Handles FullDay, Normal, and Midnight)
                            (zone.IsFullDay || (
                                zone.StartTime < zone.EndTime
                                    // Normal Shift: Time must be between Start and End
                                    ? (TimeOnly.FromDateTime((DateTime)t.AccurateTime) >= zone.StartTime &&
                                        TimeOnly.FromDateTime((DateTime)t.AccurateTime) <= zone.EndTime)
                                    // Midnight Shift: Time must be AFTER Start OR BEFORE End
                                    : (TimeOnly.FromDateTime((DateTime)t.AccurateTime) >= zone.StartTime ||
                                        TimeOnly.FromDateTime((DateTime)t.AccurateTime) <= zone.EndTime)
                            ))
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

    public async Task<PagedResult<PaymentUserShift>> ReportPaymentCollectedMoneyWithShiftZone(
    Guid? tenantId,
    int skipCount, int maxResultCount, string sorting,
    Expression<Func<PaymentUserShift, bool>> filterRequests)
    {
        if (sorting.Contains("DESC"))
        {
            var sortingSplit = sorting.Split(' ').ToList();
            sorting = sortingSplit[0] + " descending";
        }
        var dbContext = await GetDbContextAsync();

        var staffPayments =
            from staff in dbContext.ShiftStaffDetails
            join details in dbContext.ShiftDetails on staff.ShiftDetailsId equals details.Id
            join zone in dbContext.ShiftZones on details.ShiftZoneId equals zone.Id
            join payment in dbContext.Payments on staff.StaffId equals payment.CreatorId
            where (!tenantId.HasValue || zone.TenantId == tenantId)
               && (!tenantId.HasValue || payment.TenantId == tenantId)
               && DateOnly.FromDateTime(payment.CreationTime.Date) >= zone.StartDate
               && DateOnly.FromDateTime(payment.CreationTime.Date) <= zone.EndDate
               && (zone.IsFullDay || (
                       zone.StartTime < zone.EndTime
                           ? (TimeOnly.FromDateTime(payment.CreationTime) >= zone.StartTime &&
                              TimeOnly.FromDateTime(payment.CreationTime) <= zone.EndTime)
                           : (TimeOnly.FromDateTime(payment.CreationTime) >= zone.StartTime ||
                              TimeOnly.FromDateTime(payment.CreationTime) <= zone.EndTime)))
            select new
            {
                UserName = staff.StaffEnName,
                CreatorId = staff.StaffId,
                ShiftArabicName = zone.ShiftArabicName,
                ShiftEnglishName = zone.ShiftEnglishName,
                GarageArabicName = zone.ZoneArName,
                GarageEnglishName = zone.ZoneEnName,
                OrgArabicName = zone.OrganizationArName,
                OrgEnglishName = zone.OrganizationEnName,
                zone.StartDate,
                zone.EndDate,
                zone.StartTime,
                zone.EndTime,
                payment.Amount
            };

        var managerPayments =
            from zone in dbContext.ShiftZones
            join payment in dbContext.Payments on zone.ShiftManager.ShiftManagerId equals payment.CreatorId
            where (!tenantId.HasValue || zone.TenantId == tenantId)
               && (!tenantId.HasValue || payment.TenantId == tenantId)
               && DateOnly.FromDateTime(payment.CreationTime.Date) >= zone.StartDate
               && DateOnly.FromDateTime(payment.CreationTime.Date) <= zone.EndDate
               && (zone.IsFullDay || (
                       zone.StartTime < zone.EndTime
                           ? (TimeOnly.FromDateTime(payment.CreationTime) >= zone.StartTime &&
                              TimeOnly.FromDateTime(payment.CreationTime) <= zone.EndTime)
                           : (TimeOnly.FromDateTime(payment.CreationTime) >= zone.StartTime ||
                              TimeOnly.FromDateTime(payment.CreationTime) <= zone.EndTime)))
            select new
            {
                UserName = zone.ShiftManager.ManagerEnName,
                CreatorId = zone.ShiftManager.ShiftManagerId,
                ShiftArabicName = zone.ShiftArabicName,
                ShiftEnglishName = zone.ShiftEnglishName,
                GarageArabicName = zone.ZoneArName,
                GarageEnglishName = zone.ZoneEnName,
                OrgArabicName = zone.OrganizationArName,
                OrgEnglishName = zone.OrganizationEnName,
                zone.StartDate,
                zone.EndDate,
                zone.StartTime,
                zone.EndTime,
                payment.Amount
            };

        var combined = staffPayments.Union(managerPayments);

        var query = combined
            .GroupBy(x => new
            {
                x.UserName,
                x.CreatorId,
                x.ShiftArabicName,
                x.ShiftEnglishName,
                x.GarageArabicName,
                x.GarageEnglishName,
                x.OrgArabicName,
                x.OrgEnglishName,
                x.StartDate,
                x.EndDate,
                x.StartTime,
                x.EndTime
            })
            .Select(g => new PaymentUserShift
            {
                UserName = g.Key.UserName,
                ShiftArabicName = g.Key.ShiftArabicName,
                ShiftEnglishName = g.Key.ShiftEnglishName,
                GarageArabicName = g.Key.GarageArabicName,
                GarageEnglishName = g.Key.GarageEnglishName,
                OrganizationArabicName = g.Key.OrgArabicName,
                OrganizationEnglishName = g.Key.OrgEnglishName,
                StartDate = g.Key.StartDate,
                EndDate = g.Key.EndDate,
                StartTime = g.Key.StartTime,
                EndTime = g.Key.EndTime,
                TotalAmount = g.Sum(x => (decimal?)x.Amount) ?? 0
            });

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
}
