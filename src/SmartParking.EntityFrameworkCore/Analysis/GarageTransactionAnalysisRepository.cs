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

    public async Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysis(CustomerCategoryStatus category)
    {
        var dbContext = await GetDbContextAsync();

        var query = from garageTransaction in dbContext.GarageTransactions
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
                       PlateNumber = garageTransaction.PlateNumber,
                       AccurateTime = garageTransaction.AccurateTime,
                       CameraType = garageTransaction.CameraType.ToString(),
                        GarageName = garageTransaction.Garage.NameEn,
                       OrganizationName = garageTransaction.Organization.EnglishName
                    };
        return await query.ToListAsync();
    }
}
