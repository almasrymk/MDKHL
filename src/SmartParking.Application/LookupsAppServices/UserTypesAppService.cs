using SmartParking.Entities;
using SmartParking.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices
{
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
    }
}
