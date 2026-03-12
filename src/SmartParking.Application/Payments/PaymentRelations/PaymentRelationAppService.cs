using SmartParking.Entities.Payments;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Payments.PaymentRelations;

public class PaymentRelationAppService : CrudAppService<
    PaymentRelation,
    PaymentRelationDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdatePaymentRelationDto>,
    IPaymentRelationAppService
{
    private readonly IRepository<PaymentRelation, Guid> _repository;
    public PaymentRelationAppService(IRepository<PaymentRelation, Guid> repository) : base(repository)
    {
        _repository = repository;
    }

    public async Task<PaymentRelationDto> GetByPaymentIdAsync(Guid paymentId)
    {
        var paymentRelation = await _repository.FirstOrDefaultAsync(x => x.PaymentId == paymentId);
        if (paymentRelation == null)
        {
            throw new Exception("There is no such payment ID.");
        }
        return ObjectMapper.Map<PaymentRelation, PaymentRelationDto>(paymentRelation);
    }
}
