using SmartParking.Invoices;
using SmartParking.Results;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationId(int organizationId);
}
