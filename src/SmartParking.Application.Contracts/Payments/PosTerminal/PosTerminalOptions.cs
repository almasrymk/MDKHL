namespace SmartParking.Payments.PosTerminal;

public class PosTerminalOptions
{
    public const string SectionName = "PosTerminal";

    /// <summary>Fixed local Geidea Web ECR Windows Service address.</summary>
    public string ServiceUrl { get; set; } = "ws://localhost:5000/messages";

    /// <summary>Global connection handshake timeout.</summary>
    public int ConnectionTimeoutMs { get; set; } = 5000;

    /// <summary>Global transaction timeout (SAMA terminal response wait).</summary>
    public int TransactionTimeoutMs { get; set; } = 60000;
}
