
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Authentication;

public interface IUsersAppService:  IApplicationService
{
    public Task<UserDto> CreateAsync(UserCreateDto input);
    public Task<List<UserDto>> GetUserByOrganizationId(int organizationId);
    public Task<int> GetOrganizationIdByTenantAsync(Guid? tenantId);

}
