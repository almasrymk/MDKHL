using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace SmartParking.Authentication;

public class UsersAppService : ApplicationService , IUsersAppService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMyUserRepository _userRepository;
    private readonly IDataFilter _dataFilter;

    public UsersAppService(SignInManager<IdentityUser> signInManager,UserManager<IdentityUser> userManager,IMyUserRepository userRepository , IDataFilter dataFilter)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _dataFilter = dataFilter;
    }

    public async Task<UserDto> CreateAsync(UserCreateDto input)
    {
        var user = new IdentityUser(GuidGenerator.Create(), input.UserName, input.Email, input.TenantId);
        
        user.Name = input.UserName;
        user.Surname = input.Surname;
        user.SetProperty("OtpCode",null);
        user.SetProperty("OtpExpirationDate", DateTime.Now);
        user.SetProperty("UserTypeId", input.UserType);
        user.SetProperty("OrganizationId", input.OrganizationId);
        var createdUser = await _userManager.CreateAsync(user, input.Password);
        await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber);
        if (!createdUser.Succeeded)
        {
            throw new Exception(message: createdUser.Errors.FirstOrDefault()?.Description);
        }

        await _userManager.AddToRolesAsync(user, input.RoleNames);

       var userDto =  ObjectMapper.Map<IdentityUser, UserDto>(user);
        return userDto;
    }

    public async Task<List<UserDto>> GetUserByOrganizationId(int organizationId)
    {


        using (_dataFilter.Disable<IMultiTenant>())
        {

            var users = await _userRepository.FindByOrganizationAsync(organizationId);
            var userMap = ObjectMapper.Map<List<IdentityUser>, List<UserDto>>(users);
             return userMap;
        }

    }
}
