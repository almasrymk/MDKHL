using Microsoft.EntityFrameworkCore;
using SmartParking.Results;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Mobile.Otps;

[RemoteService(IsEnabled = false)]

public class OtpAppService : ApplicationService, IOtpAppService
{
    private readonly Random _random;
    private const int OtpLength = 4;
    private const int OtpValiditySecounds = 40;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;

    public OtpAppService(IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository)
    {
        //_userManager = userManager;
        _random = new Random();
        _userRepository = userRepository;

    }
    public async Task<Result<string>> GenerateOtpAsync(string phoneNumber)
    {
        try
        {
            if (phoneNumber is null)
            {
                return Result.Invalid(new ValidationError
                {
                    ErrorMessage = "Phone Number is not valid"
                });
            }
            var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user is null)
            {
                return Result.Invalid(new ValidationError
                {
                    ErrorMessage = "user isnot Found"
                });
            }

            var otp = "1234"; // call GenerateRandomOtp function   
            user?.SetProperty("OtpCode", otp);
            user?.SetProperty("OtpExpirationDate", DateTime.Now.AddSeconds(OtpValiditySecounds));

            // Update the user with OTP in the database
            await _userRepository.UpdateAsync(user);

            return Result.Success(otp);
        }
        catch
        {
            return Result.Invalid(new ValidationError
            {
                ErrorMessage = "An error occurred while processing the request."
            });
        }
       
    }

    public async Task<Result<bool>> VerifyOtpAsync(string phoneNumber, string otp)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user is null)
        {
            return Result.Invalid(new ValidationError
                   {
                        ErrorMessage = "user isnot Found"
                   });
        }

        var storedOtp = user?.GetProperty<string>("OtpCode");
        var otpExpirationDate = user?.GetProperty<DateTime>("OtpExpirationDate");
        bool flag = false;
        var currentTime= DateTime.Now;
        // Check if the provided OTP matches the stored OTP and if the OTP has not expired
        if (storedOtp == otp && otpExpirationDate >= currentTime   )
        {
            // OTP is valid, clear OTP fields
            user?.SetProperty("OtpCode", null);
            user?.SetProperty("OtpExpirationDate", DateTime.Now);
            await _userRepository.UpdateAsync(user);

            // Return success result with true
            flag = true;
        }

        // If OTP is invalid or expired, return false
        return flag == true ?Result.Success(true):Result.Success(false);
    }

    private string GenerateRandomOtp()
    {
        return _random.Next(0, (int)Math.Pow(10, OtpLength)).ToString($"D{OtpLength}");
    }


    public async Task<Result<string>> GenerateOtpByUserIdAsync(Guid userId)
    {
        if (userId.ToString() is null)
        {
           return Result.Invalid(new ValidationError
                  {
                    ErrorMessage = "User Id Not valid"
                  });
        }
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return Result.Invalid(new ValidationError
                   {
                        ErrorMessage = "User Not Found"
                   });
        }

        var otp = "1234"; // call GenerateRandomOtp function   
        user?.SetProperty("OtpCode", otp);
        user?.SetProperty("OtpExpirationDate", DateTime.Now.AddSeconds(OtpValiditySecounds));

        // Update the user with OTP in the database
        await _userRepository.UpdateAsync(user);

        return Result.Success(otp);
    }

    public async Task<Result<ForgetPasswordOtpDto>> ForgetPasswordOtpAsync(Guid userId,string otp)
    {

        var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return Result.Invalid(new ValidationError
            {
                        ErrorMessage = "User Not Found"
            });
        }
        var storedOtp = user?.GetProperty<string>("OtpCode");
        var otpExpirationDate = user?.GetProperty<DateTime>("OtpExpirationDate");
        var currentTime = DateTime.Now;

        if (storedOtp == otp && otpExpirationDate >= currentTime)
        {
            // OTP is valid, clear OTP fields
            user?.SetProperty("OtpCode", null);
            user?.SetProperty("OtpExpirationDate", DateTime.Now);
            await _userRepository.UpdateAsync(user);

        }
        // Update the user with OTP in the database
        await _userRepository.UpdateAsync(user);

        return Result.Success(
        new ForgetPasswordOtpDto
        {
            UserId = user.Id,
        });

    }
}
