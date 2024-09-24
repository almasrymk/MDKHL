using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using SmartParking.Entities.Customers;
using SmartParking.Customers;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Application.Dtos;
using SmartParking.Cutomers;
using System.Linq;

namespace SmartParking.Customers;

public class CustomerAppService : CrudAppService<Customer, CustomerDto, Guid, PagedAndSortedResultRequestDto, RegisterCustomerDto>, ICustomerAppService
{
    private readonly IRepository<Customer, Guid> _repository;
    private readonly UserManager<Customer> _userManager;    
    private readonly SignInManager<Customer> _signInManager;

    public CustomerAppService(IRepository<Customer, Guid> repository, UserManager<Customer> userManager, SignInManager<Customer> signInManager) : base(repository)
    {
        _repository = repository;
        _userManager = userManager;
        _signInManager = signInManager;
    }

   

    public async Task<CustomerDto> RegisterAsync(RegisterCustomerDto input)
    {
        var customer = new Customer
        {
        
         
            FullName = input.FullName,
            MobileNumber = input.MobileNumber,
            NationalId = input.NationalId
        };

        var result = await _userManager.CreateAsync(customer, input.Password);
        if (!result.Succeeded)
        {
            throw new UserFriendlyException("Registration failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return ObjectMapper.Map<Customer, CustomerDto>(customer);
    }

    public async Task<CustomerDto> LoginAsync(LoginCustomerDto input)
    {
        var result = await _signInManager.PasswordSignInAsync(input.Email, input.Password, false, false);
        if (!result.Succeeded)
        {
            throw new UserFriendlyException("Login failed");
        }

        var customer = await _userManager.FindByEmailAsync(input.Email);
        return ObjectMapper.Map<Customer, CustomerDto>(customer);
    }

    private string GenerateOtp()
    {
        var random = new Random();
        return random.Next(1000, 9999).ToString();
    }

    public Task<Customer> GetAsync(CustomerDto id)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResultDto<Customer>> GetListAsync(Guid input)
    {
        throw new NotImplementedException();
    }

    public Task<Customer> CreateAsync(PagedAndSortedResultRequestDto input)
    {
        throw new NotImplementedException();
    }

    public Task<Customer> UpdateAsync(CustomerDto id, RegisterCustomerDto input)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(CustomerDto id)
    {
        throw new NotImplementedException();
    }
}
