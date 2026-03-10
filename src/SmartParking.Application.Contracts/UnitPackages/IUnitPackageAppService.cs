using iText.Layout.Element;
using SmartParking.Enums;
using SmartParking.Packages;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.UnitPackages;

public interface IUnitPackageAppService
    : ICrudAppService<
        UnitPackageDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateUnitPackageDto>,
    IApplicationService
{
    Task<PagedResultDto<UnitPackageDto>> GetUnitPackagesByOrganization(PagedAndSortedResultRequestDto input, int organizationId);
    Task<List<UnitPackageDto>> GetUnitPackagesByCategory(CustomerCategoryStatus customerCategory);
    Task<List<UnitPackageDto>> GetUnitPackagesByCategory(int organizationId,CustomerCategoryStatus customerCategory);
    Task<PagedResultDto<UnitPackageDto>> GetListAsyncForMdkhl(PagedAndSortedResultRequestDto input);
}
