using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartParking.Results;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Mobile.Otps;

public class OtpAppService : ApplicationService, IOtpService
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
        if (phoneNumber is null)
        {
            Result.Invalid(new ValidationError
            {
                ErrorMessage = "Phone Number is not valid"
            });
        }
        var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user is null)
        {
            Result.Invalid(new ValidationError
            {
                ErrorMessage = "user isn ot Found"
            });
        }

        var otp = "1234"; // call GenerateRandomOtp function   
        user?.SetProperty("OtpCode", otp);
        user?.SetProperty("OtpExpirationDate", DateTime.UtcNow.AddSeconds(OtpValiditySecounds));

        // Update the user with OTP in the database
        await _userRepository.UpdateAsync(user);

        return Result.Success(otp);
    }

    public async Task<Result<bool>> VerifyOtpAsync(string phoneNumber, string otp)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user is null)
        {
            Result.Invalid(new ValidationError
            {
                ErrorMessage = "user isn ot Found"
            });
        }

        if (user?.GetProperty<string>("OtpCode") == otp && user.GetProperty<DateTime>("OtpExpirationDate") >= DateTime.UtcNow)
        {
            // OTP is valid, clear OTP fields
            user.SetProperty("OtpCode", null);
            user.SetProperty("OtpExpirationDate", new DateTime());
            await _userRepository.UpdateAsync(user);

            return Result.Success(true);
        }

        return Result.Success(false);
    }

    private string GenerateRandomOtp()
    {
        return _random.Next(0, (int)Math.Pow(10, OtpLength)).ToString($"D{OtpLength}");
    }


    public async Task<Result<string>> GenerateOtpByUserIdAsync(Guid userId)
    {
        if (userId.ToString() is null)
        {
            Result.Invalid(new ValidationError
            {
                ErrorMessage = "User Id Not valid"
            });
        }
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            Result.Invalid(new ValidationError
            {
                ErrorMessage = "User Not Found"
            });
        }

        var otp = "1234"; // call GenerateRandomOtp function   
        user?.SetProperty("OtpCode", otp);
        user?.SetProperty("OtpExpirationDate", DateTime.UtcNow.AddSeconds(OtpValiditySecounds));

        // Update the user with OTP in the database
        await _userRepository.UpdateAsync(user);

        return Result.Success(otp);
    }

    public async Task<Result<ForgetPasswordOtpDto>> ForgetPasswordOtpAsync(Guid userId)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            Result.Invalid(new ValidationError
            {
                ErrorMessage = "User Not Found"
            });
        }

        var otp = "1234"; // call GenerateRandomOtp function   
        user?.SetProperty("OtpCode", otp);
        user?.SetProperty("OtpExpirationDate", DateTime.UtcNow.AddSeconds(OtpValiditySecounds));

        // Update the user with OTP in the database
        await _userRepository.UpdateAsync(user);

        return Result.Success(
        new ForgetPasswordOtpDto
        {
            UserId = user.Id,
        });

    }
}
