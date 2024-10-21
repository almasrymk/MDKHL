using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Invoices;
using SmartParking.MobileInvoices;
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartParking.Controllers;

[Route("api/mobile/Invoice")]
[ApiController]

public class InvoicesController : AbpController
{
    private readonly IInvoicesAppService _invoicesAppService;
    public InvoicesController(IInvoicesAppService invoicesAppService)
    {
        _invoicesAppService = invoicesAppService;
    }
    [HttpPost]
    [Authorize]
    public async Task<Result<InvoiceDto>> CreateInvoice(CreateUpdateInvoiceDto input)
    {
        return await _invoicesAppService.CreateAsync(input);
    }
    [HttpGet]
    [Authorize]
    public async Task<Result<PagedResultDto<InvoiceDto>>> GetAllInvoices([FromQuery] PagedAndSortedResultRequestDto input)
    {
        return await _invoicesAppService.GetListAsync(input);
    }

}
    
    

