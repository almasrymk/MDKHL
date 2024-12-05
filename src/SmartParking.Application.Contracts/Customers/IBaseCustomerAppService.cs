using AutoMapper.Internal.Mappers;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp;

namespace SmartParking.Customers;

public interface IBaseCustomerAppService
{
    /// <summary>
    /// Asynchronously checks if the specified user exists and returns the associated customer data if found.
    /// </summary>
    /// <param name="user">The <see cref="Volo.Abp.Identity.IdentityUser"/> object representing the user to check.</param>
    /// <returns>
    /// A <see cref="CustomerDto"/> object containing customer details if the user is found;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method queries the customer information based on the provided user.
    /// If the user does not exist, it returns <c>null</c>.
    /// </remarks>
   Task<CustomerDto> UserIsFound(Volo.Abp.Identity.IdentityUser? user);

    /// <summary>
    /// Asynchronously checks the validity of a customer based on the provided input data.
    /// </summary>
    /// <param name="input">The data transfer object containing customer details to be validated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CheckCustomer(CreateUpdateCustomerDto input);

    /// <summary>
    /// Asynchronously adds a customer package for a specified organization.
    /// </summary>
    /// <param name="organization">The organization to which the customer package will be added.</param>
    /// <param name="customerAdd">The customer entity containing details for the package to be added.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddCustomerPackage(int organizationId, Guid customerId, int packageId);

    /// <summary>
    /// Method Used to create random otp
    /// </summary>
    /// <returns>string of generated otp</returns>
    Task<string> GenerateOtp(Guid CustomerId);

    /// <summary>
    /// Method used to verify user otp
    /// </summary>
    /// <param name="input">Otp dto</param>
    /// <returns>boolean depends on operation status</returns>
    /// <exception cref="NotImplementedException">Throws exception if no user with that id in database</exception>    /// <inheritdoc/>

    Task<bool> VerifyOtpAsync(OtpVerificationDto input);

    /// <summary>
    /// Method used to check if phone number is uniquer per user.
    /// </summary>
    /// <param name="phoneNumber">Required to check phone number</param>
    /// <returns>bool</returns>

    Task<bool> CheckIfPhoneNumberIsUnique(string phoneNumber);

    /// <summary>
    /// Methods that checks if national id is not duplicate.
    /// </summary>
    /// <param name="NationalId">NationalId of customer</param>
    /// <returns>Boolean that indicates the existance of National Id</returns>
    Task<bool> CheckIfNationalIDIsUnique(string NationalId);

    /// <summary>
    /// Validates whether the specified email address is in a correct format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>
    /// <c>true</c> if the email address is valid; otherwise, <c>false</c>.
    /// </returns>
    bool IsEmailValid(string email);
  
}
