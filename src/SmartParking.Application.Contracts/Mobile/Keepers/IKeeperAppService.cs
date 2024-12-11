using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
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

    Task<Tenant> GetTenantByPhoneNumberDtoAsync(string phoneNum);

    Task<Result<PaymentCheckoutDto>> CheckoutAsync(int invoiceId);
    Task<Result<InvoiceDto>> PayCashForCustomerRequestAsync(int invoiceId, string PaymentType);

}
