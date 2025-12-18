using System.Threading.Tasks;

namespace SmartParking.Invoices;

public interface IPublicInvoiceAppService
{
    Task<InvoiceDto?> GetLatestInvoiceForOrganizationByWatcher(string watcherCode);

}
