using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Invoices;
public interface IInvoiceAppService : ICrudAppService<InvoiceDto, int, PagedAndSortedResultRequestDto, CreateUpdateInvoiceDto>
{
    /// <summary>
    /// abstract method to create invoice
    /// </summary>
    /// <param name="input">ivoice create dto</param>
    /// <returns>Invoice Dto</returns>
    public Task<InvoiceDto> CreateAsync(CreateUpdateInvoiceDto input);

    /// <summary>
    /// abstract method to get specific invoice
    /// </summary>
    /// <param name="input">ivoice create dto</param>
    /// <returns>Invoice Dto</returns>
    public Task<InvoiceDto> GetAsync(int id);

    /// <summary>
    /// abstract method to get invoices
    /// </summary>
    /// <param name="input"> PagedAndSortedResultRequestDto</param>
    /// <returns>PagedResultDto of Invoice Dto</returns>
    public Task<PagedResultDto<InvoiceDto>> GetListAsync(PagedAndSortedResultRequestDto input);

}
