using SmartParking.Entities;
using SmartParking.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices
{
    public class CitiesAppService : CrudAppService<City, CityDto,
                                    int, PagedAndSortedResultRequestDto,
                                    CreateUpdateCityDto>,
        ICitiesAppService

    {
        public CitiesAppService(IRepository<City, int> repository) : base(repository)
        {
        }
    }
}
