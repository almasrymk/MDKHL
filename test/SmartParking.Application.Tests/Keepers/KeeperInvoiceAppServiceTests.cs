using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Invoices;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Mobile.Invoices;
using SmartParking.Mobile.Keepers;
using SmartParking.Mobile.Vehicles;
using SmartParking.OpeningGate;
using SmartParking.Payments;
using SmartParking.QRCode;
using SmartParking.Results;
using SmartParking.Watchers;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace SmartParking.Keepers;

public class KeeperInvoiceAppServiceTests
{
    private readonly IRepository<Invoice, int> _invoiceRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly IPaymentAppService _paymentAppService;
    private readonly IPaymentWebAppService _paymentWebAppService;
    private readonly IQRCodeAppService _qrCodeAppService;
    private readonly IVehicleAppService _vehicleAppService;
    private readonly IInvoicePaymentProcessorAppService _invoicePaymentProcessorAppService;
    private readonly KeeperInvoiceAppService _service;

    public KeeperInvoiceAppServiceTests()
    {
        _invoiceRepository = Substitute.For<IRepository<Invoice, int>>();
        _garageTransactionRepository = Substitute.For<IRepository<GarageTransaction, int>>();
        _invoiceAppService = Substitute.For<IInvoiceAppService>();
        _paymentAppService = Substitute.For<IPaymentAppService>();
        _paymentWebAppService = Substitute.For<IPaymentWebAppService>();
        _qrCodeAppService = Substitute.For<IQRCodeAppService>();
        _vehicleAppService = Substitute.For<IVehicleAppService>();
        _invoicePaymentProcessorAppService = Substitute.For<IInvoicePaymentProcessorAppService>();

        _service = new KeeperInvoiceAppService(
            _invoiceRepository,
            _garageTransactionRepository,
            _invoiceAppService,
            _paymentAppService,
            _paymentWebAppService,
            _qrCodeAppService,
            _vehicleAppService,
            _invoicePaymentProcessorAppService
        );

        var dataFilter = Substitute.For<IDataFilter>();
        dataFilter.Disable<IMultiTenant>().Returns(Substitute.For<IDisposable>());

        var objectMapper = Substitute.For<IObjectMapper>();
        objectMapper.Map<InvoiceDto, InvoiceMobileDto>(Arg.Any<InvoiceDto>())
            .Returns(callInfo =>
            {
                var src = callInfo.Arg<InvoiceDto>();
                return new InvoiceMobileDto
                {
                    Id = src.Id,
                    InvoicePrice = src.InvoicePrice
                };
            });

        var services = new ServiceCollection();
        services.AddSingleton(dataFilter);
        services.AddSingleton(objectMapper);
        _service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());

