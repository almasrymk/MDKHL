using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartParking.VerifyOtps;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.VerfiyOtps;

public class OtpService : ApplicationService,IOtpService
{
    private readonly Random _random;
    private const int OtpLength = 4;
    private const int OtpValiditySecounds = 40;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;

    public OtpService(IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository)
    {
        //_userManager = userManager;
        _random = new Random();
        _userRepository = userRepository;

    }
    public async Task<string> GenerateOtpAsync(string phoneNumber)
    {
        if(phoneNumber is null)
        {
            throw new UserFriendlyException("Phone Number Not valid");
        }
        var user = await _userRepository.FirstOrDefaultAsync(u=>u.PhoneNumber == phoneNumber);
        if (user == null)
        {
            throw new UserFriendlyException("User not found.");
        }

        var otp = "1234"; // call GenerateRandomOtp function   
        user.SetProperty("OtpCode",otp);
        user.SetProperty("OtpExpirationDate", DateTime.UtcNow.AddSeconds(OtpValiditySecounds));

        // Update the user with OTP in the database
        await _userRepository.UpdateAsync(user);

        return otp ;  
    }

    public async Task<bool> VerifyOtpAsync(string phoneNumber, string otp)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user == null)
        {
            throw new UserFriendlyException("User not found.");
        }

        if (user.GetProperty<string>("OtpCode") == otp && user.GetProperty<DateTime>("OtpExpirationDate") >= DateTime.UtcNow)
        {
            // OTP is valid, clear OTP fields
            user.SetProperty("OtpCode", null);
            user.SetProperty("OtpExpirationDate", new DateTime());
            await _userRepository.UpdateAsync(user);

            return true;
        }

        return false;
    }

    private string GenerateRandomOtp()
    {
        return _random.Next(0, (int)Math.Pow(10, OtpLength)).ToString($"D{OtpLength}");
    }
}
