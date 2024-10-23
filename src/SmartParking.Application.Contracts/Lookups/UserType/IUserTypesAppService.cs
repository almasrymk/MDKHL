using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups;

public interface IUserTypesAppService : ICrudAppService<UserTypeDto , int , PagedAndSortedResultRequestDto , CreateUpdateUserTypeDto>
{

    /// <summary>
    /// Method used to delete exists user type
    /// </summary>
    /// <param name="id">Id of user type that needed to be deleted</param>
    /// <returns>No Content</returns>
    public Task DeleteAsync(int id);
}
