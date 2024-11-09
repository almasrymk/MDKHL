using SmartParking.Cutomers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Payments;

public interface IPaymentAppService : ICrudAppService<PaymentDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePaymentDto>
{

    Task<PaymentDto> CreateCheckoutPaymentAsync(CreateUpdatePaymentDto input);
    Task<PaymentDto> GetPaymentStatus(string input, string entityId);


}
