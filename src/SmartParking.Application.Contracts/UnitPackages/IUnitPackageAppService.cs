using SmartParking.Packages;
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
}
