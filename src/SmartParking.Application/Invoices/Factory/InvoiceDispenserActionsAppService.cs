using Volo.Abp.Application.Services;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Invoices;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp;


namespace SmartParking.Invoices.Factory;

public class InvoiceDispenserActionsAppService : ApplicationService,IInvoiceActionsAppService
{
    private readonly IRepository<Invoice, int> _repository;
    private readonly IRepository<Dispenser, int> _dispenserRepository;

    public InvoiceDispenserActionsAppService(IRepository<Invoice, int> repository, 
        IRepository<Dispenser, int> dispenserRepository)
    {
        _repository = repository;
        _dispenserRepository = dispenserRepository;
    }

    [RemoteService(isEnabled: false)]
    public bool CanHandle(InvoiceSearchDto invoiceSearchDto)
    {
        return !string.IsNullOrEmpty(invoiceSearchDto.TicketId);
    }

    [RemoteService(isEnabled: false)]
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
        var result = ObjectMapper.Map<Invoice, InvoiceDto>(invoice);
        return result;
    }

    [RemoteService(isEnabled: false)]
    public async Task<Watcher> GetWatcherByCodeAsync(string watcherCode)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var camera = await _dispenserRepository.FirstOrDefaultAsync(x => x.DeviceId == watcherCode);
            return camera;
        }
    }
}
