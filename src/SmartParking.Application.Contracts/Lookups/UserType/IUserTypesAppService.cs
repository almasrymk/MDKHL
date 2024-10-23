using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups;

public interface IUserTypesAppService : ICrudAppService<UserTypeDto , int , PagedAndSortedResultRequestDto , CreateUpdateUserTypeDto>
{
    public Task DeleteAsync(int id);
}
