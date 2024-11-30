using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using SmartParking.Results;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Mobile.Keepers;

[RemoteService(isEnabled:false)]
public class UserAppService : ApplicationService, IUserAppService
{
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;

    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;
    private readonly IHttpContextAccessor _httpContext;
    public UserAppService(
         IRepository<Volo.Abp.Identity.IdentityUser,Guid> userRepository, 
         UserManager<Volo.Abp.Identity.IdentityUser> userManager,
         IHttpContextAccessor httpContext)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _httpContext = httpContext;
    }

    /// <inheritdoc/>
    public async Task<Result> SetNewPasswordAsync(ChangePasswordDto input)
    {
        var Name = CurrentUser.Name;

		Volo.Abp.Identity.IdentityUser? user;
        using(DataFilter.Disable<IMultiTenant>())
        {
		  user = await _userManager.FindByNameAsync(Name);

		}

		if (user is null)
        {
            return Result.Invalid(new ValidationError
            {
                ErrorMessage = "User Not Found"
            });
        }

		await _userManager.RemovePasswordAsync(user);
		var password = await _userManager.AddPasswordAsync(user, input.NewPassword);

		if (password.Succeeded)
		{
			return Result.SuccessWithMessage("Set New Password Successfully");

		}

		return Result.Invalid(new Results.ValidationError
		{
			ErrorMessage = "Incorrect password."
		});
	}

    /// <inheritdoc/>
    public async Task<Result<UserDataDto>> GetUserDataAsync()
    {
        
        var userName = CurrentUser.Name;
        
        if (userName.IsNullOrEmpty())
        {
            return Result<UserDataDto>.NotFound("User Not Found");
        }

        Volo.Abp.Identity.IdentityUser? user;
        using (DataFilter.Disable<IMultiTenant>())
        {
            user = await _userManager.FindByNameAsync(userName);
        }

        if (user != null)
        {
            return Result<UserDataDto>.Success(new UserDataDto
            {
                FullName = user.UserName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Image = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}/images/users/149071.png",
                OrganizationId = user.GetProperty<int>("OrganizationId")
            });
        }
        return Result<UserDataDto>.NotFound("User not found with the specified username");
    }

    /// <inheritdoc/>
    public async Task<Result<UserDataDto>> UpdateUserAsync(string phoneNumber, string password)
    {
        Volo.Abp.Identity.IdentityUser? user;
        using (DataFilter.Disable<IMultiTenant>())
        {
             user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }
        if (user is null)
        {
            return Result.Invalid(new ValidationError
            {
                ErrorMessage = "user with phone number not found"
            });
        }
        user.SetPhoneNumber(phoneNumber, true);
        // Change password (if you don’t have the current password, use RemovePassword and AddPassword)
        var removePasswordResult = await _userManager.RemovePasswordAsync(user);
        if (!removePasswordResult.Succeeded)
        {
            return Result.Invalid(new ValidationError
            {
                ErrorMessage = "failed to remove password"
            });
        }

        var addPasswordResult = await _userManager.AddPasswordAsync(user, password);
        if (!addPasswordResult.Succeeded)
        {
            return Result.Invalid(new ValidationError
            {
                ErrorMessage = "failed to set new password"
            });
        }

        return new UserDataDto
        {
            FullName = user.UserName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            Image = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}/images/users/149071.png"
        };
    }

}
