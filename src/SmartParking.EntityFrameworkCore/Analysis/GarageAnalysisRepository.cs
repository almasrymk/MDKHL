using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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


        /// <summary>
        /// Gets the garages with transactions in a given date range.
        /// </summary>
        public async Task<List<GarageAnalysis>> GetGarageStatusRange(DateTime startDate, DateTime endDate)
        {
            var dbContext = await GetDbContextAsync();
            var query = from garage in dbContext.Garages
                        join garageTransaction in dbContext.GarageTransactions
                        .Include(x=>x.Garage)
                        on garage.Id equals garageTransaction.GarageId
                        where garageTransaction.AccurateTime >= startDate
                              && garageTransaction.AccurateTime <= endDate
                        select new GarageAnalysis
                        {
                            Garage = garage,
                            
                        };

            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets the garages with transactions on a specific accurate date.
        /// </summary>
        public async Task<List<GarageAnalysis>> GetGarageStatusByDate(DateTime accurateDate)
        {
            var dbContext = await GetDbContextAsync();
            var query = from garageTransaction in dbContext.GarageTransactions
                        join garage in dbContext.Garages
                        on garageTransaction.GarageId equals garage.Id
                        where garageTransaction.AccurateTime.Value.Date == accurateDate.Date
                        select new GarageAnalysis
                        {
                            Garage = garage,
                            
                        };

            return await query.ToListAsync();
        }
    }
}
