using SmartParking.Lookups.Governorate;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups
{
    public interface IGovernorateAppService : ICrudAppService<GovernorateDto , int , PagedAndSortedResultRequestDto , CreateUpdateGovernorateDto>
    {


        /// <summary>
        /// abstract method to create governorate
        /// </summary>
        /// <param name="input">governorate create dto</param>
        /// <returns>governorateDto</returns>
        public Task<GovernorateDto> CreateAsync(CreateUpdateGovernorateDto input);

        /// <summary>
        /// abstract method to update specific governorate
        /// </summary>
        /// <param name="input">governorate create dto</param>
        /// <param name="id">id of specific governorate</param>
        /// <returns>governorateDto</returns>
        public Task<GovernorateDto> UpdateAsync(int id,CreateUpdateGovernorateDto input);
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


    /// <summary>
    /// Method used to delete exists Governorate
    /// </summary>
    /// <param name="id">Id of Governorate that needed to be deleted</param>
    /// <returns>No Content</returns>
    public Task DeleteAsync(int id);


}
