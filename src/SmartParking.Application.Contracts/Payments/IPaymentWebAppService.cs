using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Payments;

public interface IPaymentWebAppService
{
    Task<PagedResultDto<PaymentDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);
    Task<PaymentGetResponseDto> GetMdkhlAsync(Guid paymentId);
    Task<PagedResultDto<PaymentDto>> GetListOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input);
    Task<PagedResultDto<PaymentDto>> SearchMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input);
    Task<PagedResultDto<PaymentDto>> SearchOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input);

    Task<PaymentDto> GetPaymentForInvoice(int invoiceId);
}
