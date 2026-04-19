using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartParking.IntegrationUnit;
using SmartParking.IntegrationUnit.Dtos;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartParking.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IntegrationController : ControllerBase
{
    private readonly IIntegrationUnitAppService _integrationUnitAppService;
    private readonly ILogger<IntegrationController> _logger;
    public IntegrationController(IIntegrationUnitAppService integrationUnitAppService, ILogger<IntegrationController> logger)
    {
        _integrationUnitAppService = integrationUnitAppService;
        _logger = logger;
    }

    [HttpPost("book")]
    [Authorize]
    public async Task<IActionResult> Book(CreateUpdateBookingDto input)
    {
        _logger.LogInformation("Booking DTO: {BookingJson}",
         JsonSerializer.Serialize(input, new JsonSerializerOptions
         {
             WriteIndented = true
         }));
        if (input.PhoneNumber is not null) 
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
