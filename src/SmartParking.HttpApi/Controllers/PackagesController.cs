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
    public Task<Result<List<PackageDto>>> GetAllPakageByOrganization(int oragnizationId)
    {
        return _packages.GetListPackageByOrganizationId(oragnizationId);
    }    
    [HttpGet("mypakages")]
    [Authorize]
    public Task<Result<List<PackageDto>>> GetPackagesCustomer()
    {
        return _customerPackagesApp.GetPackageByCustomer();
    }    
    
    [HttpGet("assignpackage")]
    [Authorize]
    public Task<Result> AssignPackageToCustomer(int packageId)
    {
        return _customerPackagesApp.AssignPackageToCustomerAsync(packageId);
    }
}
