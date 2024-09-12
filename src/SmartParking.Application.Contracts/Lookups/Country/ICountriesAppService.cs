using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups
{
    public interface ICountriesAppService : ICrudAppService<CountryDto,int,PagedAndSortedResultRequestDto,CreateUpdateCountryDto>
    {
    }
}
