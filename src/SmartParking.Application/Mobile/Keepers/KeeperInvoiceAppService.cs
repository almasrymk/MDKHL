using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Invoices;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Mobile.Invoices;
using SmartParking.Mobile.Vehicles;
using SmartParking.Payments;
using SmartParking.QRCode;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Mobile.Keepers;

[RemoteService(isEnabled: false)]
public class KeeperInvoiceAppService : SmartParkingAppService, IKeeperInvoiceAppService
{
    private readonly IRepository<Invoice, int> _invoiceRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly IPaymentAppService _paymentAppService;
    private readonly IPaymentWebAppService _paymentWebAppService;
    private readonly IQRCodeAppService _qrCodeAppService;
    private readonly IVehicleAppService _vehicleAppService;
    private const string NotFoundMessage = "Not found";

    public KeeperInvoiceAppService(
        IRepository<Invoice, int> invoiceRepository,
        IRepository<GarageTransaction, int> garageTransactionRepository,
        IInvoiceAppService invoiceAppService,
        IPaymentAppService paymentAppService,
        IPaymentWebAppService paymentWebAppService,
        IQRCodeAppService qrCodeAppService,
        IVehicleAppService vehicleAppService)
    {
        _invoiceRepository = invoiceRepository;
        _garageTransactionRepository = garageTransactionRepository;
        _invoiceAppService = invoiceAppService;
        _paymentAppService = paymentAppService;
        _paymentWebAppService = paymentWebAppService;
        _qrCodeAppService = qrCodeAppService;
        _vehicleAppService = vehicleAppService;
    }

    /// <inheritdoc/>
    public async Task<Result<PagedResultDto<InvoiceDto>>> GetAllInvoicesByOrganizationIdAsync(int organizationId, PagedAndSortedAndSearchResultRequestDto paged)
    {
        var query = await _invoiceRepository.GetQueryableAsync();
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = query.OrderByDescending(inv => inv.InvoiceTime).Skip(paged.SkipCount).Take(paged.MaxResultCount);
        var invoices = await query.ToListAsync();
        var finalPaged = new PagedResultDto<InvoiceDto>(
            totalCount,
            ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(invoices)
        );
        return Result<PagedResultDto<InvoiceDto>>.Success(finalPaged);
    }

    /// <inheritdoc/>
    public async Task<Result<InvoiceDto>> GetLastInvoiceByOrganizationIdAndWatcherCodeAsync(int organizationId, string watcherCode)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var invoiceQuery = await _invoiceRepository.GetQueryableAsync();
            var garageTransactionQuery = await _garageTransactionRepository.GetQueryableAsync();

            var result =
                from invoice in invoiceQuery
                join transaction in garageTransactionQuery
                    on invoice.GarageTransactionExitId equals transaction.Id
                where
                    transaction.CheckoutMobile == null &&
                    (transaction.Camera.DeviceId == watcherCode || transaction.LaneNO == watcherCode)
                    && invoice.OrganizationId == organizationId
                    && invoice.InvoicePrice > 0
                    && invoice.InvoiceType == InvoiceType.UnPaid
                    && (transaction.CameraId != null || transaction.DispenserId != null)
                orderby invoice.CreationTime descending
                select invoice;

            var lastInvoice = await AsyncExecuter.FirstOrDefaultAsync(result);

            if (lastInvoice == null)
                return Result<InvoiceDto>.Success(new InvoiceDto());

