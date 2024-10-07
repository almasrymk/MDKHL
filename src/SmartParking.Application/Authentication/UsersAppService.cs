
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.ObjectMapping;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace SmartParking.Authentication;

public class UsersAppService : ApplicationService , IUsersAppService
{
    private readonly UserManager<IdentityUser> _userManager;

    public UsersAppService(SignInManager<IdentityUser> signInManager,UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto> CreateAsync(UserCreateDto input)
    {
        var user = new IdentityUser(GuidGenerator.Create(), input.UserName, input.Email, CurrentTenant.Id);
        user.SetProperty("OtpCode",null);
        user.SetProperty("OtpExpirationDate", DateTime.Now);
        if (CurrentUser.IsInRole(RolesType.Mdkhl.ToString()) || CurrentUser.IsInRole("admin"))
        {
            user.SetProperty("UserTypeId", 1);
        }else if (CurrentUser.IsInRole(RolesType.Organization.ToString()))
        {
            user.SetProperty("UserTypeId", 2);
        }
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
}
