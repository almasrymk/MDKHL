using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.Packages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Controller;

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
    [Authorize(Roles = "Mdkhl")]
    public Task<List<PackageDto>> GetAllPakageByOrganization(int oragnizationId)
    {
        return _packages.GetListPackageByOrganizationId(oragnizationId);
    }
}
