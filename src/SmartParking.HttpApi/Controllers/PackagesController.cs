using DeviceDetectorNET;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParking.CustomerPackages;
using SmartParking.Packages;
using SmartParking.Payments;
using SmartParking.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartParking.Controllers;

[Route("api/mobile/[controller]")]
[ApiController]
public class PackagesController : ControllerBase
{
    private readonly IPackagesAppService _packages;
    private readonly ICustomerPackagesAppService _customerPackagesApp;
    private readonly IPaymentAppService _paymentAppService;


    public PackagesController(IPackagesAppService package, ICustomerPackagesAppService customerPackagesApp, IPaymentAppService paymentAppService)
    {
        _packages = package;
        _customerPackagesApp = customerPackagesApp;
        _paymentAppService = paymentAppService;
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
    
    [HttpPost("assignpackage")]
    [Authorize]
    public async Task<Result<PaymentCheckoutDto>> AssignPackageToCustomer(int packageId,string ArabicNumber , string EnglishPlateNumber)
    {
        var paymentReq = await _customerPackagesApp.AssignPackageToCustomerAsync(packageId,ArabicNumber,EnglishPlateNumber);
        if (paymentReq.Errors.Any())
            return Result.Error(paymentReq.Errors.FirstOrDefault()!);
        var result=await _paymentAppService.CreateCheckoutPackagePaymentWebAsync(paymentReq.Value);
        return Result.Success(result!);
    } 
    
    [HttpPut("removepackage")]
    [Authorize]
    public async Task<Result> CancelPackage(int packageId)
    {
        await _customerPackagesApp.DeleteAsync(packageId);
        return Result.Success();
    }    
    
    [HttpGet("getpackagebyid/{packageId}")]
    [Authorize]
    public async Task<Result<CustomerPackageDto>> GetPackageById(int packageId)
    {
       return await _customerPackagesApp.GetAsync(packageId);
    }
}
