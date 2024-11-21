using SmartParking.Invoices;
using SmartParking.Results;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.MobileInvoices;

public interface IInvoicesAppService : IApplicationService
{
    Task<Result<InvoiceDto>> CreateAsync(CreateUpdateInvoiceDto input);
    Task<Result<List<InvoiceDto>>> GetListAsync();

    /// <summary>
    /// abstract method to get specific invoice
    /// </summary>
    /// <param name="id">ivoice id</param>
    /// <returns>Invoice Dto</returns>
    public Task<Result<InvoiceDto>> GetAsync(int id);
}
