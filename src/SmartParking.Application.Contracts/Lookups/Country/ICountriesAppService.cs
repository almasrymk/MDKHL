using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups
{
    public interface ICountriesAppService : ICrudAppService<CountryDto,int,PagedAndSortedResultRequestDto,CreateUpdateCountryDto>
    {

        /// <summary>
        /// abstract method to search of cities
        /// </summary>
        /// <param name="input">specifies max number of city and if it sorted or not and search term</param>
        /// <returns>Country Dto</returns>
        public Task<PagedResultDto<CountryDto>> Search(PagedAndSortedAndSearchResultRequestDto input);


        /// <summary>
        /// abstract method to filter of countries
        /// </summary>
        /// <param name="input">specifies max number of countries and if it sorted or not and filter</param>
        /// <returns>Country Dto</returns>
        public Task<PagedResultDto<CountryDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
    }
}
