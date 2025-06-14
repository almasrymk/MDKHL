using Castle.MicroKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartParking.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartParking.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WebSocketHandlerController : AbpController
{
    private readonly IWebSocketConnectionManager _connectionManager;
    private readonly ILogger<WebSocketHandlerController> _logger;
    private readonly IHandlerService _handler;
    public WebSocketHandlerController(
       IWebSocketConnectionManager connectionManager,
       ILogger<WebSocketHandlerController> logger,
       IHandlerService handler
       )
    {
        _connectionManager = connectionManager;
        _logger = logger;
        _handler = handler;
    }


    [HttpGet("connect/{organizationId}")]
    public async Task<IActionResult> Connect(int organizationId)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest("Expected WebSocket request");

        var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        await _connectionManager.AddConnection(organizationId.ToString(), socket);
        _logger.LogInformation("WebSocket connected: Org={OrganizationId}, ConnId={ConnectionId}", organizationId, organizationId.ToString());
        await _handler.ReceiveAsync(organizationId, socket);
        return new EmptyResult();
    }
}
