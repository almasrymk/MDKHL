using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Payments;

public interface IPaymentWebAppService
{
    //Task<PaymentDto> GetListMdkhlAsync(PagedAndSortedResultRequestDto input);
    Task<PaymentDto> GetListOrganizationAsync(PagedAndSortedResultRequestDto input);
}
