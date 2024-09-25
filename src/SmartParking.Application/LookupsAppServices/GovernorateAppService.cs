using SmartParking.Entities;
using SmartParking.Lookups;
using SmartParking.Lookups.Governorate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly IRepository<Country, int> _countryRepository;

        public GovernorateAppService(IRepository<Governorate, int> repository, IRepository<Country, int> countryRepository)
        : base(repository)
        {
            _countryRepository = countryRepository;
        }

        public async Task<PagedResultDto<CountryLookupDto>> GetCountryLookupAsync(PagedAndSortedResultRequestDto input)
        {
            var countries = await _countryRepository.GetListAsync();
            var countryList = ObjectMapper.Map<List<Country>, List<CountryLookupDto>>(countries)
;
            return new PagedResultDto<CountryLookupDto>
            {
                Items = countryList,
                TotalCount = countries.Count,
            };
           
        }
    }
}
