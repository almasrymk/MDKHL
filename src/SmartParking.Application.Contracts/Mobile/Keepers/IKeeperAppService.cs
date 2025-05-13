using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Mobile.Invoices;
using SmartParking.Payments;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationIdAsync(int organizationId);

    /// <summary>
    /// Retrieves Last invoice by organization id asynchronously.
    /// </summary>
    /// <param name="organizationId">id of specific organization</param>
    /// <returns> InvoiceDto</returns>
    Task<Result<InvoiceDto>> GetLastInvoiceByOrganizationIdAsync(int organizationId);

    /// <summary>
    /// Retrieves a list of all garage transaction with no plate number  asynchronously.
    /// </summary>
    /// <returns>list of GarageTransactionDto</returns>
	Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumberAsync(int organizationId, Guid userId);

    /// <summary>
	/// Retrieves a list of all garage transaction  asynchronously.
	/// </summary>
	/// <returns>list of GarageTransactionDto</returns>
	Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsAsync(int organizationId, Guid userId);
    /// <summary>
    /// Update plate number of specific transaction.
    /// </summary>
    /// <param name="garageTransactionId">id of specific garageTransaction</param>
    /// <param name="plateNumber">plate number</param>
    Task<Result> CorrectTransactionByPlateNumberAsync(int garageTransactionId , string plateNumber);

    /// <summary>
	/// Retrieves a list of all existing transaction  asynchronously.
	/// </summary>
	/// <returns>list of GarageTransactionDto</returns>
	Task<Result<List<GarageTransactionDto>>> GetAllExistingTransactionsAsync(int organizationId, Guid userId);

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
    Task<Result<InvoiceMobileDto>> CheckoutAsync(int invoiceId, decimal DiscountValue = default);

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
}
