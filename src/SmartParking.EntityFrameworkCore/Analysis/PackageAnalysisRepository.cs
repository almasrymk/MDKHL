using Microsoft.EntityFrameworkCore;
using SmartParking.Entities;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Organizations;
using SmartParking.EntityFrameworkCore;
using SmartParking.Enums;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
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

    public async Task<PagedResult<PackagePaymentAnalysis>> GetPackagePaymentAnalysis(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<PackagePaymentAnalysis, bool>> filterRequests)
    {
        var dbContext = await GetDbContextAsync();
        if (sorting.Contains("DESC"))
        {
            var sortingSplit = sorting.Split(' ').ToList();
            sorting = sortingSplit[0] + " descending";
        }
        var query = from package in dbContext.Packages
                    where tenantId == null || package.TenantId == tenantId
                    orderby package.Id
                    select new PackagePaymentAnalysis
                    {
                        Package = package,
                        PackageType = package.PackageType,
                        CountUsePackage = package.CustomerPackages.Count(),
                        ValueOfPackage = package.Price,
                        ValueTotalPackage = dbContext.Payments.Where(_ => _.PackageId == package.Id).Sum(x => x.Amount),
                        Date = DateOnly.FromDateTime(package.CreationTime),
                        Time = TimeOnly.FromDateTime(package.CreationTime),
                    
                    };

        query = query.Where(filterRequests);
        query = query.OrderBy(sorting);
        var total = query.Count();
        query = query.Skip(skipCount).Take(maxResultCount);

        return new PagedResult<PackagePaymentAnalysis>
        {
            PageSize = total,
            Queryable = query,
        };
    }
}
