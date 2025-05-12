using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Customers;

[RemoteService(IsEnabled = false)]
public class BaseCustomerAppService : ApplicationService, IBaseCustomerAppService
{
    private readonly IRepository<Customer, Guid> _repository;
    private readonly IRepository<CustomerPackage, int> _customerPackageRepository;
	private readonly IRepository<Package, int> _packageRepository;
	private readonly IDataFilter _dataFilter;

	public BaseCustomerAppService(IRepository<Customer, Guid> repository, IRepository<CustomerPackage, int> customerPackageRepository,IRepository<Package,int> packageRepository , IDataFilter dataFilter)
    {
        _repository = repository;
        _customerPackageRepository = customerPackageRepository;
		_packageRepository = packageRepository;
		_dataFilter = dataFilter;
	}
    #region public methods 
    ///<inheritdoc/>
    public async Task AddCustomerPackage(int organizationId, Guid customerId,int packageId)
    {
        var package = await _packageRepository.GetAsync(packageId);
        var customerPackage = new CustomerPackage
        {
            CustomerId = customerId,
            CustomerPackageStatus = CustomerPackageStatus.Active,
            OrganizationId = organizationId,
            TenantId = null,
            PackageId = packageId,
            PackageArabicName = package.ArabicName,
            PackageEnglishName = package.EnglishName,
            PriceOfPackage = package.Price,
            publishedOfPackage = package.published,
            ActivationDateOfPackage = package.ActivationDate,
            DectivationDateOfPackage= package.DectivationDate,
            DescriptionOfPackage = package.Description,
            DurationOfPackage = package.DurationOfPackage,
            ExpiryDate = DateTime.Today.AddDays(package.DurationOfPackage),
            PublishFromOfPackage = package.PublishFrom,
            PublishToOfPackage = package.PublishTo,
            

		};
        await _customerPackageRepository.InsertAsync(customerPackage, true);
    }


    ///<inheritdoc/>
    public async Task CheckCustomer(CreateUpdateCustomerDto input)
    {
        if (input.IdentificationType == IdentificationTypes.NationalID)
        {
            var nationalIdPattern = new Regex(@"^(1|2)\d{9}$");
            if (!nationalIdPattern.IsMatch(input.NationalId))
            {
                throw new BusinessException(LocalizationHelper.GetLocalizedString("NationIdValidation"));
            }
        }
        else if (input.IdentificationType == IdentificationTypes.Passport)
        {
            var passportPattern = new Regex(@"^(?=(?:\d*[a-zA-Z]){1}\d*$)[a-zA-Z0-9]{6,9}$");
            if (!passportPattern.IsMatch(input.NationalId))
            {
                throw new BusinessException(LocalizationHelper.GetLocalizedString("PassportValidation"));
            }
        }
        var result2 = await CheckIfNationalIDIsUnique(input.NationalId);
        if (!result2)
        {
            throw new BusinessException(LocalizationHelper.GetLocalizedString("NationalId") +" "+LocalizationHelper.GetLocalizedString("DuplicateValidation"));

        }
    }

    /// <inheritdoc/>

    public async Task<bool> CheckIfNationalIDIsUnique(string NationalId)
    {
        var query = await _repository.GetQueryableAsync();
        var result = query.Where(x => x.NationalId == NationalId).ToList();
        if (result.Count == 0)
        {
            return true;

        }
        return false;

    }

    /// <inheritdoc/>
    public async Task<bool> CheckIfPhoneNumberIsUnique(string phoneNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
			var query = await _repository.GetQueryableAsync();
			var result = query.Where(x => x.IdentityUser.PhoneNumber == phoneNumber).ToList();

			if (result.Count == 0)
			{
				return true;

			}
			return false;
		}
       

    }

    /// <inheritdoc/>
    public async Task<string> GenerateOtp(Guid CustomerId)
    {

        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.Id == CustomerId);
        var customer = query.FirstOrDefault();

        if (customer == default(Customer))
        {
            throw new EntityNotFoundException(typeof(Customer), CustomerId);

        }
        var random = new Random();
        string otp = random.Next(1000, 9999).ToString();
        customer.OTP = otp;
        customer.OtpExpirationDate = DateTime.UtcNow.AddMinutes(1);
        await _repository.UpdateAsync(customer);
        return otp;

    }

    /// <inheritdoc/>
    public bool IsEmailValid(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        var emailvalidation = Regex.IsMatch(email, pattern);
        if (!emailvalidation)
        {
            return false;

        }
        return true;
    }

    ///<inheritdoc/>
    public async Task<CustomerDto> UserIsFound(Volo.Abp.Identity.IdentityUser? user)
    {
        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.IdentityUserId == user.Id);
        var existCustomer = query.FirstOrDefault();
        await _repository.UpdateAsync(existCustomer);
        return ObjectMapper.Map<Customer, CustomerDto>(existCustomer);
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyOtpAsync(OtpVerificationDto input)
    {

        var customer = await _repository.FindAsync(input.CustomerId);

        if (customer == null)
        {

            throw new BusinessException("No User matched that criteria in Database");

        }
        if (customer.OTP == input.OtpCode)
        {
            if (DateTime.UtcNow <= customer.OtpExpirationDate)
            {
                return true;
            }
            else
            {
                throw new BusinessException("your Otp is expired");

            }
        }
        return false;

    }
    #endregion
}
