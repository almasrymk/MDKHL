using SmartParking.Entities.Payments;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Payments.PaymentRelations;

public class PaymentDetailsAppService : CrudAppService<
    PaymentDetails,
    PaymentDetailsDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdatePaymentDetailsDto>,
    IPaymentDetailsAppService
{
    private readonly IRepository<PaymentDetails, Guid> _repository;
    public PaymentDetailsAppService(IRepository<PaymentDetails, Guid> repository) : base(repository)
    {
        _repository = repository;
    }

    public async Task<PaymentDetailsDto> GetByPaymentIdAsync(Guid paymentId)
    {
        var paymentRelation = await _repository.FirstOrDefaultAsync(x => x.PaymentId == paymentId);
        if (paymentRelation == null)
        {
            throw new Exception("There is no such payment ID.");
        }
        return ObjectMapper.Map<PaymentDetails, PaymentDetailsDto>(paymentRelation);
    }
}
