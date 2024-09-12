using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups
{
    public interface IGovernorateAppService : ICrudAppService<GovernorateDto , int , PagedAndSortedResultRequestDto , CreateUpdateGovernorateDto>
    {
    }
}
