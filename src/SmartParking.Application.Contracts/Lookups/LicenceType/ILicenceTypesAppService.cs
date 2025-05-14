using SmartParking.Lookups.PlateColor;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.LicenceType;

public interface ILicenceTypesAppService : ICrudAppService<LicenceTypeDto, int, PagedAndSortedResultRequestDto, CreateUpdateLicenceTypeDto>
{


    /// <summary>
    /// Abstract method to create a new license type.
    /// </summary>
    /// <param name="input">The license type create DTO containing the necessary information to create the license type.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created license type DTO.</returns>
    public Task<LicenceTypeDto> CreateAsync(CreateUpdateLicenceTypeDto input);

    /// <summary>
    /// Abstract method to update an existing license type.
    /// </summary>
    /// <param name="input">The license type create DTO containing the updated information for the license type.</param>
    /// <param name="id">The ID of the specific license type to be updated.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated license type DTO.</returns>
    public Task<LicenceTypeDto> UpdateAsync(int id, CreateUpdateLicenceTypeDto input);

    /// <summary>
    /// Abstract method to search for license types based on specified criteria.
    /// </summary>
    /// <param name="input">The request DTO that contains the maximum number of license types to return, sorting information, and search term.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paged result DTO with the matching license types.</returns>
    public Task<PagedResultDto<LicenceTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Abstract method to filter license types based on specified criteria.
    /// </summary>
    /// <param name="input">The request DTO that contains the maximum number of license types to return, sorting information, and filter criteria.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paged result DTO with the filtered license types.</returns>
    public Task<PagedResultDto<LicenceTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Abstract method to delete a specific license type by its ID.
    /// </summary>
    /// <param name="id">The ID of the license type to be deleted.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task DeleteAsync(int id);

    /// <summary>
    /// Abstract method to retrieve a list of all license types.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of all license type DTOs.</returns>
    public Task<List<LicenceTypeDto>> GetAllLicencesAsync();

    /// <summary>
    /// Abstract method to retrieve all license types with additional result formatting.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a formatted result with a list of all license type DTOs.</returns>
    public Task<Result<List<LicenceTypeDto>>> GetAllLicenceTypesAsync();

}
