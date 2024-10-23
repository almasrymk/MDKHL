using SmartParking.Entities;
using SmartParking.Entities.Cameras;
using SmartParking.Lookups;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices;

public class ScreenAppService : CrudAppService<Screen, 
                                                ScreenDto, 
                                                int, 
                                                PagedAndSortedResultRequestDto,
                                                CreateUpdateScreenDto>,
    IScreenAppService
{
    public ScreenAppService(IRepository<Screen, int> repository) : base(repository)
    {
    }


    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var camera = await GetAsync(id);
        if (camera == null)
            throw new EntityNotFoundException(typeof(Screen), id);

        await base.DeleteAsync(id);
    }
}
