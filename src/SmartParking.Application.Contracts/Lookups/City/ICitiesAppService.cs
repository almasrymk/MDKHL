using SmartParking.Lookups.City;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups
{
    public interface ICitiesAppService :ICrudAppService<CityDto,int,PagedAndSortedResultRequestDto,CreateUpdateCityDto>
    {
        Task<List<GovernorateLookupDto>> GetGovernmentLookupAsync();
        Task<List<CityLookupDto>> GetCityLookupAsync();

    }
}
