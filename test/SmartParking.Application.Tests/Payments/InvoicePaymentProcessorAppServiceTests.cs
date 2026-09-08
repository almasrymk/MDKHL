using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Enums.PosTerminal;
using SmartParking.Localization;
using SmartParking.Payments.PosTerminal;
using SmartParking.Utilities;
using SmartParking.Validations;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartParking.Payments;

public class InvoicePaymentProcessorAppServiceTests
{
    private readonly IAbpDistributedLock _distributedLock;
    private readonly IAbpDistributedLockHandle _lockHandle;
    private readonly IRepository<Invoice, int> _invoiceRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IPaymentAppService _paymentAppService;
    private readonly InvoicePaymentProcessorAppService _processor;

    public InvoicePaymentProcessorAppServiceTests()
    {
        _distributedLock = Substitute.For<IAbpDistributedLock>();
        _lockHandle = Substitute.For<IAbpDistributedLockHandle>();
        _invoiceRepository = Substitute.For<IRepository<Invoice, int>>();
        _paymentRepository = Substitute.For<IRepository<Payment, Guid>>();
        _paymentAppService = Substitute.For<IPaymentAppService>();

        // Default: lock succeeds
        _distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IAbpDistributedLockHandle?>(_lockHandle));

        // Configure LocalizationHelper for unit test context
        var localizer = Substitute.For<IStringLocalizer<SmartParkingResource>>();
        localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IStringLocalizer<SmartParkingResource>)).Returns(localizer);
        LocalizationHelper.Configure(serviceProvider);

        _processor = new InvoicePaymentProcessorAppService(
            _distributedLock,
            _invoiceRepository,
            _paymentRepository,
            _paymentAppService,
            NullLogger<InvoicePaymentProcessorAppService>.Instance
        );
    }

    private static T SetId<T, TKey>(T entity, TKey id)
    {
        var property = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        property?.SetValue(entity, id);
        return entity;
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenLockHeldByAnotherRequestOrInstance_ReturnsPaymentInProgress()
    {
        // Arrange
        _distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IAbpDistributedLockHandle?>(null));

        var request = new ProcessInvoicePaymentRequestDto
        {
            InvoiceId = 100,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act
        var result = await _processor.InitiatePaymentAsync(request);

        // Assert
        result.Result.ShouldBe(InvoicePaymentResult.PaymentInProgress);
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("Payment operation is currently in progress");
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenInvoiceAlreadyPaid_ReturnsAlreadyPaid()
    {
        // Arrange
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.Paid, InvoicePrice = 50 }, 100);
        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));

        var request = new ProcessInvoicePaymentRequestDto
        {
            InvoiceId = 100,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act
        var result = await _processor.InitiatePaymentAsync(request);

        // Assert
        result.Result.ShouldBe(InvoicePaymentResult.AlreadyPaid);
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("already paid");
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenExistingSuccessfulPaymentExists_ReturnsAlreadyPaid()
    {
        // Arrange
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.UnPaid, InvoicePrice = 50 }, 100);
        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));

        var payments = new List<Payment>
        {
            SetId(new Payment { InvoiceId = 100, PaymentStatus = PaymentStatus.Success, PaymentNumber = "PAY-001" }, Guid.NewGuid())
        };

        _paymentRepository.GetListAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult(payments));

        var request = new ProcessInvoicePaymentRequestDto
        {
            InvoiceId = 100,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act
        var result = await _processor.InitiatePaymentAsync(request);

        // Assert
        result.Result.ShouldBe(InvoicePaymentResult.AlreadyPaid);
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("successful payment already exists");
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenActivePendingPaymentExists_ReturnsPaymentInProgress()
    {
        // Arrange
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.UnPaid, InvoicePrice = 50 }, 100);
        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));

        var pendingId = Guid.NewGuid();
        var payments = new List<Payment>
        {
            SetId(new Payment { InvoiceId = 100, PaymentStatus = PaymentStatus.Pending, PaymentNumber = "PAY-PENDING" }, pendingId)
        };

        _paymentRepository.GetListAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult(payments));

        var request = new ProcessInvoicePaymentRequestDto
        {
            InvoiceId = 100,
            PaymentMethod = PaymentMethod.POS
        };

        // Act
        var result = await _processor.InitiatePaymentAsync(request);

        // Assert
        result.Result.ShouldBe(InvoicePaymentResult.PaymentInProgress);
        result.PaymentId.ShouldBe(pendingId);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenCash_CreatesSuccessfulPaymentAndCompletes()
    {
        // Arrange
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.UnPaid, InvoicePrice = 50, OrganizationId = 1 }, 100);
        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));

        var payments = new List<Payment>();
        _paymentRepository.GetListAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult(payments));

        var expectedPaymentId = Guid.NewGuid();
        _paymentAppService.CreateCheckoutInvoicePaymentWebAsync(Arg.Any<PaymentCheckoutRequestDto>())
            .Returns(Task.FromResult<PaymentCheckoutDto?>(new PaymentCheckoutDto
            {
                PaymentId = expectedPaymentId,
                PaymentNumber = "PAY-SUCCESS",
                PaymentStatus = PaymentStatus.Success
            }));

        var request = new ProcessInvoicePaymentRequestDto
        {
            InvoiceId = 100,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act
        var result = await _processor.InitiatePaymentAsync(request);

        // Assert
        result.Result.ShouldBe(InvoicePaymentResult.Completed);
        result.PaymentId.ShouldBe(expectedPaymentId);
        result.PaymentNumber.ShouldBe("PAY-SUCCESS");
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenPos_CreatesPendingPaymentAndReturnsStarted()
    {
        // Arrange
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.UnPaid, InvoicePrice = 50, OrganizationId = 1 }, 100);
        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));

        var payments = new List<Payment>();
        _paymentRepository.GetListAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult(payments));

        var expectedPaymentId = Guid.NewGuid();
        _paymentAppService.CreatePendingPosPaymentAsync(Arg.Any<PaymentCheckoutRequestDto>(), Arg.Any<string?>())
            .Returns(Task.FromResult(new PaymentCheckoutDto
            {
                PaymentId = expectedPaymentId,
                PaymentNumber = "PAY-PENDING-01",
                PaymentStatus = PaymentStatus.Pending
            }));

        var request = new ProcessInvoicePaymentRequestDto
        {
            InvoiceId = 100,
            PaymentMethod = PaymentMethod.POS
        };

        // Act
        var result = await _processor.InitiatePaymentAsync(request);

        // Assert
        result.Result.ShouldBe(InvoicePaymentResult.Started);
        result.PaymentId.ShouldBe(expectedPaymentId);
        result.PaymentNumber.ShouldBe("PAY-PENDING-01");
    }

    [Fact]
    public async Task CompletePosPaymentAsync_WhenInvoiceAlreadyPaidAndPosApproved_CallsRefundAndReturnsAlreadyPaid()
    {
        // Arrange
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.Paid, InvoicePrice = 50 }, 100);
        var paymentId = Guid.NewGuid();
        var payment = SetId(new Payment { InvoiceId = 100, PaymentStatus = PaymentStatus.Pending }, paymentId);

        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));
        _paymentRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult<Payment?>(payment));

        var request = new CompletePosPaymentRequestDto
        {
            InvoiceId = 100,
            PaymentId = paymentId,
            SamaCode = "000" // Approved
        };

        // Act
        var result = await _processor.CompletePosPaymentAsync(request);

        // Assert
        result.Result.ShouldBe(InvoicePaymentResult.AlreadyPaid);
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("refunded");
        await _paymentAppService.Received(1).RefundPosPaymentAsync(Arg.Is<CompletePosTerminalRefundDto>(r => r.PaymentId == paymentId));
    }

    [Fact]
    public async Task CompletePosPaymentAsync_WhenPosApproved_CompletesPayment()
    {
        // Arrange
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.UnPaid, InvoicePrice = 50 }, 100);
        var paymentId = Guid.NewGuid();
        var payment = SetId(new Payment { InvoiceId = 100, PaymentStatus = PaymentStatus.Pending }, paymentId);

        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));
        _paymentRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult<Payment?>(payment));

        _paymentAppService.CompletePosTerminalPaymentAsync(Arg.Any<CompletePosTerminalPaymentDto>())
            .Returns(Task.FromResult(new PaymentDto
            {
                Id = paymentId,
                PaymentNumber = "PAY-COMPLETED",
                PaymentStatus = PaymentStatus.Success
            }));

        var request = new CompletePosPaymentRequestDto
        {
            InvoiceId = 100,
            PaymentId = paymentId,
            SamaCode = "000"
        };

        // Act
        var result = await _processor.CompletePosPaymentAsync(request);

        // Assert
        result.Result.ShouldBe(InvoicePaymentResult.Completed);
        result.PaymentId.ShouldBe(paymentId);
        result.PaymentNumber.ShouldBe("PAY-COMPLETED");
    }

    [Fact]
    public async Task CompletePosPaymentAsync_WhenPosDeclined_ReturnsFailed()
    {
        // Arrange
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.UnPaid, InvoicePrice = 50 }, 100);
        var paymentId = Guid.NewGuid();
        var payment = SetId(new Payment { InvoiceId = 100, PaymentStatus = PaymentStatus.Pending }, paymentId);

        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));
        _paymentRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult<Payment?>(payment));

        _paymentAppService.CompletePosTerminalPaymentAsync(Arg.Any<CompletePosTerminalPaymentDto>())
            .Returns(Task.FromResult(new PaymentDto
            {
                Id = paymentId,
                PaymentStatus = PaymentStatus.Cancelled
            }));

        var request = new CompletePosPaymentRequestDto
        {
            InvoiceId = 100,
            PaymentId = paymentId,
            SamaCode = "100" // Declined
        };

        // Act
        var result = await _processor.CompletePosPaymentAsync(request);

        // Assert
        result.Result.ShouldBe(InvoicePaymentResult.Failed);
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("declined or cancelled");
    }

    [Fact]
    public async Task CompletePosPaymentAsync_WhenLockHeldByAnotherRequest_ReturnsPaymentInProgress()
    {
        // Arrange
        _distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IAbpDistributedLockHandle?>(null));

        var request = new CompletePosPaymentRequestDto
        {
            InvoiceId = 100,
            PaymentId = Guid.NewGuid(),
            SamaCode = "000"
        };

        // Act
        var result = await _processor.CompletePosPaymentAsync(request);

        // Assert
        result.Result.ShouldBe(InvoicePaymentResult.PaymentInProgress);
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("Concurrent operation in progress");
    }

    [Fact]
    public async Task InitiatePaymentAsync_AfterCleanupJobDeletedStalePendingPayment_SucceedsWithNewPayment()
    {
        // Arrange: Invoice was restored to UnPaid and stale payment was deleted by background job
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.UnPaid, InvoicePrice = 50, OrganizationId = 1 }, 100);
        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));

        // Background worker deleted stale pending payments; repository returns empty list
        var payments = new List<Payment>();
        _paymentRepository.GetListAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult(payments));

        var newPaymentId = Guid.NewGuid();
        _paymentAppService.CreateCheckoutInvoicePaymentWebAsync(Arg.Any<PaymentCheckoutRequestDto>())
            .Returns(Task.FromResult<PaymentCheckoutDto?>(new PaymentCheckoutDto
            {
                PaymentId = newPaymentId,
                PaymentNumber = "PAY-AFTER-CLEANUP",
                PaymentStatus = PaymentStatus.Success
            }));

        var request = new ProcessInvoicePaymentRequestDto
        {
            InvoiceId = 100,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act
        var result = await _processor.InitiatePaymentAsync(request);

        // Assert: New payment succeeds normally
        result.Result.ShouldBe(InvoicePaymentResult.Completed);
        result.PaymentId.ShouldBe(newPaymentId);
        result.PaymentNumber.ShouldBe("PAY-AFTER-CLEANUP");
    }

    [Fact]
    public async Task InitiatePaymentAsync_WithValidDiscount_UpdatesInvoiceDiscountAndCalculatesCorrectAmount()
    {
        // Arrange
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.UnPaid, InvoicePrice = 100, OrganizationId = 1, Discount = 0 }, 100);
        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));
        _paymentRepository.GetListAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult(new List<Payment>()));

        PaymentCheckoutRequestDto? capturedRequest = null;
        var paymentId = Guid.NewGuid();
        _paymentAppService.CreateCheckoutInvoicePaymentWebAsync(Arg.Do<PaymentCheckoutRequestDto>(r => capturedRequest = r))
            .Returns(Task.FromResult<PaymentCheckoutDto?>(new PaymentCheckoutDto
            {
                PaymentId = paymentId,
                PaymentNumber = "PAY-DISCOUNT-1",
                PaymentStatus = PaymentStatus.Success
            }));

        var request = new ProcessInvoicePaymentRequestDto
        {
            InvoiceId = 100,
            PaymentMethod = PaymentMethod.Cash,
            Discount = 20
        };

        // Act
        var result = await _processor.InitiatePaymentAsync(request);

        // Assert: Invoice not prematurely Paid; discount updated; amount calculated correctly
        result.Result.ShouldBe(InvoicePaymentResult.Completed);
        result.PaymentId.ShouldBe(paymentId);
        capturedRequest.ShouldNotBeNull();
        capturedRequest.Amount.ShouldBe(PaymentUtilityCalculationts.GetPriceForPayment(100, 20));
        invoice.Discount.ShouldBe(20);
        await _invoiceRepository.Received(1).UpdateAsync(invoice, autoSave: true);
    }

    [Fact]
    public async Task InitiatePaymentAsync_PreCheckoutWithDiscountForPos_CreatesPendingPaymentAndReturnsStarted()
    {
        // Arrange
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.UnPaid, InvoicePrice = 100, OrganizationId = 1, Discount = 0 }, 100);
        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));
        _paymentRepository.GetListAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult(new List<Payment>()));

        PaymentCheckoutRequestDto? capturedRequest = null;
        var pendingPaymentId = Guid.NewGuid();
        _paymentAppService.CreatePendingPosPaymentAsync(Arg.Do<PaymentCheckoutRequestDto>(r => capturedRequest = r), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new PaymentCheckoutDto
            {
                PaymentId = pendingPaymentId,
                PaymentNumber = "PAY-POS-DISC",
                PaymentStatus = PaymentStatus.Pending
            }));

        var request = new ProcessInvoicePaymentRequestDto
        {
            InvoiceId = 100,
            PaymentMethod = PaymentMethod.POS,
            Discount = 15,
            TerminalId = "TERM-01"
        };

        // Act
        var result = await _processor.InitiatePaymentAsync(request);

        // Assert: Pending payment created, amount has discount deducted, started state returned
        result.Result.ShouldBe(InvoicePaymentResult.Started);
        result.PaymentId.ShouldBe(pendingPaymentId);
        result.PaymentNumber.ShouldBe("PAY-POS-DISC");
        capturedRequest.ShouldNotBeNull();
        capturedRequest.Amount.ShouldBe(PaymentUtilityCalculationts.GetPriceForPayment(100, 15));
        invoice.Discount.ShouldBe(15);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WithNegativeDiscount_ReturnsFailedWithValidationError()
    {
        // Arrange
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.UnPaid, InvoicePrice = 100, OrganizationId = 1 }, 100);
        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));
        _paymentRepository.GetListAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult(new List<Payment>()));

        var request = new ProcessInvoicePaymentRequestDto
        {
            InvoiceId = 100,
            PaymentMethod = PaymentMethod.Cash,
            Discount = -10
        };

        // Act
        var result = await _processor.InitiatePaymentAsync(request);

        // Assert: Fails validation; no payment checkout is created
        result.Result.ShouldBe(InvoicePaymentResult.Failed);
        result.Message.ShouldNotBeNull();
        await _paymentAppService.DidNotReceive().CreateCheckoutInvoicePaymentWebAsync(Arg.Any<PaymentCheckoutRequestDto>());
    }

    [Fact]
    public async Task InitiatePaymentAsync_WithDiscountExceedingInvoicePrice_ReturnsFailedWithValidationError()
    {
        // Arrange
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.UnPaid, InvoicePrice = 100, OrganizationId = 1 }, 100);
        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));
        _paymentRepository.GetListAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult(new List<Payment>()));

        var request = new ProcessInvoicePaymentRequestDto
        {
            InvoiceId = 100,
            PaymentMethod = PaymentMethod.Cash,
            Discount = 150
        };

        // Act
        var result = await _processor.InitiatePaymentAsync(request);

        // Assert: Fails validation; no payment checkout is created
        result.Result.ShouldBe(InvoicePaymentResult.Failed);
        result.Message.ShouldNotBeNull();
        await _paymentAppService.DidNotReceive().CreateCheckoutInvoicePaymentWebAsync(Arg.Any<PaymentCheckoutRequestDto>());
    }

    [Fact]
    public async Task CompletePosPaymentAsync_WhenInvoiceAlreadyPaidConcurrentlyAndPosApproved_CallsRefundAndReturnsAlreadyPaid()
    {
        // Arrange: Invoice was already paid by concurrent Cash or Web request
        var invoice = SetId(new Invoice { InvoiceType = InvoiceType.Paid, InvoicePrice = 100 }, 100);
        var paymentId = Guid.NewGuid();
        var pendingPayment = SetId(new Payment { PaymentStatus = PaymentStatus.Pending, InvoiceId = 100 }, paymentId);

        _invoiceRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Invoice, bool>>>())
            .Returns(Task.FromResult<Invoice?>(invoice));
        _paymentRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Payment, bool>>>())
            .Returns(Task.FromResult<Payment?>(pendingPayment));

        var request = new CompletePosPaymentRequestDto
        {
            InvoiceId = 100,
            PaymentId = paymentId,
            SamaCode = "000",
            Rrn = "123456789",
            AuthCode = "AUTH1"
        };

        // Act
        var result = await _processor.CompletePosPaymentAsync(request);

        // Assert: Compensating refund triggered; returns AlreadyPaid
        result.Result.ShouldBe(InvoicePaymentResult.AlreadyPaid);
        result.Message.ShouldContain("refunded");
        await _paymentAppService.Received(1).RefundPosPaymentAsync(Arg.Is<CompletePosTerminalRefundDto>(r => r.PaymentId == paymentId));
    }
}
