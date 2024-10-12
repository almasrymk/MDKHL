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
    /// Abstract method to delete organization
    /// </summary>
    /// <param name="id">Id of organization</param>
    /// <returns>Successfull message of deletion</returns>
    Task<string> DeleteByIdAsync(int id);

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
}
