using SmartParking.Entities.Analysis;
using SmartParking.Entities.GarageTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Repositories;

public interface IGarageTransactionAnalysisRepository : IRepository<GarageTransaction, int>
{
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsCustomerAnalysis();
}
