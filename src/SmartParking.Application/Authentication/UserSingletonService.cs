using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartParking.Enums;
using SmartParking.KeeperShifts;
using SmartParking.SingletonService;
using SmartParking.StateManagements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

namespace SmartParking.Authentication;

public class UserSingletonService : ISingletonService<IdentityUser, Guid?>
{
    public static IdentityUser? instance;
    private readonly IUsersAppService _usersAppService;

    public UserSingletonService(IUsersAppService usersAppService)
    {
        _usersAppService = usersAppService;
    }

    public async ValueTask DisposeAsync()
    {
        instance = null;

    }

    public async Task<IdentityUser> GetAsync(Guid? key)
    {
        if (instance == null)
        {
           
           instance = await _usersAppService.IsUserFound(key);
        }

        return instance;
    }

    public async Task<IdentityUser> SetAsync(Guid? key)
    {


        if (instance == null)
        {

           instance = await _usersAppService.IsUserFound(key);

        }
        return instance;
    }
   
    
    public async Task<bool> ISCurrentUserIsAssignShift(Guid? key)
    {
        if (instance == null)
        {
            await GetAsync(key);

        }
        var userCategory = instance.GetProperty<UserTypeCategory>("UserTypeCategory");
        return userCategory > 0 ? true : false;
    }

    public void Refresh()
    {
        instance = null;
    }
}
