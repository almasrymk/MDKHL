using SmartParking.Entities.Analysis;
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Repositories;

public interface IPackageAnalysisRepository : IRepository<Package, int>
{
    Task<PagedResult<PackagePaymentAnalysis>> GetPackagePaymentAnalysis(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<PackagePaymentAnalysis, bool>> filterRequests);
}
