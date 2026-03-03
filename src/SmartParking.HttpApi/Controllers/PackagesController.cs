using DeviceDetectorNET;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParking.CustomerPackages;
using SmartParking.CustomerPackages.AssignantionsContarcts;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
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
    private readonly IAssignCustomerPackagesMobileAppService _customerPackagesActionsAppService;



    public PackagesController(IPackagesAppService package, ICustomerPackagesAppService customerPackagesApp, IPaymentAppService paymentAppService, IAssignCustomerPackagesMobileAppService customerPackagesActionsAppService)
    {
        _packages = package;
        _customerPackagesApp = customerPackagesApp;
        _paymentAppService = paymentAppService;
        _customerPackagesActionsAppService = customerPackagesActionsAppService;
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
    public async Task<Result<PaymentCheckoutDto>> AssignPackageToCustomer([FromBody] AssignMobilePackageRequestDto request)
    {
        var customerPackageDto =
            await _customerPackagesActionsAppService.AssignPackageToCustomerAsync(request.PackageId,request.ArabicNumber,request.EnglishPlateNumber);
        if (customerPackageDto.Errors.Any())
            return Result.Error(customerPackageDto.Errors.First());

        if (customerPackageDto.ValidationErrors.Any())
            return Result.Invalid(customerPackageDto.ValidationErrors.First());

        var paymentReq = customerPackageDto.Value
            .MapToPaymentCheckout(customerPackageDto, customerPackageDto.Value.TenantId!.Value);

        var result = await _paymentAppService.CreateCheckoutPackagePaymentWebAsync(paymentReq);

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

    [HttpGet("paymentsbyorganization")]
    [Authorize]
    public async Task<Result<List<PaymentPackageDto>>> GetAllPaymentPackages([FromQuery]int organizationId)
    {
        return await _paymentAppService.GetAllCustomerPackgePaymentsAsync(organizationId);
    }
}
