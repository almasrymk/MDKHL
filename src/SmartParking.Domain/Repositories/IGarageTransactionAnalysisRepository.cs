using SmartParking.Entities.Analysis;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Repositories;

public interface IGarageTransactionAnalysisRepository : IRepository<GarageTransaction, int>
{
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysis(CustomerCategoryStatus category, Guid? tenantId);
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisPlate(string PlateNumber);
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisCustomerId(Guid CustomerId);
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisDateCustomerId(Guid CustomerId, DateTime startDate, DateTime endDate);
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisDate(CustomerCategoryStatus category, DateTime startDate, DateTime endDate);


}
