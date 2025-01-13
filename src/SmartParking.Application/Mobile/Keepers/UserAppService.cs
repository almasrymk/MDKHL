using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using SmartParking.Authentication;
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
    private readonly IUsersAppService _usersAppService;
    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;
    private readonly IHttpContextAccessor _httpContext;
    public UserAppService(
         IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository,
         UserManager<Volo.Abp.Identity.IdentityUser> userManager,
         IHttpContextAccessor httpContext,
         IUsersAppService usersAppService)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _httpContext = httpContext;
        _usersAppService = usersAppService;
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
        
        var userName = CurrentUser.UserName;
        
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
    public async Task<Result<UserDataDto>> UpdateUserAsync(string FullName,string phoneNumber, string password, string email, string image)
    {
        Volo.Abp.Identity.IdentityUser? user;
        using (DataFilter.Disable<IMultiTenant>())
        {
            user = await _userRepository.FirstOrDefaultAsync(user => user.Id == CurrentUser.Id);
        }
        if (user is null)
        {
            return Result.Invalid(new ValidationError
            {
                ErrorMessage = "user with phone number not found"
            });
        }
        if (user.Name != FullName) { 
            user.Name = FullName;

        }
        if (phoneNumber != null)
        {
            user.SetPhoneNumber(phoneNumber, true);
            await _userManager.SetUserNameAsync(user,phoneNumber);
        }
        if (password != null)
        {
            user = await ChangePassword(password, user);

        }
        if (email != null)
        {
            var token =await _userManager.GenerateChangeEmailTokenAsync(user, email);
            var identityResult = await _userManager.ChangeEmailAsync(user, email,token);

        }
        UserDto result = new();
        if (image != null)
        {
             result = await _usersAppService.UpdateImageAsync(CurrentUser.Id, image);
        }
        user.SetConcurrencyStampIfNotNull(user.ConcurrencyStamp);
        var Newuser = await _userManager.UpdateAsync(user);


        return new UserDataDto
        {
            FullName = user.Name,
            UserName = user.UserName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            Image = image is not null ? result.Image : null,
            OrganizationId = user.GetProperty<int>("OrganizationId")
        };
    }

    private async Task<Result<Volo.Abp.Identity.IdentityUser?>> ChangePassword(string password, Volo.Abp.Identity.IdentityUser? user)
    {
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
        return user;
    }
}
