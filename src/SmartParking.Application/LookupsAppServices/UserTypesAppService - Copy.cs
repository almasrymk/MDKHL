using SmartParking.Entities;
using SmartParking.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices
{
    public class PackageTypesAppService : CrudAppService<PackageType,
                                                       PackageTypeDto,
                                                       int,
                                                       PagedAndSortedResultRequestDto,
                                                       CreateUpdatePackageTypeDto>,
        IPackageTypesAppService
    {
        public PackageTypesAppService(IRepository<PackageType, int> repository) : base(repository)
        {
        }
    }
}
