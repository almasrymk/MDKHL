namespace SmartParking.Payments.PosTerminal;

public class PosTerminalOptions
{
    public const string SectionName = "PosTerminal";

    /// <summary>WebSocket URL of the local Geidea Web ECR Service on the client POS machine.</summary>
    public string WebSocketUrl { get; set; } = "ws://localhost:5000/messages";

    /// <summary>Alias getter/setter for WebSocketUrl.</summary>
    public string ServiceUrl
    {
        get => WebSocketUrl;
        set => WebSocketUrl = value;
    }

    /// <summary>Global connection timeout (ms).</summary>
    public int ConnectionTimeoutMs { get; set; } = 10000;

    /// <summary>Global transaction timeout (ms).</summary>
    public int TransactionTimeoutMs { get; set; } = 90000;
}
