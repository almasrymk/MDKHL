using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.GarageTransactions;
using SmartParking.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Repositories;

public class GarageTransactionWorkerRepository : EfCoreRepository<SmartParkingDbContext, GarageTransaction, int>,
          IGarageTransactionWorkerRepository
{

    public GarageTransactionWorkerRepository(IDbContextProvider<SmartParkingDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task UpdateTransactionsAsync(int GracePeriod)
    {

        var dbContext = await GetDbContextAsync();
        var now = DateTime.Now;
        var threeDaysAgo = now.AddDays(-3);
         await dbContext.GarageTransactions
         .Where(t => t.CheckoutMobile != null &&
                     t.CreationTime >= threeDaysAgo)
         .ExecuteUpdateAsync(setters => setters
             .SetProperty(
                 t => t.RemainingTime,
                 t => t.CheckoutMobile.Value.AddMinutes(GracePeriod) <= now
                     ? DateTime.MinValue
                     : t.CheckoutMobile.Value.AddMinutes(GracePeriod)
             )
             .SetProperty(
                 t => t.IsPending,
                 t => t.CheckoutMobile.Value.AddMinutes(GracePeriod) > now
             )
             .SetProperty(
                 t => t.CanCheckout,
                 t => t.CheckoutMobile.Value.AddMinutes(GracePeriod) <= now
             )
         );
        
    }
}
