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
                    join shift in dbContext.Shifts
                        on zone.ShiftId equals shift.Id

                    // Join with Garage and Organization using the IDs in the Shift entity
                    join garage in dbContext.Garages
                        on shift.GarageId equals garage.Id
                    join org in dbContext.Organizations
                        on shift.OrganizationId equals org.Id

                    // Optional Tenant filtering
                    where tenantId.HasValue ? staff.TenantId == tenantId : true

                    select new ShiftAnalysis
                    {
                        KeeperId = staff.StaffId,
                        KeeperName = staff.StaffEnName,

                        ShiftId = shift.Id,
                        ShiftNameAr = shift.ArabicName,
                        ShiftNameEn = shift.EnglishName,

                        GarageId = shift.GarageId,
                        GarageNameAr = garage.NameAr,
                        GarageNameEn = garage.NameEn,

                        OrganizationId = shift.OrganizationId,
                        OrganizationNameAr = org.ArabicName,
                        OrganizationNameEn = org.EnglishName,

                        StartDate = shift.StartDate,
                        EndDate = shift.EndDate,
                        StartTime = shift.StartTime,
                        EndTime = shift.EndTime,

                        NumberOfTransaction = dbContext.GarageTransactions
                        .Where(t =>
                            // 1. Physical Location Match
                            t.GarageId == shift.GarageId &&

                            // 2. Device Match (Dispenser or Camera)
                            (t.DispenserId == details.ShiftDevices.DeviceId ||
                                t.CameraId == details.ShiftDevices.DeviceId) &&

                            // 3. Date Range Match
                            DateOnly.FromDateTime((DateTime)t.AccurateTime!) >= shift.StartDate &&
                            DateOnly.FromDateTime((DateTime)t.AccurateTime) <= shift.EndDate &&

                            // 4. Time Logic (Handles FullDay, Normal, and Midnight)
                            (shift.IsFullDay || (
                                shift.StartTime < shift.EndTime
                                    // Normal Shift: Time must be between Start and End
                                    ? (TimeOnly.FromDateTime((DateTime)t.AccurateTime) >= shift.StartTime &&
                                        TimeOnly.FromDateTime((DateTime)t.AccurateTime) <= shift.EndTime)
                                    // Midnight Shift: Time must be AFTER Start OR BEFORE End
                                    : (TimeOnly.FromDateTime((DateTime)t.AccurateTime) >= shift.StartTime ||
                                        TimeOnly.FromDateTime((DateTime)t.AccurateTime) <= shift.EndTime)
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

        var query = from staff in dbContext.ShiftStaffDetails
                    join details in dbContext.ShiftDetails
                        on staff.ShiftDetailsId equals details.Id
                    join zone in dbContext.ShiftZones
                        on details.ShiftZoneId equals zone.Id
                    join shift in dbContext.Shifts
                        on zone.ShiftId equals shift.Id
                    join garage in dbContext.Garages
                        on shift.GarageId equals garage.Id
                    join org in dbContext.Organizations
                        on shift.OrganizationId equals org.Id

                    where tenantId.HasValue ? staff.TenantId == tenantId : true

                    select new PaymentUserShift
                    {
                        UserName = staff.StaffEnName,

                        ShiftArabicName = shift.ArabicName,
                        ShiftEnglishName = shift.EnglishName,

                        GarageArabicName = garage.NameAr,
                        GarageEnglishName = garage.NameEn,

                        OrganizationArabicName = org.ArabicName,
                        OrganizationEnglishName = org.EnglishName,

                        StartDate = shift.StartDate,
                        EndDate = shift.EndDate,
                        StartTime = shift.StartTime,
                        EndTime = shift.EndTime,

                        TotalAmount = dbContext.Payments
                            .Where(p =>
                                p.CreatorId == staff.StaffId &&

                                (!tenantId.HasValue || p.TenantId == tenantId) &&

                                DateOnly.FromDateTime(p.CreationTime.Date) >= shift.StartDate &&
                                DateOnly.FromDateTime(p.CreationTime.Date) <= shift.EndDate &&

                                (shift.IsFullDay || (
                                    shift.StartTime < shift.EndTime
                                        ? (TimeOnly.FromDateTime(p.CreationTime) >= shift.StartTime &&
                                           TimeOnly.FromDateTime(p.CreationTime) <= shift.EndTime)
                                        : (TimeOnly.FromDateTime(p.CreationTime) >= shift.StartTime ||
                                           TimeOnly.FromDateTime(p.CreationTime) <= shift.EndTime)
                                ))
                            )
                            .Sum(p => (decimal?)p.Amount) ?? 0
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
}
