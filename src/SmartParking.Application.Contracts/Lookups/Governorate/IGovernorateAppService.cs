using SmartParking.Lookups.Governorate;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups
{
    public interface IGovernorateAppService : ICrudAppService<GovernorateDto , int , PagedAndSortedResultRequestDto , CreateUpdateGovernorateDto>
    {
        Task<List<CountryLookupDto>> GetCountryLookupAsync();


        /// <summary>
        /// abstract method to search of governorates
        /// </summary>
        /// <param name="input">specifies max number of governorate and if it sorted or not and search term</param>
        /// <returns>GovernorateDto</returns>
        public Task<PagedResultDto<GovernorateDto>> Search(PagedAndSortedAndSearchResultRequestDto input);


        /// <summary>
        /// abstract method to filter of governorates
        /// </summary>
        /// <param name="input">specifies max number of governorate and if it sorted or not and filter</param>
        /// <returns>GovernorateDto</returns>
        public Task<PagedResultDto<GovernorateDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);


    }
}
