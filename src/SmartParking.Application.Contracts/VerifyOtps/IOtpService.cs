using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.VerifyOtps;

public interface IOtpService : IApplicationService
{
    public Task<string> GenerateOtpAsync(string mobileNumber);
    public Task<bool> VerifyOtpAsync(string mobileNumber, string otp);


}
