using SmartParking.Cutomers;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Garages;
/// <summary>
/// Interface for managing Garage-related operations, including CRUD and various filtering and searching methods.
/// </summary>
public interface IGarageAppService : ICrudAppService<GarageDto, int, PagedAndSortedResultRequestDto, CreateUpdateGarageDto>
{
    /// <summary>
    /// Abstract method to create a new garage.
    /// </summary>
    /// <param name="input">DTO containing the data to create a new garage.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="GarageDto"/> containing the details of the newly created garage.</returns>
    Task<GarageDto> CreateAsync(CreateUpdateGarageDto input);

    /// <summary>
    /// Abstract method used to update an existing garage.
    /// </summary>
    /// <param name="id">The unique identifier of the garage to update.</param>
    /// <param name="input">DTO containing the updated data for the garage.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="GarageDto"/> containing the updated details of the garage.</returns>
    Task<GarageDto> UpdateAsync(int id, CreateUpdateGarageDto input);

    /// <summary>
    /// Retrieves the garage details associated with a specific organization.
    /// </summary>
    /// <param name="input">Pagination, sorting, and filtering input.</param>
    /// <param name="organizationId">The unique identifier of the organization.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="PagedResultDto{GarageDto}"/> containing the paged garage details.</returns>
    Task<PagedResultDto<GarageDto>> GetGarageByOrganization(PagedAndSortedResultRequestDto input, int organizationId);

    /// <summary>
    /// Abstract method to search for garages based on specified criteria.
    /// </summary>
    /// <param name="input">Specifies maximum number of garages to return, whether to apply sorting, and the search term.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="PagedResultDto{GarageDto}"/> containing the search results.</returns>
    Task<PagedResultDto<GarageDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Abstract method to search for garages for MDKHL (specific use case).
    /// </summary>
    /// <param name="input">Specifies maximum number of garages to return, whether to apply sorting, and the search term.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="PagedResultDto{GarageDto}"/> containing the search results for MDKHL.</returns>
    Task<PagedResultDto<GarageDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Abstract method to filter garages based on specific criteria.
    /// </summary>
    /// <param name="input">Specifies maximum number of garages to return, whether to apply sorting, and any filter criteria.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="PagedResultDto{GarageDto}"/> containing the filtered results.</returns>
    Task<PagedResultDto<GarageDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Abstract method to filter garages for MDKHL (specific use case).
    /// </summary>
    /// <param name="input">Specifies maximum number of garages to return, whether to apply sorting, and any filter criteria for MDKHL.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="PagedResultDto{GarageDto}"/> containing the filtered results for MDKHL.</returns>
    Task<PagedResultDto<GarageDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Method to get all garages with pagination.
    /// </summary>
    /// <param name="input">Pagination input, specifying sorting and filtering options.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="PagedResultDto{GarageDto}"/> containing the list of garages.</returns>
    Task<PagedResultDto<GarageDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Method to get the list of garages for an organization with search and pagination.
    /// </summary>
    /// <param name="input">Search and pagination parameters.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="PagedResultDto{GarageDto}"/> containing the list of garages for the organization.</returns>
    Task<PagedResultDto<GarageDto>> GetListOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Method to get the list of garages for MDKHL with search and pagination.
    /// </summary>
    /// <param name="input">Search and pagination parameters for MDKHL.</param>
    /// <param name="tenantId">Optional tenant ID for filtering garages by tenant.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="PagedResultDto{GarageDto}"/> containing the list of garages for MDKHL.</returns>
    Task<PagedResultDto<GarageDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Method to get the total count of all garages.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with an integer value representing the total count of garages.</returns>
    Task<int> GetAllGaragesCount();

    /// <summary>
    /// Method to delete an existing garage by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the garage to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(int id);

    /// <summary>
    /// Method to change the tenant for all cameras related to a specific garage.
    /// </summary>
    /// <param name="GarageID">The unique identifier of the garage.</param>
    /// <param name="TenantId">The new tenant ID to assign to the cameras.</param>
    /// <returns>A task that represents the asynchronous operation, with a boolean flag indicating the status of the operation.</returns>
    Task<bool> ChangeTenantForCamerasRelatedToGarage(int GarageID, Guid TenantId);

    /// <summary>
    /// Method to get the details of a specific garage by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the garage.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="GarageDto"/> containing the garage details.</returns>
    Task<GarageDto> GetAsync(int id);

    /// <summary>
    /// Method to add the number of active cameras to an organization's garage.
    /// </summary>
    /// <param name="GarageId">The unique identifier of the garage.</param>
    /// <param name="NumOfActiveCamera">The number of active cameras to add.</param>
    /// <returns>A task representing the asynchronous operation, with a boolean flag indicating the success of the operation.</returns>
    Task<bool> AddActiveCameraRelatedToOrganizationGarage(int GarageId, int NumOfActiveCamera);

    /// <summary>
    /// Method used by MDKHL admin to get the count of all active garages and their maximum capacity.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with a tuple containing both the count of active garages and their maximum capacity.</returns>
    Task<Tuple<int, int>> GetMaxCapacityofActiveGaragesAndItsCount();
}
