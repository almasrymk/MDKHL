using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Lookups;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices;
[Authorize]

public class ScreenAppService : CrudAppService<Screen, 
                                                ScreenDto, 
                                                int, 
                                                PagedAndSortedResultRequestDto,
                                                CreateUpdateScreenDto>,
    IScreenAppService
{
    private readonly IRepository<Screen, int> _repository;

    public ScreenAppService(IRepository<Screen, int> repository) : base(repository)
    {
        _repository = repository;
    }


    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var screen = await _repository.FindAsync(id);
        if (screen == null)
            throw new EntityNotFoundException(typeof(Screen), id);
        screen.Status = Status.Deleted;
        await CurrentUnitOfWork.SaveChangesAsync();


    }
}
