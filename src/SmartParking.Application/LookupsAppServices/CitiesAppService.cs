using SmartParking.Entities;
using SmartParking.Lookups;
using SmartParking.Lookups.City;
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
        private readonly IRepository<City, int> _repository;
        private readonly IRepository<Governorate, int> _governorateRepository;

        public CitiesAppService(IRepository<City, int> repository, IRepository<Governorate, int> governorateRepository)
        : base(repository)
        {
            _repository = repository;
            _governorateRepository = governorateRepository;
        }

        public async Task<List<GovernorateLookupDto>> GetGovernmentLookupAsync()
        {
            var governorates = await _governorateRepository.GetListAsync();
            var governoratesList = ObjectMapper.Map<List<Governorate>, List<GovernorateLookupDto>>(governorates)
;
            return governoratesList;

        }

        public async Task<List<CityLookupDto>> GetCityLookupAsync()
        {
            var cities = await _repository.GetListAsync();
            var citiesList = ObjectMapper.Map<List<City>, List<CityLookupDto>>(cities)
;
            return citiesList;

        }
    }
}
