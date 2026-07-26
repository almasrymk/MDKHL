using System;
using System.Text.Json.Serialization;

namespace SmartParking.Payments.PosTerminal;

public class PosBaseMessage
{
    [JsonPropertyName("messageType")]
    public string MessageType { get; set; } = string.Empty;
}

public sealed class PosHandshakeMessage : PosBaseMessage
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "1.0";

    [JsonPropertyName("organizationId")]
    public string OrganizationId { get; set; } = string.Empty;

    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; } = string.Empty;
}

public sealed class PosHandshakeAckMessage : PosBaseMessage
{
    public PosHandshakeAckMessage()
    {
        MessageType = "HANDSHAKE_ACK";
    }

    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");
}

public sealed class PosHandshakeRejectedMessage : PosBaseMessage
{
    public PosHandshakeRejectedMessage()
    {
        MessageType = "HANDSHAKE_REJECTED";
    }

    [JsonPropertyName("success")]
    public bool Success { get; set; } = false;

    [JsonPropertyName("error")]
    public PosErrorDetail? Error { get; set; }
}

public sealed class PosCommandMessage : PosBaseMessage
{
    public PosCommandMessage()
    {
        MessageType = "COMMAND";
    }

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public object? Payload { get; set; }
}

public sealed class PosResponseMessage : PosBaseMessage
{
    public PosResponseMessage()
    {
        MessageType = "RESPONSE";
    }

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public object? Payload { get; set; }

    [JsonPropertyName("error")]
    public PosErrorDetail? Error { get; set; }
}

public sealed class PosEventMessage : PosBaseMessage
{
    public PosEventMessage()
    {
        MessageType = "EVENT";
    }

    [JsonPropertyName("organizationId")]
    public string OrganizationId { get; set; } = string.Empty;

    [JsonPropertyName("eventName")]
    public string EventName { get; set; } = string.Empty;

    [JsonPropertyName("terminalAction")]
    public string? TerminalAction { get; set; }

    [JsonPropertyName("terminalStatus")]
    public string? TerminalStatus { get; set; }

    [JsonPropertyName("optionalMessage")]
    public string? OptionalMessage { get; set; }

    [JsonPropertyName("payload")]
    public object? Payload { get; set; }
}

public sealed class PosPingMessage : PosBaseMessage
{
    public PosPingMessage()
    {
        MessageType = "PING";
    }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");
}

public sealed class PosPongMessage : PosBaseMessage
{
    public PosPongMessage()
    {
        MessageType = "PONG";
    }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");
}

public sealed class PosErrorDetail
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public sealed class PosTerminalJsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("responseCode")]
    public string? ResponseCode { get; set; }

    [JsonPropertyName("responseMessage")]
    public string? ResponseMessage { get; set; }

    [JsonPropertyName("maskedPan")]
    public string? MaskedPan { get; set; }

    [JsonPropertyName("authCode")]
    public string? AuthCode { get; set; }

    [JsonPropertyName("rrn")]
    public string? Rrn { get; set; }

    [JsonPropertyName("cardScheme")]
    public string? CardScheme { get; set; }

    [JsonPropertyName("error")]
    public PosErrorDetail? Error { get; set; }
}

