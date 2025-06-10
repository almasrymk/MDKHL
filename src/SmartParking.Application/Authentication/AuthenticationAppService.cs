
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Mobile.Otps;
using SmartParking.Organizations;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Validation;
using static System.Net.WebRequestMethods;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace SmartParking.Authentication;
// Service Implementation

/// <inheritdoc/>
public class AuthenticationAppService : ApplicationService, IAuthenticationAppService
{

    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IAuthenticationUtility _authenticationUtility;
    private readonly IUsersAppService _usersAppService;
    private readonly TaqnyatSmsOptionsDto _options;
    private readonly HttpClient _httpClient;
    private readonly ICurrentTenant _currentTenant;
    private readonly IOrganizationsAppService _organizationsAppService;

    public AuthenticationAppService(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IOptions<TaqnyatSmsOptionsDto> options,
        IHttpContextAccessor httpContext,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<Tenant, Guid> tenantRepository,
        IRepository<Customer, Guid> CustomerRepository,
        IRepository<Organization, int> organizationRepository,
        IDataFilter dataFilter,
        IAuthenticationUtility authenticationUtility,
        IUsersAppService usersAppService,
        HttpClient httpClient,
        ICurrentTenant currentTenant,
        IOrganizationsAppService organizationsAppService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _httpContext = httpContext;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _customerRepository = CustomerRepository;
        _organizationRepository = organizationRepository;
        _dataFilter = dataFilter;
        _authenticationUtility = authenticationUtility;
        _usersAppService = usersAppService;
        _httpClient = httpClient;
        _currentTenant = currentTenant;
        _options = options.Value;
        _organizationsAppService = organizationsAppService;
    }

    #region Public Methods
    /// <inheritdoc/>

    public async Task<Tenant> GetTenantByNameDtoAsync(string name)
    {
        var query = await _tenantRepository.GetQueryableAsync();
        query = query.Where(t => t.Name == name);
        var tenant = await AsyncExecuter.FirstOrDefaultAsync(query);
        return tenant;

    }
    /// <inheritdoc/>

