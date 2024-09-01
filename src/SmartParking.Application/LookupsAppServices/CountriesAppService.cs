using SmartParking.Entities;
using SmartParking.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices
{
    public class CountriesAppService : CrudAppService<Country,
                                        CountryDto, int,
                                        PagedAndSortedResultRequestDto,
                                        CreateUpdateCountryDto>, 
        ICountriesAppService
    {
        public CountriesAppService(IRepository<Country, int> repository) : base(repository)
        {
        }
    }
}