        _vehicleAppService.GetVehicleByPlateNumberAsync(Arg.Any<string?>())
            .Returns(Task.FromResult(new SmartParking.Vehicles.VehicleDto()));
    }

    [Fact]
    public async Task CheckoutAsync_WhenProcessorReturnsAlreadyPaid_ReturnsInvoiceIsAlreadyPaidWithoutException()
    {
        // Arrange
        _invoicePaymentProcessorAppService.InitiatePaymentAsync(Arg.Any<ProcessInvoicePaymentRequestDto>())
            .Returns(Task.FromResult(new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.AlreadyPaid,
                Message = "Invoice is already paid."
            }));

        // Act
        var result = await _service.CheckoutAsync(100, (int)PaymentMethod.Cash);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("InvoiceIsAlreadyPaid");
        await _qrCodeAppService.DidNotReceive().CreateQRCOdeImageAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task CheckoutAsync_WhenProcessorReturnsPaymentInProgress_ReturnsPaymentInProgressWithoutException()
    {
        // Arrange
        _invoicePaymentProcessorAppService.InitiatePaymentAsync(Arg.Any<ProcessInvoicePaymentRequestDto>())
            .Returns(Task.FromResult(new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.PaymentInProgress,
                Message = "Payment in progress."
            }));

        // Act
        var result = await _service.CheckoutAsync(100, (int)PaymentMethod.Cash);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("PaymentInProgress");
        await _qrCodeAppService.DidNotReceive().CreateQRCOdeImageAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task CheckoutAsync_WhenProcessorReturnsFailedWithMessage_ReturnsProvidedErrorMessage()
    {
        // Arrange
        _invoicePaymentProcessorAppService.InitiatePaymentAsync(Arg.Any<ProcessInvoicePaymentRequestDto>())
            .Returns(Task.FromResult(new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.Failed,
                Message = "Custom failure message"
            }));

        // Act
        var result = await _service.CheckoutAsync(100, (int)PaymentMethod.Cash);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Custom failure message");
        await _qrCodeAppService.DidNotReceive().CreateQRCOdeImageAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task CheckoutAsync_WhenProcessorReturnsFailedWithoutMessage_ReturnsPaymentFailedDefault()
    {
        // Arrange
        _invoicePaymentProcessorAppService.InitiatePaymentAsync(Arg.Any<ProcessInvoicePaymentRequestDto>())
            .Returns(Task.FromResult(new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.Failed,
                Message = null
            }));

        // Act
        var result = await _service.CheckoutAsync(100, (int)PaymentMethod.Cash);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("PaymentFailed");
        await _qrCodeAppService.DidNotReceive().CreateQRCOdeImageAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task CheckoutAsync_WhenProcessorReturnsCompleted_ReturnsSuccessWithPaymentInfo()
    {
        // Arrange
        _invoicePaymentProcessorAppService.InitiatePaymentAsync(Arg.Any<ProcessInvoicePaymentRequestDto>())
            .Returns(Task.FromResult(new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.Completed,
                PaymentId = Guid.NewGuid(),
                PaymentNumber = "PAY-12345",
                CheckoutData = new PaymentCheckoutDto { QrCodeInvoice = "qr-content" }
            }));

        _qrCodeAppService.CreateQRCOdeImageAsync(Arg.Any<string?>())
            .Returns(Task.FromResult(Result<string>.Success("data:image/png;base64,...")));

        _invoiceAppService.GetAsync(100)
            .Returns(Task.FromResult(new InvoiceDto
            {
                Id = 100,
                InvoicePrice = 50,
                InvoiceType = InvoiceType.Paid.ToString()
            }));

        // Act
        var result = await _service.CheckoutAsync(100, (int)PaymentMethod.Cash);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.PaymentMethod.ShouldBe(PaymentMethod.Cash.ToString());
        result.Value.InvoiceNumber.ShouldBe("PAY-12345");
    }

    [Fact]
    public async Task CheckoutAsync_WhenProcessorReturnsStarted_ReturnsSuccessWithPaymentInfo()
    {
        // Arrange
        _invoicePaymentProcessorAppService.InitiatePaymentAsync(Arg.Any<ProcessInvoicePaymentRequestDto>())
            .Returns(Task.FromResult(new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.Started,
                PaymentId = Guid.NewGuid(),
                PaymentNumber = "PAY-POS-999",
                CheckoutData = new PaymentCheckoutDto { QrCodeInvoice = "pos-qr" }
            }));

        _qrCodeAppService.CreateQRCOdeImageAsync(Arg.Any<string?>())
            .Returns(Task.FromResult(Result<string>.Success("data:image/png;base64,pos...")));

        _invoiceAppService.GetAsync(100)
            .Returns(Task.FromResult(new InvoiceDto
            {
                Id = 100,
                InvoicePrice = 75,
                InvoiceType = InvoiceType.UnPaid.ToString()
            }));

        // Act
        var result = await _service.CheckoutAsync(100, (int)PaymentMethod.POS);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.PaymentMethod.ShouldBe(PaymentMethod.POS.ToString());
        result.Value.InvoiceNumber.ShouldBe("PAY-POS-999");
    }
}
