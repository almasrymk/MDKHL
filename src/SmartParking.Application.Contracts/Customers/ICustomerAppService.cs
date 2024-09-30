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

    /// <summary>
    /// Method to add customer to database
    /// It depends on UserManager to add identity user to User Table
    /// User Manager confirms that user has SecurityStamp
    /// Calls repository to add customer to databas
    /// </summary>
    /// <param name="input">Customer create dto</param>
    /// <returns>A dto for created customer</returns>
    /// <exception cref="UserFriendlyException">Throws exception when error occurs while saving customer</exception>
    Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input);

    /// <summary>
    /// A method used to register customer
    /// Method uses create customer method to create customer
    /// </summary>
    /// <param name="input">Customer create dto</param>
    /// <returns>A dto for created customer</returns>
    Task<CustomerDto> RegisterAsync(CreateUpdateCustomerDto input);
    /// <summary>
    /// Method Used to create random otp
    /// </summary>
    /// <returns>string of generated otp</returns>
    string GenerateOtp();



    //Task<bool> SendOtpAsync(string mobileNumber);


    /// <summary>
    /// Method used to verify user otp
    /// </summary>
    /// <param name="input">Otp dto</param>
    /// <returns>boolean depends on operation status</returns>
    /// <exception cref="NotImplementedException">Throws exception if no user with that id in database</exception>    /// <inheritdoc/>

    Task<bool> VerifyOtpAsync(OtpVerificationDto input);
}
