using SmartParking.Entities.Invoices;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Invoices;

public class InvoiceAppService : CrudAppService<Invoice,InvoiceDto,int, PagedAndSortedResultRequestDto,CreateUpdateInvoiceDto> , IInvoiceAppService
{
    private IDataFilter _dataFilter;
    public InvoiceAppService(IRepository<Invoice,int> repository, IDataFilter dataFilter) 
    :base(repository)
    {
        dataFilter = _dataFilter;
    }

    public override async Task<InvoiceDto> CreateAsync(CreateUpdateInvoiceDto input)
    {
        if (CurrentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.CreateAsync(input);
            }
        }
        else
            return await base.CreateAsync(input);
    }
    public override async Task<PagedResultDto<InvoiceDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        if (CurrentUser.IsInRole("admin"))
        {

            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.GetListAsync(input);
            }
        }

        return await base.GetListAsync(input);
    }

    public override async Task<InvoiceDto> GetAsync(int id)
    {
        if (CurrentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.GetAsync(id);
            }
        }
        else
            return await base.GetAsync(id);
    }
}
