using Microsoft.AspNetCore.Authorization;
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
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace SmartParking.Authentication;

[Authorize]
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
        if(input.RoleNames.Count() == 0 )
        {
            throw new Exception(message: "Must be enter Role");
        }
        var user = new IdentityUser(GuidGenerator.Create(), input.UserName, input.Email, input.TenantId);
         await CheckPhoneIsUnique(input.PhoneNumber);
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
    /// <summary>
    /// Checks if the provided phone number is unique and associated with a user.
    /// </summary>
    /// <param name="phone">The phone number to check for uniqueness.</param>
    /// <returns>The <see cref="IdentityUser"/> associated with the phone number if it exists.</returns>
    /// <exception cref="ValidationException">Thrown when the phone number is not associated with any user.</exception>
    private async Task<IdentityUser> CheckPhoneIsUnique(string phone )
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phone);

        if (user != null)
        {
            throw new Exception("User with this phone number does  exist.");
        }
        return user;

    }
    public async Task<int> GetOrganizationIdByTenantAsync(Guid? tenantId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var user = await _userRepository.FindByOrganizationByTenantAsync(tenantId);
            return user.GetProperty<int>("OrganizationId");
        }
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
