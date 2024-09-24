using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups
{
    public interface ICitiesAppService :ICrudAppService<CityDto,int,PagedAndSortedResultRequestDto,CreateUpdateCityDto>
    {
    }
}
