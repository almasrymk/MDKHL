using SmartParking.Invoices;
using SmartParking.Results;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.MobileInvoices;

public interface IInvoicesAppService : IApplicationService
{
    /// <summary>
    /// Asynchronously creates a new invoice based on the provided input data.
    /// </summary>
    /// <param name="input">An object containing the data to create a new invoice.</param>
    /// <returns>A task representing the asynchronous operation, containing a <see cref="Result{InvoiceDto}"/> object with the created invoice's details or an error message.</returns>
    Task<Result<InvoiceDto>> CreateAsync(CreateUpdateInvoiceDto input);

    /// <summary>
    /// Asynchronously retrieves a list of invoices.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing a <see cref="Result{List{InvoiceDto}}"/> object with the list of invoices.</returns>
    Task<Result<List<InvoiceDto>>> GetListAsync();

    /// <summary>
    /// Abstract method to retrieve a specific invoice by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice.</param>
    /// <returns>A task representing the asynchronous operation, containing a <see cref="Result{InvoiceDto}"/> object with the details of the requested invoice or an error message.</returns>
    public Task<Result<InvoiceDto>> GetAsync(int id);

}
