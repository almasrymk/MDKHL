using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Mobile.Invoices;
using SmartParking.Results;
using SmartParking.Shifts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.TenantManagement;

namespace SmartParking.Mobile.Keepers;

public interface IKeeperAppService
{
    /// <summary>
    /// Logs in a user with the provided input.
    /// </summary>
    /// <param name="input">The login input containing username and password.</param>
    /// <returns>A <see cref="LoginResponseDto"/> containing the result of the login attempt.</returns>
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto input);
    /// <summary>
    /// Retrieves a list of all invoices by organization id asynchronously.
    /// </summary>
    /// <param name="organizationId">id of specific organization</param>
    /// <returns>list of InvoiceDto</returns>
    Task<Result<PagedResultDto<InvoiceDto>>> GetAllInvoicesByOrganizationIdAsync(int organizationId, PagedAndSortedAndSearchResultRequestDto paged);
    /// <summary>
    /// Retrieves Last invoice by organization id, and watcher code asynchronously.
    /// </summary>
    /// <param name="organizationId">id of specific organization</param>
    /// <param name="watcherCode">watcher code</param>
    /// <returns> InvoiceDto</returns>
    Task<Result<InvoiceDto>> GetLastInvoiceByOrganizationIdAndWatcherCodeAsync(int organizationId, string watcherCode);
    /// <summary>
    /// Retrieves a list of all garage transaction with no plate number  asynchronously.
    /// </summary>
    /// <returns>list of GarageTransactionDto</returns>
	Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumberAsync(int organizationId, Guid userId);
    /// <summary>
	/// Retrieves a list of all garage transaction  asynchronously.
	/// </summary>
	/// <returns>list of GarageTransactionDto</returns>
	Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllGarageTransactionsAsync(int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged);
    /// <summary>
    /// Update plate number of specific transaction.
    /// </summary>
    /// <param name="garageTransactionId">id of specific garageTransaction</param>
    /// <param name="plateNumber">plate number</param>
    Task<Result> CorrectTransactionByPlateNumberAsync(int garageTransactionId, string plateNumber);
    /// <summary>
	/// Retrieves a list of all existing transaction  asynchronously.
	/// </summary>
	/// <returns>list of GarageTransactionDto</returns>
	Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllExistingTransactionsAsync(int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged);
    /// <summary>
    /// Retrieves the tenant associated with the given phone number.
    /// </summary>
    /// <param name="phoneNum">The phone number to search for the tenant.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="Tenant"/> for the provided phone number.</returns>
    Task<Tenant> GetTenantByPhoneNumberDtoAsync(string phoneNum);
    /// <summary>
    /// Initiates the checkout process for a specific invoice, including the application of a discount value.
    /// </summary>
    /// <param name="invoiceId">The unique identifier of the invoice to be processed.</param>
    /// <param name="DiscountValue">The discount value to be applied to the invoice (optional, defaults to 0).</param>
    /// <returns>A task representing the asynchronous operation, containing the result of type <see cref="InvoiceMobileDto"/>.</returns>
    Task<Result<InvoiceMobileDto>> CheckoutAsync(int invoiceId, int paymentMethodType, decimal DiscountValue = default);
    /// <summary>
    /// Processes a cash payment for the customer request for a specific invoice, including the option for a discount value.
    /// </summary>
    /// <param name="invoiceId">The unique identifier of the invoice to be paid.</param>
    /// <param name="PaymentType">The type of payment being made (e.g., cash, credit, etc.).</param>
    /// <param name="DiscountValue">The discount value to be applied to the invoice (optional, defaults to 0).</param>
    /// <returns>A task representing the asynchronous operation, containing the result of type <see cref="InvoiceMobileDto"/> for the payment request.</returns>
    Task<Result<InvoiceMobileDto>> PayCashForCustomerRequestAsync(int invoiceId, string PaymentType, decimal DiscountValue = default);
    /// <summary>
    /// Retrieves all invoices associated with a specific organization that have been paid.
    /// </summary>
    /// <param name="organizationId">The unique identifier of the organization.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of type <see cref="List{InvoiceDto}"/> for all paid invoices in the organization.</returns>
    Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationIdPaidAsync(int organizationId);
    /// <summary>
    /// Retrieves the payment QR code associated with the specified invoice.
    /// </summary>
    /// <param name="InvoiceId">The unique identifier of the invoice.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing a <see cref="Result{String}"/> with the QR code string if successful.
    /// If not found or an error occurs, returns an appropriate error Result.
    /// </returns>
    Task<Result<string>> GetPaymentQrCodeByInvoiceId(int InvoiceId);
    /// <summary>
    /// Retrieves shift watcher actions.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation, containing a <see cref="Result{ICollection{ShiftWatcherActionDto}}"/>
    /// with the collection of shift watcher actions. If none are found, returns a successful Result with an empty collection.
    /// </returns>
    Task<Result<ICollection<ShiftWatcherActionDto>>> GetShiftWatcherActionAsync();
    /// <summary>
    /// Checks if the current user has an active shift zone action (gate).
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation, containing a <see cref="Result{Boolean}"/> indicating whether the current user has an active shift zone action.
    /// </returns>
    Task<Result<bool>> HasActiveShiftZoneActionAsync();

    /// <summary>
    /// Retrieves the latest unlinked transaction for a selected watcher/camera device.
    /// </summary>
    /// <param name="organizationId">The organization ID.</param>
    /// <param name="watcherCode">The watcher/device code.</param>
    /// <returns>A Result containing the UnlinkedTransactionViewDto.</returns>
    Task<Result<SmartParking.Mobile.Keepers.Dtos.UnlinkedTransactionViewDto>> GetLatestUnlinkedTransactionByWatcherCodeAsync(int organizationId, string watcherCode);

    /// <summary>
    /// Converts a pending unlinked transaction into an active ENTRY GarageTransaction.
    /// </summary>
    /// <param name="input">The recovery input data.</param>
    /// <returns>A Result containing the created GarageTransactionDto.</returns>
    Task<Result<GarageTransactionDto>> SendUnlinkedTransactionAsEntryAsync(SmartParking.Mobile.Keepers.Dtos.SendUnlinkedAsEntryDto input);
}