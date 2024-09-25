using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using SmartParking.Entities.Customers;
using SmartParking.Customers;
using Volo.Abp.Application.Dtos;
using SmartParking.Cutomers;
using System.Linq;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Identity;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
using Volo.Abp.Data;
using Abp.ObjectComparators.BooleanComparators;
using System.ComponentModel;

namespace SmartParking.Customers;

public class CustomerAppService : CrudAppService<Customer, CustomerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCustomerDto>, ICustomerAppService
{
    private readonly IRepository<Customer, Guid> _repository;
    private IIdentityUserAppService _identityUserAppService;
    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;


    public CustomerAppService(IRepository<Customer, Guid> repository,IIdentityUserAppService identityUserAppService , UserManager<Volo.Abp.Identity.IdentityUser> userManager) : base(repository)
    {
        _repository = repository;
        _identityUserAppService= identityUserAppService;
        _userManager=userManager;
    }
    /// <summary>
    /// A method used to register customer
    /// Method uses create customer method to create customer
    /// </summary>
    /// <param name="input">Customer create dto</param>
    /// <returns>A dto for created customer</returns>
    public async Task<CustomerDto> RegisterAsync(CreateUpdateCustomerDto input)
    {
             return await CreateAsync(input);
    }




    /// <summary>
    /// Method Used to create random otp
    /// </summary>
    /// <returns>string of generated otp</returns>

    public string GenerateOtp()
    {
        var random = new Random();
        return random.Next(1000, 9999).ToString();
    }



    /// <summary>
    /// Method to add customer to database
    /// It depends on UserManager to add identity user to User Table
    /// User Manager confirms that user has SecurityStamp
    /// Calls repository to add customer to databas
    /// </summary>
    /// <param name="input">Customer create dto</param>
    /// <returns>A dto for created customer</returns>
    /// <exception cref="UserFriendlyException">Throws exception when error occurs while saving customer</exception>
    public override async Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input)
    {
        var customer = ObjectMapper.Map<CreateUpdateCustomerDto, Customer>(input);
        try
        {

            if (! (await CheckIfPhoneNumberIsUnique(input.MobileNumber)))
            {
                throw new Exception("Phone Number is used");

            }
            var result = await _userManager.CreateAsync(customer.IdentityUser, input.Password);
            if (!result.Succeeded) { 
                    throw new Exception( FormatErrorMessage(result) );
            }
            await _userManager.UpdateSecurityStampAsync(customer.IdentityUser);
            await _repository.InsertAsync(customer);
            return ObjectMapper.Map<Customer, CustomerDto>(customer);



        }
        catch (Exception ex)
        {

            throw new UserFriendlyException("Registration failed: " + ex.Message);
        }
    }
    /// <summary>
    /// Method used to fromat messages that has been thrown in exception
    /// </summary>
    /// <param name="result">IdentityResult type that has message</param>
    /// <returns>String of contains message</returns>
    public string FormatErrorMessage(IdentityResult result)
    {
        string message = string.Empty;
        foreach (var error in result.Errors)
        {
            message += error.Description;
        }
        return message;
    }


    /// <summary>
    /// Method used to verify user otp
    /// </summary>
    /// <param name="input">Otp dto</param>
    /// <returns>boolean depends on operation status</returns>
    /// <exception cref="NotImplementedException">Throws exception if no user with that id in database</exception>

    public async Task<bool> VerifyOtpAsync(OtpVerificationDto input)
    {
        var cutsomer = await _repository.FindAsync(input.CustomerId);
        if (cutsomer == null) {

            throw new UserFriendlyException("No User matched that criteria in Database" );

        }
        if (cutsomer.OTP == input.OtpCode)
        {
            return true;
        }
        return false;

    }

    public async Task<bool> CheckIfPhoneNumberIsUnique(string phoneNumber) 
    {

        var result = await _repository.CountAsync(x => x.IdentityUser.PhoneNumber == phoneNumber);
        if (result == 0) {
            return true;
        
        }
        return false;

    }
    

}
