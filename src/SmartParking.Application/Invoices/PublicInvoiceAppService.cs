using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Invoices;
using SmartParking.Enums;
using SmartParking.Localization;
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
    private readonly ICurrentTenant _currentTenant;

    public PublicInvoiceAppService(
        IRepository<Invoice, int> invoiceRepository,
        IRepository<GarageTransaction, int> garageTransactionRepository,
        IRepository<Dispenser, int> dispenserRepository,
        ICurrentTenant currentTenant)
    {
        _invoiceRepository = invoiceRepository;
        _garageTransactionRepository = garageTransactionRepository;
        _dispenserRepository = dispenserRepository;
        _currentTenant = currentTenant;
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
}
