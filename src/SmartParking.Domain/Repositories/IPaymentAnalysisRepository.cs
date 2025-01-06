using SmartParking.Entities.Analysis;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Repositories;

public interface IPaymentAnalysisRepository : IRepository<Payment, Guid>
{
    Task<List<PaymentAnalysis>> GetPaymentCollectedMoneyDailyAndMonthlyAnalysis(int? organizationId);
}
