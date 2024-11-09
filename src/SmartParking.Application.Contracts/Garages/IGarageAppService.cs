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

public interface IGarageAppService : ICrudAppService<GarageDto,int,PagedAndSortedResultRequestDto,CreateUpdateGarageDto>
{
    /// <summary>
    /// abstract method to create garage
    /// </summary>
    /// <param name="input">Garage create dto</param>
    /// <returns>Garage Dto</returns>
    Task<GarageDto> CreateAsync(CreateUpdateGarageDto input);


    /// <summary>
    /// Abstract method used to update garage
    /// </summary>
    /// <param name="id">Garage id</param>
    /// <param name="input">Dto of create garage</param>
    /// <returns>Garage dto of update one</returns>

    Task<GarageDto> UpdateAsync(int id, CreateUpdateGarageDto input);
    /// <summary>
    /// Retrieves the garage details associated with a specific organization.
    /// </summary>
    /// <param name="organizationId">The unique identifier of the organization.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="GarageDto"/> containing the garage details.</returns>
    Task<PagedResultDto<GarageDto>> GetGarageByOrganization(PagedAndSortedResultRequestDto input  , int organizationId);

    /// <summary>
    /// abstract method to search of garages
    /// </summary>
    /// <param name="input">specifies max number of garage and if it sorted or not and search term</param>
    /// <returns>Garage Dto</returns>
    public Task<PagedResultDto<GarageDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of garages
    /// </summary>
    /// <param name="input">specifies max number of garages and if it sorted or not and filter</param>
    /// <returns>Garage Dto</returns>
    public Task<PagedResultDto<GarageDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);



    /// <summary>
    /// Method used to get all garages
    /// </summary>
    /// <param name="input">Input used for pagination</param>
    /// <returns>Paged Result of Garages</returns>
    Task<PagedResultDto<GarageDto>> GetListAsync(PagedAndSortedResultRequestDto input);


    /// <summary>
    /// Method used to get Count of Garages.
    /// </summary>
    /// <returns>Count of Garages</returns>
    Task<int> GetAllGaragesCount();
    /// <summary>
    /// Method used to delete exists garage
    /// </summary>
    /// <param name="id">Id of garage that needed to be deleted</param>
    /// <returns>No Content</returns>
    Task DeleteAsync(int id);

    /// <summary>
    /// Method used to change tenant for list of cameras that related to specific garage.
    /// </summary>
    /// <param name="GarageID">GarageId</param>
    /// <param name="TenantId">TenantId</param>
    /// <returns></returns>
    Task<bool> ChangeTenantForCamerasRelatedToGarage(int GarageID, Guid TenantId);
    /// <summary>
    /// Method used to get garage by Id
    /// </summary>
    /// <param name="id">Id of garage</param>
    /// <returns>Garage Dto</returns>

    Task<GarageDto> GetAsync(int id);
    /// <summary>
    /// Method used to add number of active camera to organization related to its garage.
    /// </summary>
    /// <param name="GarageId">GarageId</param>
    /// <param name="NumOfActiveCamera">NumberOfActiveCameras</param>
    /// <returns>Flag detects the status</returns>
    Task<bool> AddActiveCameraRelatedToOrganizationGarage(int GarageId,int NumOfActiveCamera);
/// <summary>
/// Method used by mdkhl admin.
/// To get count of all active garages and its maximum capacity.
/// </summary>
/// <returns>tuple of both values.</returns>
	Task<Tuple<int, int>> GetMaxCapacityofActiveGaragesAndItsCount();

}
