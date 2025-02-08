using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParking.CustomerPackages;
using SmartParking.Packages;
using SmartParking.Results;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Controllers;

[Route("api/mobile/[controller]")]
[ApiController]
public class PackagesController : ControllerBase
{
    private readonly IPackagesAppService _packages;
    private readonly ICustomerPackagesAppService _customerPackagesApp;

    public PackagesController(IPackagesAppService package, ICustomerPackagesAppService customerPackagesApp)
    {
        _packages = package;
        _customerPackagesApp = customerPackagesApp;
    }
    [HttpGet("{oragnizationId}")]
    [Authorize]
    public async Task<Result<List<PackageDto>>> GetAllPakageByOrganization(int oragnizationId)
    {
        return await _packages.GetListPackageByOrganizationId(oragnizationId);
    }    
    [HttpGet("mypackages")]
    [Authorize]
    public async Task<Result<List<CustomerPackageDto>>> GetPackagesCustomer()
    {
        return await _customerPackagesApp.GetPackageByCustomer();
    }    
    
    [HttpGet("assignpackage")]
    [Authorize]
    public async Task<Result> AssignPackageToCustomer(int packageId,string ArabicNumber , string EnglishPlateNumber)
    {
        return await _customerPackagesApp.AssignPackageToCustomerAsync(packageId,ArabicNumber,EnglishPlateNumber);
    } 
    
    [HttpPut("removepackage")]
    [Authorize]
    public async Task<Result> CancelPackage(int packageId)
    {
        await _customerPackagesApp.DeleteAsync(packageId);
        return Result.Success();
    }
}
