using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.GarageTransactions.Factory;
using SmartParking.Localization;
using SmartParking.MobileInvoices;
using SmartParking.Payments;
using SmartParking.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Invoices;

[RemoteService(IsEnabled = true)]
[AllowAnonymous]
public class PublicInvoiceAppService : ApplicationService, IPublicInvoiceAppService
{
    private readonly IRepository<Invoice, int> _invoiceRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;
    private readonly IRepository<Dispenser, int> _dispenserRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IPaymentWebAppService _paymentWebAppService;
    private readonly IGarageTransactionsActionsFactoryAppService _garageTransactionsActionsFactoryAppService;
    public PublicInvoiceAppService(
        IRepository<Invoice, int> invoiceRepository,
        IRepository<GarageTransaction, int> garageTransactionRepository,
        IRepository<Dispenser, int> dispenserRepository,
        IPaymentWebAppService paymentWebAppService,
        IRepository<Payment, Guid> paymentRepository,
        IGarageTransactionsActionsFactoryAppService garageTransactionsActionsFactoryAppService)
    {
        _invoiceRepository = invoiceRepository;
        _garageTransactionRepository = garageTransactionRepository;
        _dispenserRepository = dispenserRepository;
        _paymentWebAppService = paymentWebAppService;
        _paymentRepository = paymentRepository;
        _garageTransactionsActionsFactoryAppService = garageTransactionsActionsFactoryAppService;
    }

    public async Task CancelInvoiceAsync(int invoiceId, InvoiceSearchDto searchDto)
    {
        var invoice = await _invoiceRepository.FirstOrDefaultAsync(inv => inv.Id == invoiceId);

        if (invoice == null)
            throw new UserFriendlyException("Invoice not found.");

        if (invoice.InvoiceType == InvoiceType.Paid)
            throw new UserFriendlyException("Invoice is already paid.");

        var exitTransactionId = invoice.GarageTransactionExitId;

        // Delete payment
        var payment = await _paymentWebAppService.GetPaymentForInvoice(invoiceId);
        if (payment != null)
        {
            await _paymentRepository.DeleteAsync(payment.Id);
        }

        await _invoiceRepository.DeleteAsync(invoice.Id);
        // Delete exit transaction
        if (exitTransactionId > 0)
        {
            await _garageTransactionRepository.DeleteAsync(exitTransactionId);
        }

        // Restore entry transaction
        var entryTransaction =
            await _garageTransactionsActionsFactoryAppService.GetGarageTransaction(searchDto);
        await _garageTransactionsActionsFactoryAppService.UnMappingEntryTransactionForDeletedExit(entryTransaction);

       
    }

    public async Task<InvoiceDto?> GetLatestInvoiceForOrganizationByWatcher(string watcherCode)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var invoiceQuery = await _invoiceRepository.GetQueryableAsync();
            var garageTransactionQuery = await _garageTransactionRepository.GetQueryableAsync();
            var dispenserQuery = await _dispenserRepository.GetQueryableAsync();

            var query =
                from invoice in invoiceQuery
                join gt in garageTransactionQuery
                    on invoice.GarageTransactionExitId equals gt.Id
                join d in dispenserQuery
                    on gt.DispenserId equals d.Id into dispenserJoin
                from d in dispenserJoin.DefaultIfEmpty()
                where invoice.InvoiceType == InvoiceType.UnPaid
                      && invoice.InvoicePrice > 0
                      && (gt.Camera.DeviceId == watcherCode || d.DeviceId == watcherCode)
                orderby invoice.CreationTime descending
                select invoice;

            var invoiceResult = await query.FirstOrDefaultAsync();

            if (invoiceResult == null)
            {
                throw new BusinessException(
                    LocalizationHelper.GetLocalizedString("VehicleNotFoundAtGate")
                );
            }

            return ObjectMapper.Map<Invoice, InvoiceDto>(invoiceResult);
        }
    }

    private static CreateUpdateGarageTransactionDto BuildRevertedTransaction(
    GarageTransactionDto dto)
    {
        return new CreateUpdateGarageTransactionDto
        {
            CameraId = dto.CameraId,
            PlateNumber = dto.PlateNumber,
            SnapTime = dto.SnapTime,
            AccurateTime = dto.AccurateTime,
            CheckoutMobile = null,
            OrganizationID = dto.OrganizationID,
            GarageId = dto.GarageId,
            VehiclePicture = dto.VehiclePicture,
            CameraType = dto.CameraType,
            TenantId = dto.TenantId,
            IsIn = true,
            IsMapped = dto.IsMapped,
            RemainingTime = dto.RemainingTime,
            IsPending = null,
            CanCheckout = true,
            CustomerCategory = dto.CustomerCategory,
            DispenserId = dto.DispenserId,
            LaneNO = dto.LaneNO,
            TicketId = dto.TicketId
        };
    }
}
