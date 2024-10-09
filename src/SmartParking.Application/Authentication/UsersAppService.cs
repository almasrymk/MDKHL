
using Abp.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
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

        // Create a parameter for the IdentityUser object (x => ...)
        ParameterExpression param = Expression.Parameter(typeof(IdentityUser), "x");

        // Get the method info for GetProperty<int>
        var methodInfo = typeof(ObjectExtensions).GetMethod("GetProperty")
            ?.MakeGenericMethod(typeof(int));

        // Create an Expression to call GetProperty<int>("OrganizationId")
        var getPropertyCall = Expression.Call(
            methodInfo,
            param,
            Expression.Constant("OrganizationId") // Pass the property name
        );

        // Create the constant expression for the organizationId
        ConstantExpression value = Expression.Constant(organizationId);

        // Create an expression for x => x.GetProperty<int>("OrganizationId") == organizationId
        Expression condition = Expression.Equal(getPropertyCall, value);

        // Create the lambda expression: x => x.GetProperty<int>("OrganizationId") == organizationId
        Expression<Func<IdentityUser, bool>> expression = Expression.Lambda<Func<IdentityUser, bool>>(condition, param);
        Console.WriteLine("Expression"+expression);
        var users = await _userManager.Users.Where(expression).ToListAsync();
        Console.WriteLine(users[1].GetProperty("OrganizationId"));
        var userMap = ObjectMapper.Map<List<IdentityUser>, List<UserDto>>(users);
        return userMap;

    }
}
