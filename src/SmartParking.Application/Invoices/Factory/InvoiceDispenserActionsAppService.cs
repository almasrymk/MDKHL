using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Invoices;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Invoices.Factory;

public class InvoiceDispenserActionsAppService : IInvoiceActionsAppService
{
    private readonly IRepository<Invoice, int> _repository;
    private readonly IObjectMapper _objectMapper;

    public InvoiceDispenserActionsAppService(IRepository<Invoice, int> repository, IObjectMapper objectMapper)
    {
        _repository = repository;
        _objectMapper = objectMapper;
    }
    public bool CanHandle(InvoiceSearchDto invoiceSearchDto)
    {
        return !string.IsNullOrEmpty(invoiceSearchDto.TicketId);
    }

    public async Task<InvoiceDto> Search(InvoiceSearchDto invoiceSearchDto)
    {
        var invoice = await _repository.FirstOrDefaultAsync(_ => _.TicketId == invoiceSearchDto.TicketId);
        if (string.IsNullOrEmpty(invoiceSearchDto.TicketId))
        {
            invoice = await _repository.FirstOrDefaultAsync(_ => _.PlateNumber == invoiceSearchDto.PlateNumber);
        }
       
        if (invoice == null)
        {
            return null;
        }
        var result = _objectMapper.Map<Invoice, InvoiceDto>(invoice);
        return result;
    }
}
