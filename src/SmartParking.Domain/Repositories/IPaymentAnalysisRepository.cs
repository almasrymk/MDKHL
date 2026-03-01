using SmartParking.Entities.Analysis;
using SmartParking.Entities.Payments;
using System;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Repositories;

public interface IPaymentAnalysisRepository : IRepository<Payment, Guid>
{
//    Task<PagedResult<PaymentUserShift>> GetPaymentCollectedMoneyWithKeeperAndShiftAnalysis(Guid? tenantId,
//        int skipCount, int maxResultCount, string sorting,
//        Expression<Func<PaymentUserShift, bool>> filterRequests);
    Task<PagedResult<PaymentAnalysis>> GetPaymentForKeeper(Guid? tenantId, int skipCount, int maxResultCount, string sorting, Expression<Func<PaymentAnalysis, bool>> filterRequests);
    Task<PagedResult<DiscountPaymentAnalysis>> GetReportDiscount(Guid? tenantId,
       int skipCount, int maxResultCount, string sorting,
       Expression<Func<DiscountPaymentAnalysis, bool>> filterRequests, string searchTerm);
}
