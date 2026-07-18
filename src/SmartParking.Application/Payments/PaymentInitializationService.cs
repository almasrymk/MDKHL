using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Payments.PaymentIntegrations;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Payments;

public class PaymentInitializationService : IPaymentInitializationService, ITransientDependency
{
    private readonly IEnumerable<IPaymentInitializationStrategy> _strategies;
    private readonly IPaymentGatewayIntegrationService _gatewayService;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentInitializationService> _logger;

    public PaymentInitializationService(
        IEnumerable<IPaymentInitializationStrategy> strategies,
        IPaymentGatewayIntegrationService gatewayService,
        IRepository<Payment, Guid> paymentRepository,
        IServiceProvider serviceProvider,
        ILogger<PaymentInitializationService> logger)
    {
        _strategies = strategies;
        _gatewayService = gatewayService;
        _paymentRepository = paymentRepository;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Result<MobilePaymentSessionDto>> InitializeAsync(PaymentCategory category, int sourceId, PaymentMethod paymentMethod)
    {
        var strategy = _strategies.FirstOrDefault(s => s.Category == category);
        if (strategy == null)
        {
            _logger.LogError("PaymentInitializationService: No strategy found for category {Category}", category);
            return Result<MobilePaymentSessionDto>.Error($"No payment strategy found for {category}");
        }

        var buildResult = await strategy.BuildAsync(sourceId, paymentMethod);
        if (!buildResult.IsSuccess)
        {
            return Result<MobilePaymentSessionDto>.Error(buildResult.Errors.FirstOrDefault() ?? "Failed to build payment request");
        }

        var context = buildResult.Value;
        var request = context.CheckoutRequest;

        request.CallbackUrl = PaymentCallbackHelper.ResolveCallbackUrl(_serviceProvider, GetBaseUri(), _gatewayService.Gateway);

        var initiateResult = await _gatewayService.InitiateAsync(request);
        request.BaseURI = GetBaseUri();
        
        var sessionDto = initiateResult.ToMobilePaymentSessionDto();

        var payment = await _paymentRepository.FirstOrDefaultAsync(p => p.PaymentNumber == sessionDto.MerchantReferenceId);
        if (payment != null)
        {
            sessionDto.PaymentId = payment.Id;
        }

        _logger.LogInformation("PaymentInitializationService: Session initialized for {Category} SourceId {SourceId}, PaymentNumber {PaymentNumber}", category, sourceId, sessionDto.MerchantReferenceId);
        return Result<MobilePaymentSessionDto>.Success(sessionDto, "Payment session initialized successfully");
    }

    private string GetBaseUri()
    {
        var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
        var request = httpContextAccessor?.HttpContext?.Request;

        if (request != null)
        {
            return $"{request.Scheme}://{request.Host}{request.PathBase}".TrimEnd('/');
        }
        
        var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
        return configuration["App:SelfUrl"]?.TrimEnd('/') ?? string.Empty;
    }
}
