using SmartParking.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Cutomers;

public interface ICustomerAppService : ICrudAppService<Customer, CustomerDto, Guid, PagedAndSortedResultRequestDto, RegisterCustomerDto>
{
    Task<CustomerDto> RegisterAsync(RegisterCustomerDto input);
    Task<CustomerDto> LoginAsync(LoginCustomerDto input);
    //Task<bool> SendOtpAsync(string mobileNumber);
    //Task<bool> VerifyOtpAsync(OtpVerificationDto input);
    //Task ResetPasswordAsync(ResetPasswordDto input);
}
