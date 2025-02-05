using SmartParking.Entities.Analysis;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Entities.Organizations;

public interface IGarageAnalysisRepository : IRepository<Garage, int>
{
    Task<PagedResult<GarageAnalysis>> GetGarageReportAsync(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<GarageAnalysis, bool>> filterRequests);


}
