
using SmartParking.Entities.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Payments;

public class PaymentWebAppService : ApplicationService, IPaymentWebAppService
{
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IDataFilter _dataFilter;

    public PaymentWebAppService(IRepository<Payment,Guid> paymentRepository,IDataFilter dataFilter)
    {
        _paymentRepository = paymentRepository;
        _dataFilter = dataFilter;
    }

    //public Task<PaymentDto> GetListMdkhlAsync(PagedAndSortedResultRequestDto input)
    //{
    //    using (_dataFilter.Disable<IMultiTenant>())
    //    {

    //    }
    //}

    public Task<PaymentDto> GetListOrganizationAsync(PagedAndSortedResultRequestDto input)
    {
        throw new NotImplementedException();
    }
}