    public async Task<UserDataDto> UpdateUser(string phone, string password)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == CurrentUser.Id);

        if (user == null)
        {
            throw new ValidationException(LocalizationHelper.GetLocalizedString("User") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
        }
        user.SetPhoneNumber(phone, true);
        var removePasswordResult = await _userManager.RemovePasswordAsync(user);
        if (!removePasswordResult.Succeeded)
        {
            throw new ValidationException(LocalizationHelper.GetLocalizedString("TryAgainException"));
        }

        var addPasswordResult = await _userManager.AddPasswordAsync(user, password);
        if (!addPasswordResult.Succeeded)
        {
            throw new ValidationException(LocalizationHelper.GetLocalizedString("TryAgainException"));
        }

        return new UserDataDto
        {
            FullName = user.Name,
            Email = user.Email,
            Phone = user.PhoneNumber,
            Image = user.GetProperty<string>("Image"),
            IdentificationTypes = user.GetProperty<IdentificationTypes>("IdentificationTypes").ToString(),
            NationalId = user.GetProperty<string>("NationalId")
        };
    }
    /// <inheritdoc/>
    public async Task<Result<forgetpasswordDto>> ForgetPasswordAsync(string phone)
    {
        try
        {
            var user = await _authenticationUtility.CheckPhoneISFound(phone);
            return Result<forgetpasswordDto>.Success(new forgetpasswordDto
            {
                UserId = user.Id.ToString(),
            });
        }
        catch (Exception ex)
        {
            return Result<forgetpasswordDto>.Error(errorMessages: ex.Message);
        }

    }



    /// <inheritdoc/>
    public async Task<Result<UserDataDto>> GetUserDataAsync()
    {


        var userName = CurrentUser.UserName;
        if (userName.IsNullOrEmpty())
        {
            return Result<UserDataDto>.NotFound();
        }
        var user = await _userManager.FindByNameAsync(userName);
        if (user is not null)
        {
            return Result<UserDataDto>.Success(new UserDataDto
            {
                FullName = user.Name,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Image = $"{user.GetProperty<string>("Image")}",
                IdentificationTypes = user.GetProperty<IdentificationTypes>("IdentificationTypes").ToString(),
                NationalId = user.GetProperty<string>("NationalId")
            });
        }
        return Result<UserDataDto>.NotFound();

    }
    /// <inheritdoc/>
    public async Task<Result<LoginResultDto>> LoginAsync(LoginMobileInputDto input)
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {
            try
            {
                // Step 1: Validate the user's credentials
                var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == input.PhoneNumber);
                if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password))
                {
                    return Result<LoginResultDto>.Invalid(new Results.ValidationError
                    {
                        ErrorCode = "403",
                        ErrorMessage = "Invalid credentials."
                    });

                }
                if (user.IsActive == false)
                {
                    return Result<LoginResultDto>.Invalid(new Results.ValidationError
                    {
                        ErrorCode = "403",
                        ErrorMessage = "Invalid credentials."
                    });
                }
                if (!user.PhoneNumberConfirmed)
                {
                    return Result<LoginResultDto>.Invalid(new Results.ValidationError
                    {
                        ErrorCode = "423",
                        ErrorMessage = "Verfiy phone number"
                    });
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                CurrentTenant.Change(user.TenantId);

                var roles = await _userManager.GetRolesAsync(user);
                return Result<LoginResultDto>.Success(new LoginResultDto
                {
                    AccessToken = await _authenticationUtility.GenerateTokenAsync(user) ?? string.Empty,
                    Role = roles.FirstOrDefault(),
                    Baseurl = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}",
                    TenantId = CurrentTenant.Id.ToString()
                });
            }
            catch (Exception ex)
            {
                return Result<LoginResultDto>.Invalid(new Results.ValidationError
                {
                    ErrorCode = "403",
                    ErrorMessage = "Invalid credentials."
                });
            }
        }


    }
    /// <inheritdoc/>

    public async Task<Result<LoginAppResultDto>> LoginAppAsync(LoginMobileInputDto input)
    {
        var LoginResult = await LoginAsync(input);
        var LoginAppResultDto = new LoginAppResultDto();

        if (LoginResult.IsSuccess)
        {
            CurrentTenant.Change( Guid.Parse(LoginResult.Value.TenantId));
            var tenant = await _tenantRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(LoginResult.Value.TenantId));
            LoginAppResultDto.TenantId = LoginResult.Value.TenantId;
            LoginAppResultDto.AccessToken = LoginResult.Value.AccessToken;
            LoginAppResultDto.Role = LoginResult.Value.Role;
            var organization = await _organizationsAppService.GetOrganizationIdByTenant(new Guid(LoginResult.Value.TenantId));
            LoginAppResultDto.Baseurl = $"{_httpContext.HttpContext.Request.Scheme}:/{tenant.Name}/{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}";
            LoginAppResultDto.OrganizationId = organization;
            LoginAppResultDto.ImageBase64 = await _organizationsAppService.GetOrganizationPictureByOrganizationTenant(new Guid(LoginResult.Value.TenantId));
            return Result<LoginAppResultDto>.Success(LoginAppResultDto);
        }
        return Result<LoginAppResultDto>.Invalid(LoginResult.ValidationErrors);
    }
   [RemoteService(IsEnabled = false)]
    /// <inheritdoc/>

    public async Task<string> GenerateToken(string userId)
    {
        await Task.Yield();
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKeyBytes = Encoding.UTF8.GetBytes("6eb044cb72029502b56bb72f9dff6dfac055ff6e5e2ce088969c83e9f35e2504");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("userId", userId) }),
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKeyBytes),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    /// <inheritdoc/>
    [RemoteService(IsEnabled = false)]
    public async Task<IdentityUser> LoginForWebAsync(LoginInputDto input)
    {
        IdentityUser? user;

        if (CurrentTenant.IsAvailable)
        {
            user = await FindUserAsync(input);
        }
        else
        {
            using (DataFilter.Disable<IMultiTenant>())
            {
                user = await FindUserAsync(input);
            }
        }

        if (user == null)
            throw new Exception(LocalizationHelper.GetLocalizedString("InvalidCredentials"));

        await CheckOrganization(user);

        await CheckRole(user);

        await CheckPassword(input, user);


        return user;
    }

   

    /// <inheritdoc/>
    public async Task<Result<RegisterResultDto>> RegisterAsync(RegisterInputDto input)
    {

        try
        {

            var user = ObjectMapper.Map<RegisterInputDto, UserCreateDto>(input);
            var userAdded = await _usersAppService.AddAndValidateUserAsync(user);
            await _usersAppService.CreateCustomerWithRoleAsync(userAdded, user);

            var AccessToken = await _authenticationUtility.GenerateTokenAsync(userAdded) ?? string.Empty;
            return Result<RegisterResultDto>.Success(new RegisterResultDto
            {
                AccessToken = AccessToken,
            });
        }
        catch (AbpValidationException validationException)
        {
            return Result<RegisterResultDto>.Error(validationException?.ValidationErrors?.FirstOrDefault()?.MemberNames.First() + " - " + validationException?.ValidationErrors?.FirstOrDefault()?.ErrorMessage);
        }
        catch (BusinessException ex)
        {

            return Result<RegisterResultDto>.Error(ex.Message);
        }
        catch (AuthenticationException ex)
        {
            return Result<RegisterResultDto>.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<RegisterResultDto>.Error(ex.Message);

        }



    }

    /// <inheritdoc/>

    public async Task<Result> SetNewPassword(ChangePasswordInputDto input)
    {

        var username = CurrentUser.UserName;
        var user = await _userManager.FindByNameAsync(username);

        await _userManager.RemovePasswordAsync(user);
        var password = await _userManager.AddPasswordAsync(user, input.NewPassword);
        user.SetConcurrencyStampIfNotNull(user.ConcurrencyStamp);
        var Newuser = await _userManager.UpdateAsync(user);

        if (Newuser.Succeeded)
        {
            return Result.SuccessWithMessage("Set New Password Successfully");

        }

        return Result.Invalid(new Results.ValidationError
        {
            ErrorMessage = "Incorrect password."
        });

    }
    /// <inheritdoc/>

    public async Task<bool> SetNewPasswordWeb(string UserName, string NewPassword)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.UserName == UserName || x.PhoneNumber == UserName);

            if (user == null)
            {
                throw new Exception("user isn't exist");
            }
            await _userManager.RemovePasswordAsync(user);
            var password = await _userManager.AddPasswordAsync(user, NewPassword);
            if (password.Succeeded)
            {
                user.SetConcurrencyStampIfNotNull(user.ConcurrencyStamp);
                await _userManager.UpdateAsync(user);
                return true;
            }
            else
            {
                return false;
            }
        }

    }


    /// <inheritdoc/>
    public async Task LogOutAsync()
    {
        await _signInManager.SignOutAsync();
    }
    /// <inheritdoc/>

    public async Task<Tenant> GetTenantByPhoneNumberDtoAsync(string phoneNum)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNum);
            var query = await _tenantRepository.GetQueryableAsync();
            query = query.Where(t => t.Id == user.TenantId);
            var tenant = await AsyncExecuter.FirstOrDefaultAsync(query);
            return tenant;
        }

    }
    /// <inheritdoc/>

    public async Task<bool> GenerateOTPForUser(string UserName)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.UserName == UserName || x.PhoneNumber == UserName);
            if (user == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("User") + LocalizationHelper.GetLocalizedString("NotFound"));
            }
            var OTP = GenrateOTP();
            var result = await SendSms(user.PhoneNumber, $"{OTP} رمز التحقق لدخول نظام مدخل هو ");
            user.SetProperty("OtpCode", OTP);
            user.SetProperty("OtpExpirationDate", DateTime.Now.AddMinutes(2));
            await _userManager.UpdateAsync(user);
            return result;
        }
    }
    /// <inheritdoc/>

    public async Task<bool> SendSms(string phoneNumber, string message)
    {

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");

        var payload = new
        {
            recipients = new[] { phoneNumber },
            body = message,
            sender = _options.SenderName
        };

        var jsonPayload = JsonSerializer.Serialize(payload);

        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_options.ApiUrl, content);

        return response.IsSuccessStatusCode;

    }
    /// <inheritdoc/>

    public async Task<bool> VerifyOtp(string UserName, string Otp)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.UserName == UserName || x.PhoneNumber == UserName);
            if (user == null)
            {
                throw new Exception("user isn't exist");
            }
            var result = CheckOtp(Otp, user.GetProperty<string>("OtpCode"), user.GetProperty<DateTime>("OtpExpirationDate"));
            return result;
        }
    }

    /// <inheritdoc/>

    public async Task<Guid> GetCurrentCustomerId()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var customer = await _customerRepository.GetQueryableAsync();
            var customerId = customer.Where(s => s.IdentityUserId == CurrentUser.Id).Select(s => s.Id).FirstOrDefault();
            return customerId;
        }

    }
    /// <inheritdoc/>

    public async Task<IdentityUser?> IsUserFoundAsync(string Id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await _userManager.FindByIdAsync(Id);
        }
    }
    /// <inheritdoc/>

    public async Task<string> GetUserNameByPhoneNumber(string phone)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.PhoneNumber == phone || x.UserName == phone);
            if (user == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("User") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }
            return user.UserName;
        }
    }
    #endregion
    #region Private Methods
    /// <summary>
    /// Generates a 4-digit One-Time Password (OTP).
    /// </summary>
    /// <returns>A string representing the generated OTP.</returns>
    private string GenrateOTP()
    {
        int min = 1000;
        int max = 9999;
        Random random = new Random();
        return random.Next(min, max).ToString();
    }
    /// <summary>
    /// Finds a user based on the provided login input, searching by either username or phone number.
    /// </summary>
    /// <param name="input">The login input containing the username or phone number to search for.</param>
    /// <returns>The found <see cref="IdentityUser"/> if found, otherwise null.</returns>
    private async Task<IdentityUser?> FindUserAsync(LoginInputDto input)
    {
        return await _userRepository.FirstOrDefaultAsync(u =>
            u.UserName == input.Username || u.PhoneNumber == input.Username);
    }
    /// <summary>
    /// Checks if the provided OTP is valid by comparing it with the current OTP and ensuring it is not expired.
    /// </summary>
    /// <param name="Otp">The OTP entered by the user.</param>
    /// <param name="CurrentOtp">The current OTP stored for the user.</param>
    /// <param name="ExpiryTime">The expiration time of the OTP.</param>
    /// <returns>True if the OTP matches and has not expired; otherwise, false.</returns>
    private bool CheckOtp(string Otp, string CurrentOtp, DateTime ExpiryTime)
    {
        if (CurrentOtp == Otp && ExpiryTime >= DateTime.Now)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Verifies the password of the user.
    /// Throws an exception if the user does not exist or if the password is incorrect or if the user is inactive.
    /// </summary>
    /// <param name="input">The login input containing the password to check.</param>
    /// <param name="user">The <see cref="IdentityUser"/> whose password is being verified.</param>
    /// <exception cref="Exception">Thrown if the password is incorrect or the user is inactive.</exception>
    private async Task CheckPassword(LoginInputDto input, IdentityUser user)
    {
        if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password))
            throw new Exception(LocalizationHelper.GetLocalizedString("InvalidCredentials"));
        if (user.IsActive == false)
            throw new Exception(LocalizationHelper.GetLocalizedString("User") + " " + LocalizationHelper.GetLocalizedString("InActive"));
        return;
    }
    /// <summary>
    /// Verifies if the user's role is allowed. Throws an exception if the user is a "Customer" and the only role.
    /// </summary>
    /// <param name="user">The <see cref="IdentityUser"/> whose roles are being checked.</param>
    /// <exception cref="Exception">Thrown if the user has a role of "Customer" and no other roles.</exception>
    private async Task CheckRole(IdentityUser user)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Customer") && roles.Count == 1)
                throw new Exception(LocalizationHelper.GetLocalizedString("Customer") + " " + LocalizationHelper.GetLocalizedString("NotAllowed"));
            return;
        }

    }
    /// <summary>
    /// Verifies if the organization associated with the user is active and valid. Throws an exception if the organization is inactive or not found.
    /// </summary>
    /// <param name="user">The <see cref="IdentityUser"/> whose organization is being checked.</param>
    /// <exception cref="Exception">Thrown if the organization is inactive or not found.</exception>
    private async Task CheckOrganization(IdentityUser user)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var organizationId = user.GetProperty<int>("OrganizationId");

            if (organizationId == 0) return;

            var organization = await _organizationRepository.FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null || organization.Status == Enums.Status.Inactive || organization.Status == Enums.Status.Deleted)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString(
                    (AlertMessages.AlertMessages.FAILED) + " : " + LocalizationHelper.GetLocalizedString("Organization") + " " +
                    LocalizationHelper.GetLocalizedString("InActive") + " , " +
                    LocalizationHelper.GetLocalizedString("Organization") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation")));
            }

            return;
        }

    }
    #endregion
}
