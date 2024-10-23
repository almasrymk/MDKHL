using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups;

public interface IScreenAppService : ICrudAppService<ScreenDto , int , PagedAndSortedResultRequestDto , CreateUpdateScreenDto>
{
    public Task DeleteAsync(int id);
}
