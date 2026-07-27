using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartParking.IntegrationUnit;
using SmartParking.IntegrationUnit.Dtos;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartParking.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IntegrationController : ControllerBase
{
    private readonly ILogger<IntegrationController> _logger;
    private readonly IIntegrationHandler _integrationHandler;
    public IntegrationController(ILogger<IntegrationController> logger, 
        IIntegrationHandler integrationHandler)
    {
        _logger = logger;
        _integrationHandler = integrationHandler;
    }

    [HttpPost("book")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    public async Task<IActionResult> Book(CreateUpdateBookingDto input)
    {
        _logger.LogInformation("Booking DTO: {BookingJson}",
         JsonSerializer.Serialize(input, new JsonSerializerOptions
         {
             WriteIndented = true
         }));
        var result = await _integrationHandler.HandleBookingAsync(input);
        if (!result.IsSuccess)
        {
            _logger.LogError("Booking failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }
        return Ok(result);
    }

    [HttpPost("v2/book")]
  //  [Authorize]
    [ApiExplorerSettings(GroupName = "v2")]
    public async Task<IActionResult> BookV2([FromBody] CreateUpdateBookingV2Dto input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.ProviderName))
        {
            return BadRequest(new[] { "ProviderName is required." });
        }

        _logger.LogInformation("Booking V2 DTO: {BookingJson}",
         JsonSerializer.Serialize(input, new JsonSerializerOptions
         {
             WriteIndented = true
         }));
        var result = await _integrationHandler.HandleBookingV2Async(input);
        if (!result.IsSuccess)
        {
            _logger.LogError("Booking V2 failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }
        return Ok(result);
    }
}
