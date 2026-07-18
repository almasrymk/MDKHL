using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Payments.Strategies;

public class PackagePaymentCompletionStrategy : IPaymentCompletionStrategy, ITransientDependency
{
    private readonly IRepository<CustomerPackage, int> _customerPackageRepo;

    public PackagePaymentCompletionStrategy(IRepository<CustomerPackage, int> customerPackageRepo)
    {
        _customerPackageRepo = customerPackageRepo;
    }

    public PaymentCategory Category => PaymentCategory.Package;

    public async Task CompleteAsync(Payment payment)
    {
        var package = await _customerPackageRepo.FirstOrDefaultAsync(p => p.Id == payment.CustomerPackageId);
        if (package != null)
        {
            package.CustomerPackageStatus = CustomerPackageStatus.Active;
            await _customerPackageRepo.UpdateAsync(package);
        }
    }
}
