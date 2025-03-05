using SmartParking.Entities.Analysis;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
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

namespace SmartParking.Repositories;

public interface IGarageTransactionAnalysisRepository : IRepository<GarageTransaction, int>
{
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysis(CustomerCategoryStatus category, Guid? tenantId);
    Task<PagedResult<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisReportAsync(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<GarageTransactionAnalysis, bool>> filterRequests);
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisPlate(string PlateNumber , Guid? tenantId);
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisCustomerId(Guid CustomerId, Guid? tenantId);
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisDateCustomerId(Guid CustomerId, DateTime startDate, DateTime endDate);
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisDateCustomerName(string UserName, DateTime startDate, DateTime endDate,CustomerCategoryStatus customerCategory, Guid? tenantId);
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisDate(Guid? tenantId);


}
