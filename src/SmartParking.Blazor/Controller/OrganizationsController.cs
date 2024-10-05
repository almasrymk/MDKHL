using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Organizations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SmartParking.Blazor.Controller;
[Route("api/mobile/[controller]")]
[ApiController]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationsAppService _organizations;

    public OrganizationsController(IOrganizationsAppService organizations)
    {
        _organizations = organizations;
    }
    [HttpGet]
    [Authorize(Roles = "admin")]
    public Task<List<OrganizationDto>> GetAll()
    {
        return _organizations.GetAllMobileAsync();
    }
}
