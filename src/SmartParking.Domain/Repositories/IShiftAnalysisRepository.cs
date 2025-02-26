using SmartParking.Entities.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Repositories;
public interface IShiftAnalysisRepository
{
    Task<PagedResult<ShiftAnalysis>> ReportNumberOfTransactionsWhileKeeperShift(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<ShiftAnalysis, bool>> filterRequests);
   // Task<PagedResult<ShiftAnalysis>> ReportForTotalAmountWhileKeeperShift(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<ShiftAnalysis, bool>> filterRequests);
}
