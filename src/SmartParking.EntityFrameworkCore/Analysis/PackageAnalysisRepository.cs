using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Organizations;
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

public class PackageAnalysisRepository : EfCoreRepository<SmartParkingDbContext, Package, int>,
          IPackageAnalysisRepository
{
   
        private readonly IDataFilter _dataFilter;

        public PackageAnalysisRepository(
            IDbContextProvider<SmartParkingDbContext> dbContextProvider, IDataFilter dataFilter)
            : base(dbContextProvider)
        {
            this._dataFilter = dataFilter;
        }

    public async Task<List<PackagePaymentAnalysis>> GetPackagePaymentAnalysis(Guid? tenantId)
    {
        var dbContext = await GetDbContextAsync();
        var query = from package in dbContext.Packages
                    where tenantId == null || package.TenantId == tenantId
                    select new PackagePaymentAnalysis
                    {
                        Package = package,
                        CountUsePackage = package.CustomerPackages.Count(),
                        ValueOfPackage = package.Price,
                        ValueTotalPackage = dbContext.Payments.Where(_ => _.PackageId == package.Id).Sum(x => x.Amount),
                    
                    };

        return await query.ToListAsync();
    }
}
