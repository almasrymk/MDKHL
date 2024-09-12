using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups
{
    public interface IUserTypesAppService : ICrudAppService<UserTypeDto , int , PagedAndSortedResultRequestDto , CreateUpdateUserTypeDto>
    {
    }
}
