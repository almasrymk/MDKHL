using SmartParking.Entities;
using SmartParking.Lookups;
using SmartParking.Lookups.City;
using SmartParking.Lookups.Governorate;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly IRepository<Governorate, int> _governorateRepository;

        public CitiesAppService(IRepository<City, int> repository, IRepository<Governorate, int> governorateRepository)
        : base(repository)
        {
            _governorateRepository = governorateRepository;
        }

        public async Task<PagedResultDto<GovernmentLookupDto>> GetGovernmentLookupAsync(PagedAndSortedResultRequestDto input)
        {
            var countries = await _governorateRepository.GetListAsync();
            var countryList = ObjectMapper.Map<List<Governorate>, List<GovernmentLookupDto>>(countries)
;
            return new PagedResultDto<GovernmentLookupDto>
            {
                Items = countryList,
                TotalCount = countries.Count,
            };

        }
    }
}
