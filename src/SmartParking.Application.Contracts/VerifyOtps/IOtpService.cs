using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.VerifyOtps;

public interface IOtpService : IApplicationService
{
    public Task<string> GenerateOtpAsync(string phoneNumber);
    public Task<Result<string>> GenerateOtpByUserIdAsync(Guid userId);
    public Task<bool> VerifyOtpAsync(string phoneNumber, string otp);
    public Task<Result<ForgetPasswordOtpDto>> ForgetPasswordOtpAsync(Guid userId);



}
