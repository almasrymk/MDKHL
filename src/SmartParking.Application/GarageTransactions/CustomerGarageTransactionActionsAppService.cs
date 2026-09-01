using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions.Factory;
using SmartParking.Invoices;
using SmartParking.Localization;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.MultiTenancy;

namespace SmartParking.GarageTransactions;

public class CustomerGarageTransactionActionsAppService : SmartParkingAppService, ICustomerGarageTransactionActionsAppService
{
    private readonly IGarageTransactionsActionsFactoryAppService _actionsFactoryService;
    private readonly ICustomerMobileTransactionsAppService _customerMobileAppService;

    public CustomerGarageTransactionActionsAppService(ICustomerMobileTransactionsAppService customerMobileAppService, IGarageTransactionsActionsFactoryAppService actionsFactoryService)
    {
        _customerMobileAppService = customerMobileAppService;
        _actionsFactoryService = actionsFactoryService;
    }

    [RemoteService(false)]
    public async Task<Result<CreateUpdateInvoiceDto>> VehiclePreCheckoutAsync(string input)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Result<CreateUpdateInvoiceDto>.Error("Vehicle not found.");
            }

            var invoiceSearchDto = new InvoiceSearchDto();

            if (!string.IsNullOrEmpty(input) && char.IsLetter(input.LastOrDefault()))
            {
                invoiceSearchDto.TicketId = string.Empty;
                invoiceSearchDto.PlateNumber = input;
            }
            else
            {
                invoiceSearchDto.TicketId = input ?? string.Empty;
                invoiceSearchDto.PlateNumber = string.Empty;
            }

            var transaction = await _actionsFactoryService.GetGarageTransaction(invoiceSearchDto);
            if (transaction == null)
            {
                return Result<CreateUpdateInvoiceDto>.Error("Vehicle not found.");
            }

            if (transaction.IsIn == false)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("CarAlreadyExit"));
            }

            return await _customerMobileAppService.PreCheckout(transaction.Id);
        }
    }

    [RemoteService(false)]
    public async Task<Result<InvoiceDto>> VehicleCheckoutAsync(string input)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Result<InvoiceDto>.Error("Vehicle not found.");
            }

            var invoiceSearchDto = new InvoiceSearchDto();

            if (!string.IsNullOrEmpty(input) && char.IsLetter(input.LastOrDefault()))
            {
                invoiceSearchDto.TicketId = string.Empty;
                invoiceSearchDto.PlateNumber = input;
            }
            else
            {
                invoiceSearchDto.TicketId = input ?? string.Empty;
                invoiceSearchDto.PlateNumber = string.Empty;
            }

            var transaction = await _actionsFactoryService.GetGarageTransaction(invoiceSearchDto);
            if (transaction == null)
            {
                return Result<InvoiceDto>.Error("Vehicle not found.");
            }

            if (transaction.CheckoutMobile is not null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("CarAlreadyExit"));
            }

            return await _customerMobileAppService.Checkout(transaction.Id);
        }
    }
}