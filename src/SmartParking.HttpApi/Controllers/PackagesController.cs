using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    public PackagesController(IPackagesAppService package)
    {
        _packages = package;
    }
    [HttpGet("{oragnizationId}")]
    [Authorize]
    public Task<Result<List<PackageDto>>> GetAllPakageByOrganization(int oragnizationId)
    {
        return _packages.GetListPackageByOrganizationId(oragnizationId);
    }
}
