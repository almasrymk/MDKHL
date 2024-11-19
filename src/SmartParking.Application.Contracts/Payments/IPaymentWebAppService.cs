using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Payments;

public interface IPaymentWebAppService
{
    Task<PagedResultDto<PaymentResponseDto>> GetListMdkhlAsync(PagedAndSortedResultRequestDto input);
    Task<PaymentGetResponseDto> GetMdkhlAsync(Guid paymentId);
    Task<PagedResultDto<PaymentResponseDto>> GetListOrganizationAsync(PagedAndSortedResultRequestDto input);
    Task<PagedResultDto<PaymentResponseDto>> SearchMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input);
    Task<PagedResultDto<PaymentResponseDto>> SearchOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input);
}
