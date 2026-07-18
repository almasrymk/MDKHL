using Microsoft.Extensions.Logging;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Results;
using SmartParking.Utilities;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Payments.Strategies;

public class PackagePaymentInitializationStrategy : IPaymentInitializationStrategy, ITransientDependency
{
    private readonly IRepository<CustomerPackage, int> _customerPackageRepo;
    private readonly ILogger<PackagePaymentInitializationStrategy> _logger;

    public PackagePaymentInitializationStrategy(
        IRepository<CustomerPackage, int> customerPackageRepo,
        ILogger<PackagePaymentInitializationStrategy> logger)
    {
        _customerPackageRepo = customerPackageRepo;
        _logger = logger;
    }

    public PaymentCategory Category => PaymentCategory.Package;

    public async Task<Result<PaymentInitializationContext>> BuildAsync(int sourceId, PaymentMethod paymentMethod)
    {
        var packageResult = await GetPackageByIdAsync(sourceId);
        if (!packageResult.IsSuccess)
        {
            _logger.LogWarning("PackagePaymentInitializationStrategy: {Error}", packageResult.Errors.FirstOrDefault());
            return Result<PaymentInitializationContext>.Error(packageResult.Errors.FirstOrDefault() ?? "Error initializing package payment");
        }

        var customerPackage = packageResult.Value;
        
        var amount = PaymentUtilityCalculationts.GetPriceForPayment(customerPackage.PriceOfPackage ?? 0, customerPackage.Discount);

        var request = customerPackage.ToPaymentCheckoutRequestDto(amount, paymentMethod);

        return Result<PaymentInitializationContext>.Success(new PaymentInitializationContext
        {
            Amount = amount,
            CheckoutRequest = request
        });
    }

    private async Task<Result<CustomerPackage>> GetPackageByIdAsync(int packageId)
    {
        var customerPackage = await _customerPackageRepo.FirstOrDefaultAsync(p => p.Id == packageId);
        if (customerPackage == null)
        {
            return Result<CustomerPackage>.Error("Customer package not found");
        }

        if (!customerPackage.PriceOfPackage.HasValue || customerPackage.PriceOfPackage.Value <= 0)
        {
            return Result<CustomerPackage>.Error("Invalid package price");
        }

        return Result<CustomerPackage>.Success(customerPackage);
    }
}
