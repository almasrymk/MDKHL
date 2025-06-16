using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartParking.WebSockets;

public class HandlerService : IHandlerService
{
    private readonly ILogger<HandlerService> _logger;
    public HandlerService(ILogger<HandlerService> logger)
    {
        _logger = logger;
    }
    public async Task<string> ReceiveAsync(int organizationId, WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        string lastMessage = "No message received.";

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // Timeout after 30 seconds
                WebSocketReceiveResult result;

                try
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("ReceiveAsync timed out for organization {OrganizationId}", organizationId);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("Client initiated connection close for organization {OrganizationId}", organizationId);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                _logger.LogInformation("Received message from client: {Message}", message);
                lastMessage = message;
            }

            return lastMessage;
        }
        catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
        {
            _logger.LogWarning("Client disconnected unexpectedly for organization {OrganizationId}", organizationId);
            return "Client disconnected unexpectedly.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WebSocket communication");
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error occurred", CancellationToken.None);
            }
            return $"Error: {ex.Message}";
        }
    }

}
