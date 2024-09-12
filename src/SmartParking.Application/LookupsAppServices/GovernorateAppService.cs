using SmartParking.Entities;
using SmartParking.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices
{
    public class GovernorateAppService : CrudAppService<Governorate,
                                                       GovernorateDto,
                                                       int,
                                                       PagedAndSortedResultRequestDto,
                                                       CreateUpdateGovernorateDto>,
                                          IGovernorateAppService
    {
        public GovernorateAppService(IRepository<Governorate, int> repository) : base(repository)
        {
        }
    }
}
