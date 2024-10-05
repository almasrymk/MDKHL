using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartParking.VerifyOtps;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;

namespace SmartParking.VerfiyOtps;

public class OtpService : ApplicationService,IOtpService
{
    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;
    private readonly Random _random;
    private const int OtpLength = 4;
    private const int OtpValiditySecounds = 40;
    public OtpService(UserManager<Volo.Abp.Identity.IdentityUser> userManager)
    {
        _userManager = userManager;
        _random = new Random();
    }
    public async Task<string> GenerateOtpAsync(string mobileNumber)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(m=>m.PhoneNumber == mobileNumber);
        if (user == null)
        {
            throw new UserFriendlyException("User not found.");
        }

        var otp = GenerateRandomOtp();
        user.SetProperty("OtpCode",otp);
        user.SetProperty("OtpExpirationDate", DateTime.UtcNow.AddSeconds(OtpValiditySecounds));

        // Update the user with OTP in the database
        await _userManager.UpdateAsync(user);

        return "1234";
    }

    public async Task<bool> VerifyOtpAsync(string mobileNumber, string otp)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == mobileNumber);
        if (user == null)
        {
            throw new UserFriendlyException("User not found.");
        }

        if (user.GetProperty<string>("OtpCode") == otp && user.GetProperty<DateTime>("OtpExpirationDate") >= DateTime.UtcNow)
        {
            // OTP is valid, clear OTP fields
            user.SetProperty("OtpCode", null);
            user.SetProperty("OtpExpirationDate", new DateTime());
            await _userManager.UpdateAsync(user);

            return true;
        }

        return false;
    }

    private string GenerateRandomOtp()
    {
        return _random.Next(0, (int)Math.Pow(10, OtpLength)).ToString($"D{OtpLength}");
    }
}
