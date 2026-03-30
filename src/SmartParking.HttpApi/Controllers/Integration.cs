using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.IntegrationUnit;
using System.Threading.Tasks;

namespace SmartParking.Controllers;

[Route("api/[controller]")]
[ApiController]
public class Integration : ControllerBase
{
    private readonly IIntegrationUnitAppService _integrationUnitAppService;

    public Integration(IIntegrationUnitAppService integrationUnitAppService)
    {
        _integrationUnitAppService = integrationUnitAppService;
    }

    [HttpPost("book")]
    [Authorize]
    public async Task<IActionResult> Book(CreateUpdateBookingDto input)
    {
        if (input.CustomerId is not null && input.VehicleId is not null) 
        { 
            var updateResult = await _integrationUnitAppService.UpdateBookingAsync(input);
            if (!updateResult.IsSuccess)
            {
                return BadRequest(updateResult);
            }
            return Ok(updateResult);
        }

        var result = await _integrationUnitAppService.CreateBookingAsync(input);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
