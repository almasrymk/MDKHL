using SmartParking.Invoices;
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.MobileInvoices;

public interface IInvoicesAppService : IApplicationService
{
    Task<Result<InvoiceDto>> CreateAsync(CreateUpdateInvoiceDto input);
    Task<Result<PagedResultDto<InvoiceDto>>> GetListAsync(PagedAndSortedResultRequestDto input);
}
