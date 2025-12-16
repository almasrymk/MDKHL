using SmartParking.Entities.Invoices;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Invoices.Factory;

public class InvoiceCameraActionsAppService : IInvoiceActionsAppService
{
    private readonly IRepository<Invoice, int> _repository;
    private readonly IObjectMapper _objectMapper;

    public InvoiceCameraActionsAppService(IRepository<Invoice, int> repository, IObjectMapper objectMapper)
    {
        _repository = repository;
        _objectMapper = objectMapper;
    }
    public bool CanHandle(InvoiceSearchDto invoiceSearchDto)
    {
        return (!string.IsNullOrEmpty(invoiceSearchDto.PlateNumber) && string.IsNullOrEmpty(invoiceSearchDto.TicketId));
    }

    public async Task<InvoiceDto> Search(InvoiceSearchDto invoiceSearchDto)
    {
        var invoice = await _repository.FirstOrDefaultAsync(_ => _.PlateNumber == invoiceSearchDto.PlateNumber);
        if (invoice == null)
        {
            return null;
        }
        var result = _objectMapper.Map<Invoice, InvoiceDto>(invoice);
        return result;
    }
}
