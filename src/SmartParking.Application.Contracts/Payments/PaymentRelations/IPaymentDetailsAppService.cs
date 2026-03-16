using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Payments.PaymentRelations;

public interface IPaymentDetailsAppService : 
    ICreateAppService<PaymentDetailsDto, CreateUpdatePaymentDetailsDto>,
    IReadOnlyAppService<PaymentDetailsDto, Guid, PagedAndSortedResultRequestDto>
{
    Task<PaymentDetailsDto> GetByPaymentIdAsync(Guid paymentId);
}
