using Volo.Abp.Application.Services;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Invoices;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp;

namespace SmartParking.Invoices.Factory;

public class InvoiceCameraActionsAppService : ApplicationService, IInvoiceActionsAppService
{
    private readonly IRepository<Invoice, int> _repository;
    private readonly IRepository<Camera, int> _cameraRepository;

    public InvoiceCameraActionsAppService(IRepository<Invoice, int> repository, 
        IRepository<Camera, int> cameraRepository)
    {
        _repository = repository;
        _cameraRepository = cameraRepository;
    }
    [RemoteService(isEnabled:false)]
    public bool CanHandle(InvoiceSearchDto invoiceSearchDto)
    {
        return (!string.IsNullOrEmpty(invoiceSearchDto.PlateNumber) && string.IsNullOrEmpty(invoiceSearchDto.TicketId));
    }

    [RemoteService(isEnabled: false)]
    public async Task<InvoiceDto> Search(InvoiceSearchDto invoiceSearchDto)
    {
        var invoice = await _repository.FirstOrDefaultAsync(_ => _.PlateNumber == invoiceSearchDto.PlateNumber);
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
            var camera = await _cameraRepository.FirstOrDefaultAsync(x => x.DeviceId == watcherCode);
            return camera;
        }
    }
}
