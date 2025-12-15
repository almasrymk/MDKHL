using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartParking.WebSockets;

public class HandlerService : IHandlerService
{
    private readonly ILogger<HandlerService> _logger;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<string>> _pendingRequests = new();

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
                ProcessMessage(message);
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

    public async Task<TResponse?> SendMessageAsync<TResponse>(int orgId, WebSocket webSocket, object data)
    {

        if (webSocket.State != WebSocketState.Open)
        {
            _logger.LogWarning("Attempted to send message on a closed WebSocket.");
            return default;
        }
        var requestId = Guid.NewGuid();
        var tcs = new TaskCompletionSource<string>();
        _pendingRequests[requestId] = tcs;

        try
        {
            var request = new
            {
                RequestId = requestId,
                Data = data
            };

            var jsonMessage = JsonSerializer.Serialize(request);
            var buffer = Encoding.UTF8.GetBytes(jsonMessage);
            var segment = new ArraySegment<byte>(buffer);

            await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, default);

            _logger.LogInformation("Sent message to client: {Message}", jsonMessage);
            var response = await tcs.Task;
            var parsedResponse = JsonSerializer.Deserialize<RequestMessage<TResponse>>(response);
            return parsedResponse!.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message via WebSocket");
            _pendingRequests.TryRemove(requestId, out _);
            tcs.TrySetException(ex);
            return default;
        }
    }

    public async Task<FindMyCarResponse> FindMyCarAsync(int orgId, string action, WebSocket webSocket, string licensePlate)
    {

        var request = new
        {
            action = action,
            LicensePlate = licensePlate
        };

        var response = await SendMessageAsync<FindMyCarResponse>(orgId, webSocket, request);
        return response!;
    }
    private void ProcessMessage(string message)
    {
        try
        {
            var request = JsonSerializer.Deserialize<RequestMessage<object>>(message);
            if (request != null)
            {
                HandleResponse(request.RequestId, message);
            }
            else
            {
                Console.WriteLine($"Recieved message: {message}");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to parse message: {ex.Message}");
        }
    }
    public void HandleResponse(Guid requestId, string response)
    {
        if (_pendingRequests.TryRemove(requestId, out var tcs))
        {
            tcs.TrySetResult(response);
        }
        else
        {
            Console.WriteLine($"No pending request found for RequestId: {requestId}");
        }
    }
}


