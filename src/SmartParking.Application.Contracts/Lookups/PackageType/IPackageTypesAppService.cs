using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups;

public interface IPackageTypesAppService : ICrudAppService<PackageTypeDto , int , PagedAndSortedResultRequestDto , CreateUpdatePackageTypeDto>
{


    /// <summary>
    /// Abstract method to create a new package type.
    /// </summary>
    /// <param name="input">The DTO containing the details for the new package type.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the created package type DTO.</returns>
    Task<PackageTypeDto> CreateAsync(CreateUpdatePackageTypeDto input);

    /// <summary>
    /// Abstract method to update an existing package type with the specified ID.
    /// </summary>
    /// <param name="input">The DTO containing the updated details for the package type.</param>
    /// <param name="id">The ID of the package type to update.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the updated package type DTO.</returns>
    Task<PackageTypeDto> UpdateAsync(int id, CreateUpdatePackageTypeDto input);

    /// <summary>
    /// Abstract method to search for package types based on the specified search criteria.
    /// </summary>
    /// <param name="input">The request DTO that specifies the search term, sort order, and maximum number of results.</param>
    /// <returns>A task representing the asynchronous operation, with a result of a paged list of package type DTOs matching the search criteria.</returns>
    Task<PagedResultDto<PackageTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Abstract method to filter package types based on specified filter criteria.
    /// </summary>
    /// <param name="input">The request DTO that specifies the filter criteria, sort order, and maximum number of results.</param>
    /// <returns>A task representing the asynchronous operation, with a result of a paged list of package type DTOs matching the filter criteria.</returns>
    Task<PagedResultDto<PackageTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Method to delete an existing package type by its ID.
    /// </summary>
    /// <param name="id">The ID of the package type to delete.</param>
    /// <returns>A task representing the asynchronous operation. The result is empty as it only performs a delete operation.</returns>
    Task DeleteAsync(int id);

    /// <summary>
    /// Method to retrieve a paged list of package types based on the specified input.
    /// </summary>
    /// <param name="input">The request DTO that specifies the sorting and paging criteria.</param>
    /// <returns>A task representing the asynchronous operation, with a result of a paged list of package type DTOs.</returns>
    Task<PagedResultDto<PackageTypeDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Method to retrieve all package types without any paging or sorting.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with a result of a list of all package type DTOs.</returns>
    Task<List<PackageTypeDto>> GetAllListAsync();

}