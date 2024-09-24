using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups
{
    public interface IPackageTypesAppService : ICrudAppService<PackageTypeDto , int , PagedAndSortedResultRequestDto , CreateUpdatePackageTypeDto>
    {
    }
}
