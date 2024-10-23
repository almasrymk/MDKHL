using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities;
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
    public UserTypesAppService(IRepository<UserType, int> repository) : base(repository)
    {
    }
    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var city = await GetAsync(id);
        if (city == null)
            throw new EntityNotFoundException(typeof(UserType), id);

        await base.DeleteAsync(id);
    }

}
