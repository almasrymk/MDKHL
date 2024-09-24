using SmartParking.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Cutomers;

public interface ICustomerAppService : ICrudAppService<CustomerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCustomerDto>
{
   Task<CustomerDto> RegisterAsync(CreateUpdateCustomerDto input);
    Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input);
   string GenerateOtp();


   //Task<bool> SendOtpAsync(string mobileNumber);



   Task<bool> VerifyOtpAsync(OtpVerificationDto input);
}
