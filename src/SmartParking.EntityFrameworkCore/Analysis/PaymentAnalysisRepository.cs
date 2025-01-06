using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.EntityFrameworkCore;
using SmartParking.Enums;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
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

   
    public async Task<List<PaymentAnalysis>> GetPaymentCollectedMoneyDailyAndMonthlyAnalysis(int? organizationId)
    {
        var dbContext = await GetDbContextAsync();
        var query = from p in dbContext.Payments
                    where p.OrganizationId == null || p.OrganizationId == organizationId
                    from user in dbContext.Users
                    where p.CreatorId == user.Id
                    from roleId in user.Roles.Select(r => r.RoleId) 
                    join role in dbContext.Roles 
                        on roleId equals role.Id 
                    where role.Name == RolesType.Keeper.ToString() 
                    group p by new
                    {
                        p.CreatorId,
                        Day = DateTime.Now.Date,
                        Month = new { p.CreationTime.Year, p.CreationTime.Month }
                    } into g
                    select new PaymentAnalysis
                    {
                        KeeperName = dbContext.Users.Where(u => u.Id == g.Key.CreatorId)
                                          .Select(u => u.Name)
                                          .FirstOrDefault(),
                        AmountDaily = g.Where(x => g.Key.Day.DayOfYear == DateTime.Now.DayOfYear).Sum(x => (double)x.Amount),
                        AmountMonthly = g.Where(x => g.Key.Month.Year == DateTime.Now.Year &&
                                                      g.Key.Month.Month == DateTime.Now.Month).Sum(x => (double)x.Amount),
                        Day = g.Key.Day.ToString("yyyy-MM-dd"),
                        Month = $"{g.Key.Month.Year}-{g.Key.Month.Month:D2}"
                    };

        return await query.ToListAsync();
    }


}
