using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups;

public interface IScreenAppService : ICrudAppService<ScreenDto , int , PagedAndSortedResultRequestDto , CreateUpdateScreenDto>
{

    /// <summary>
    /// Method used to delete exists screen
    /// </summary>
    /// <param name="id">Id of screen that needed to be deleted</param>
    /// <returns>No Content</returns>
    public Task DeleteAsync(int id);
}