            return Result<InvoiceDto>.Success(ObjectMapper.Map<Invoice, InvoiceDto>(lastInvoice));
        }
    }

    /// <inheritdoc/>
    public async Task<Result<InvoiceMobileDto>> CheckoutAsync(int invoiceId, int paymentMethodType, decimal discountValue = default)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var invoice = await _invoiceAppService.GetAsync(invoiceId);
            if (CheckIfInvoiceIsPaid(invoice))
            {
                return Result.Error("InvoiceIsAlreadyPaid");
            }
            if (discountValue != default)
            {
                try
                {
                    await _invoiceAppService.UpdateDiscountForInvoice(invoiceId, discountValue);
                }
                catch (Exception ex)
                {
                    return Result.Error(ex.Message);
                }
            }
            var result = await RequestToPaymentCheckout(invoice, paymentMethodType, discountValue);
            var image = await _qrCodeAppService.CreateQRCOdeImageAsync(result is null ? " " : result.QrCodeInvoice);
            var mobileInvoice = await ProcessingInvoiceMobileDto(discountValue, invoiceId, image);
            mobileInvoice.Value.PaymentMethod = result?.PaymentMethod.ToString();
            mobileInvoice.Value.InvoiceNumber = result?.PaymentNumber;
            return mobileInvoice;
        }
    }

    /// <inheritdoc/>
    public async Task<Result<InvoiceMobileDto>> PayCashForCustomerRequestAsync(int invoiceId, string paymentType, decimal discountValue = default)
    {
        return await CheckoutAsync(invoiceId, (int)PaymentMethod.Cash, discountValue);
    }

    /// <inheritdoc/>
    public async Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationIdPaidAsync(int organizationId)
    {
        var invoiceQueryable = await _invoiceRepository.GetQueryableAsync();
        var listInvoice = await AsyncExecuter.ToListAsync(
            invoiceQueryable
                .Where(inv => inv.OrganizationId == organizationId && inv.InvoiceType == InvoiceType.Paid)
                .OrderByDescending(inv => inv.InvoiceTime)
                .Take(100));
        return ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(listInvoice);
    }

    /// <inheritdoc/>
    public async Task<Result<string>> GetPaymentQrCodeByInvoiceId(int invoiceId)
    {
        var payment = await _paymentWebAppService.GetPaymentForInvoice(invoiceId);
        if (payment == null)
        {
            return Result.Error(NotFoundMessage);
        }
        return payment.QrCodeInvoice;
    }

    #region Private Helpers

    private static bool CheckIfInvoiceIsPaid(InvoiceDto invoice)
    {
        return invoice.InvoiceType == InvoiceType.Paid.ToString();
    }

    private async Task<Result<InvoiceMobileDto>> ProcessingInvoiceMobileDto(decimal discountValue, int invoiceId, Result<string> image)
    {
        var invoice = await _invoiceAppService.GetAsync(invoiceId);
        var invoiceDto = ObjectMapper.Map<InvoiceDto, InvoiceMobileDto>(invoice);
        invoiceDto.QrCodeInvoice = image.Value;
        invoiceDto.Vehicle = await _vehicleAppService.GetVehicleByPlateNumberAsync(invoice.PlateNumber);
        invoiceDto.TotalPriceWithVatAndDiscount = PaymentUtilityCalculationts.GetPriceForPayment(invoice.InvoicePrice, discountValue);
        invoiceDto.ExitTransactionImage = invoiceDto.ExitTransactionImage;
        return invoiceDto;
    }

    private async Task<PaymentCheckoutDto?> RequestToPaymentCheckout(InvoiceDto invoice, int paymentMethodType, decimal discountValue)
    {
        if (!Enum.IsDefined(typeof(PaymentMethod), paymentMethodType))
        {
            throw new ArgumentException($"Invalid payment method type: {paymentMethodType}");
        }
        var paymentMethod = (PaymentMethod)paymentMethodType;
        var paymentCheckout = new PaymentCheckoutRequestDto
        {
            PaymentCategory = PaymentCategory.PaymentParking,
            PaymentMethod = paymentMethod,
            Amount = PaymentUtilityCalculationts.GetPriceForPayment(invoice.InvoicePrice, discountValue),
            CustomerId = invoice.CustomerId,
            OrganizationId = invoice.OrganizationId,
            InvoiceId = invoice.Id,
            PaymentName = paymentMethod.ToString(),
            TenantId = invoice.TenantId.Value
        };
        return await _paymentAppService.CreateCheckoutInvoicePaymentWebAsync(paymentCheckout);
    }

    #endregion
}
