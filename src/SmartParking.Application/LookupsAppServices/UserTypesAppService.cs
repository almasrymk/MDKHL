using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities;
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

public class UserTypesAppService : CrudAppService<UserType,
                                                   UserTypeDto,
                                                   int,
                                                   PagedAndSortedResultRequestDto,
                                                   CreateUpdateUserTypeDto>,
    IUserTypesAppService
{
    private readonly IRepository<UserType, int> _repository;

    public UserTypesAppService(IRepository<UserType, int> repository) : base(repository)
    {
        _repository = repository;
    }
    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var userType = await _repository.FindAsync(id);
        if (userType == null)
            throw new EntityNotFoundException(typeof(UserType), id);

        await _repository.HardDeleteAsync(userType);
    }

}
