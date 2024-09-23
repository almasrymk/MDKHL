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
    Task<string> DeleteByIdAsync(int id);

    /// <summary>
    /// abstract method to search of garage
    /// </summary>
    /// <param name="input">specifies max number of garage and if it sorted or not and search term</param>
    /// <returns>garage Dto</returns>
    public Task<PagedResultDto<GarageDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of garages
    /// </summary>
    /// <param name="input">specifies max number of garage and if it sorted or not and filter</param>
    /// <returns>garage Dto</returns>
    public Task<PagedResultDto<GarageDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
}
