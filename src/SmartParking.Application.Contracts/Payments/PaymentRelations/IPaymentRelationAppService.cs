using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Payments.PaymentRelations;

public interface IPaymentRelationAppService : 
    ICreateAppService<PaymentRelationDto, CreateUpdatePaymentRelationDto>,
    IReadOnlyAppService<PaymentRelationDto, Guid, PagedAndSortedResultRequestDto>
{
    Task<PaymentRelationDto> GetByPaymentIdAsync(Guid paymentId);
}
