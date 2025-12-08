using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Server;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using static QRCoder.PayloadGenerator;

namespace SmartParking.Mobile.Otps;

[RemoteService(IsEnabled = false)]

public class OtpAppService : ApplicationService, IOtpAppService
{
    private readonly Random _random = new Random();
    private const int OtpLength = 4;
    private const int OtpValidityMinutes = 5;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;
    private readonly ILogger<OtpAppService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IDataFilter _dataFilter;

    private readonly IAuthenticationUtility _authenticationUtility;
    private readonly TaqnyatSmsOptionsDto _options;

    public OtpAppService(IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository,
        IOptions<TaqnyatSmsOptionsDto> options,
        HttpClient httpClient,
        IDataFilter dataFilter,
        IAuthenticationUtility authenticationUtility
,
        ILogger<OtpAppService> logger)
    {
        _userRepository = userRepository;
        _options = options.Value;
        _httpClient = httpClient;
        _dataFilter = dataFilter;
        _authenticationUtility = authenticationUtility;
        _logger = logger;
    }

    #region Public Methods
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
                _logger.LogInformation($"User Is found and Otp is set {user?.GetProperty<string>("OtpCode")} with expiration date is {user?.GetProperty<DateTime>("OtpExpirationDate")}");
                var result = await SendSms(phoneNumber, "رمز التحقق لنظام مدخل الخاص بشركة رواد المدينة هو " + otp);
                _logger.LogInformation($"Request Has been sent to teqnyat ? {result}");

                // Update the user with OTP in the database
                await _userRepository.UpdateAsync(user);

                return Result.Success("رمز التحقق لنظام مدخل الخاص بشركة رواد المدينة هو" + otp);
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
        using (_dataFilter.Disable<IMultiTenant>())
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
                    AccessToken =  await _authenticationUtility.GenerateTokenAsync(user) ?? string.Empty,
                }, "Verfiy Otp Successfully");
            }
            else
            {
                return Result.Invalid(new Results.ValidationError
                {
                    ErrorMessage = "Verfiy Otp Failed"
                });
            }
        }
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
            _logger.LogInformation($"User Is found and Otp is set {user?.GetProperty<string>("OtpCode")} with expiration date is {user?.GetProperty<DateTime>("OtpExpirationDate")}");
            var result = await SendSms(phonenumber, "رمز التحقق لنظام مدخل الخاص بشركة رواد المدينة هو " + otp);
            _logger.LogInformation($"Request Has been sent to teqnyat ? {result}");
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
    #endregion 
    #region Private Methods

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
        _logger.LogInformation($"message that will be sent to teqnyat is {message} with sender Name {_options.SenderName} and url {_options.ApiUrl} ");
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_options.ApiUrl, content);
        string responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogInformation($"teqnyat response is {responseBody}");
        return response.IsSuccessStatusCode;
    }
    private string GenerateRandomOtp()
    {
        return _random.Next(0, (int)Math.Pow(10, OtpLength)).ToString($"D{OtpLength}");
    }
    #endregion

}
