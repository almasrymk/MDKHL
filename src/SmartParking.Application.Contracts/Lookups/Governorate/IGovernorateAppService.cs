using SmartParking.Lookups.Governorate;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups
{
    public interface IGovernorateAppService : ICrudAppService<GovernorateDto , int , PagedAndSortedResultRequestDto , CreateUpdateGovernorateDto>
    {
        Task<PagedResultDto<CountryLookupDto>> GetCountryLookupAsync(PagedAndSortedResultRequestDto input);

    }
}
