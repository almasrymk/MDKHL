using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Repositories;

public interface IGarageTransactionWorkerRepository
{
    Task UpdateTransactionsAsync(int GracePeriod, int organizationId);
}
