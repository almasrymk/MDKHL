using SmartParking.Entities.Analysis;
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Repositories;

public interface IPackageAnalysisRepository : IRepository<Package, int>
{
    Task<List<PackagePaymentAnalysis>> GetPackagePaymentAnalysis(Guid? tenantId);
}
