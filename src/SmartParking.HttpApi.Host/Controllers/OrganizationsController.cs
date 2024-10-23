using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Organizations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Controllers;

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
    [Authorize(Roles = "Mdkhl")]
    public async Task<List<OrganizationDto>> GetAll()
    {
        return await _organizations.GetAllMobileAsync();
    }
}
