using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Mobile.Otps;

[RemoteService(IsEnabled = false)]

public class OtpAppService : ApplicationService, IOtpAppService
{
    private readonly Random _random;
    private const int OtpLength = 4;
    private const int OtpValiditySecounds = 60;
    private const int OtpValidityMinutes = 5;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;
    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;
    private readonly IOptionsMonitor<OpenIddictServerOptions> _openIddictOptions;
    private readonly IHttpContextAccessor _httpContext;
    private readonly HttpClient _httpClient;
    private readonly IDataFilter _dataFilter;
    private readonly TaqnyatSmsOptionsDto _options;

    public OtpAppService(IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository,
        UserManager<Volo.Abp.Identity.IdentityUser> userManager,
        IOptionsMonitor<OpenIddictServerOptions> openIddictOptions,
        IHttpContextAccessor httpContext,
        IOptions<TaqnyatSmsOptionsDto> options,
        HttpClient httpClient,
        IDataFilter dataFilter
        )
    {
        //_userManager = userManager;
        _random = new Random();
        _userRepository = userRepository;
        _openIddictOptions = openIddictOptions;
        _userManager = userManager;
        _httpContext = httpContext;
        _options = options.Value;
        _httpClient = httpClient;
        _dataFilter = dataFilter;
    }

    /// <inheritdoc/>
    public async Task<Result<string>> GenerateOtpAsync(string phoneNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            try
            {
                if (phoneNumber is null)
                {
                    return Result.Invalid(new Results.ValidationError
                    {
                        ErrorMessage = "Phone Number is not valid"
                    });
                }
                var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
                if (user is null)
                {
                    return Result.Invalid(new Results.ValidationError
                    {
                        ErrorMessage = "user isnot Found"
                    });
                }

                var otp = GenerateRandomOtp(); // call GenerateRandomOtp function   

                user?.SetProperty("OtpCode", otp);

                user?.SetProperty("OtpExpirationDate", DateTime.Now.AddMinutes(OtpValidityMinutes));

                await SendSms(phoneNumber, "رمز التحقق لدخول نظام مدخل هو " + otp);
                // Update the user with OTP in the database
                await _userRepository.UpdateAsync(user);

                return Result.Success(" رمز التحقق لدخول نظام مدخل هو" + otp);
            }
            catch (Exception ex)
            {
                return Result.Invalid(new Results.ValidationError
                {
                    ErrorMessage = ex.Message
                });
            }
        }

    }

    /// <inheritdoc/>
    public async Task<Result<VerifyOtpDto>> VerifyOtpAsync(string phoneNumber, string otp)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user is null)
        {
            return Result.Invalid(new Results.ValidationError
            {
                ErrorMessage = "User Not Found"
            });
        }

        var storedOtp = user?.GetProperty<string>("OtpCode");

        var otpExpirationDate = user?.GetProperty<DateTime>("OtpExpirationDate");

        var currentTime = DateTime.Now;

        // Check if the provided OTP matches the stored OTP and if the OTP has not expired
        if (storedOtp == otp && otpExpirationDate >= currentTime)
        {
            // OTP is valid, clear OTP fields
            user.SetPhoneNumberConfirmed(true);
            user?.SetProperty("OtpCode", null);
            user?.SetProperty("OtpExpirationDate", DateTime.Now);
            await _userRepository.UpdateAsync(user);

            // Return success result with true
            return Result.Success(
            new VerifyOtpDto
            {
                AccessToken = await GenerateTokenAsync(user) ?? null,
            }, "Verfiy Otp Successfully");
        }
        else
        {
            return Result.Invalid(new Results.ValidationError
            {
                ErrorMessage = "Verfiy Otp Failed"
            });
        }
        // If OTP is invalid or expired, return false
    }

    private string GenerateRandomOtp()
    {
        return _random.Next(0, (int)Math.Pow(10, OtpLength)).ToString($"D{OtpLength}");
    }

    /// <inheritdoc/>
    public async Task<Result<string>> GenerateOtpByUserIdAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == CurrentUser.Id);
            if (user is null)
            {
                return Result.Invalid(new Results.ValidationError
                {
                    ErrorMessage = "Login first"
                });
            }

            var otp = GenerateRandomOtp(); // call GenerateRandomOtp function   
            user.SetProperty("OtpCode", otp);
            user?.SetProperty("OtpExpirationDate", DateTime.Now.AddMinutes(OtpValidityMinutes));
            var phonenumber = user?.PhoneNumber;
            await SendSms(phonenumber, "رمز التحقق لدخول نظام مدخل هو " + otp);
            // Update the user with OTP in the database
            await _userRepository.UpdateAsync(user);

            // Update the user with OTP in the database
            await _userRepository.UpdateAsync(user);

            return Result.Success(otp);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<string>> ForgetPasswordOtpAsync(string phoneNumber)
    {

        var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user is null)
        {
            return Result.Invalid(new Results.ValidationError
            {
                ErrorMessage = "User Not Found"
            });
        }
        var otp = "1234"; // call GenerateRandomOtp function   
        user?.SetProperty("OtpCode", otp);
        user?.SetProperty("OtpExpirationDate", DateTime.Now.AddMinutes(OtpValidityMinutes));

        // Update the user with OTP in the database
        await _userRepository.UpdateAsync(user);

        return Result.Success(otp, "Otp Generated Successfully");

    }

    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the token will be generated.</param>
    /// <returns>A string representing the generated JWT token.</returns>
    private async Task<string> GenerateTokenAsync(Volo.Abp.Identity.IdentityUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>()
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("given_name", user.UserName),
            new Claim("email", user.Email),
            new Claim("role", string.Join(",", roles)),
            new Claim("tenantid", user.TenantId?.ToString() ?? ""),
            new Claim("scope", "address email phone roles profile offline_access SmartParking") //replace Qa with yours

        };
        var options = _openIddictOptions.CurrentValue;
        var descriptor = new SecurityTokenDescriptor
        {
            Audience = "SmartParking", // replace with yours,
            EncryptingCredentials = options.DisableAccessTokenEncryption
                ? null
                : options.EncryptionCredentials.First(),
            Expires = null,
            Subject = new ClaimsIdentity(claims, TokenValidationParameters.DefaultAuthenticationType),
            IssuedAt = DateTime.UtcNow,
            Issuer = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}{_httpContext.HttpContext.Request.PathBase}", // replace with yours,
            SigningCredentials = options.SigningCredentials.First(),
            TokenType = OpenIddictConstants.JsonWebTokenTypes.AccessToken,
        };

        var accessToken = options.JsonWebTokenHandler.CreateToken(descriptor);

        return accessToken;
    }

    /// <summary>
    /// function that send sms 
    /// </summary>
    /// <param name="phoneNumber">phoneNumber of user</param>
    /// <param name="message">messgae that need to send</param>
    /// <returns>bool</returns>
    private async Task<bool> SendSms(string phoneNumber, string message)
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
}
