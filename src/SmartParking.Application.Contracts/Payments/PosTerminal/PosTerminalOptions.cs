using System;
using System.Collections.Generic;

namespace SmartParking.Payments.PosTerminal;

public class PosTerminalOptions
{
    public const string SectionName = "PosTerminal";

    /// <summary>HTTP Base URL of the Standalone Node.js POS Service.</summary>
    public string ServiceBaseUrl { get; set; } = "http://127.0.0.1:5005";

    /// <summary>Backwards-compatibility alias for ServiceBaseUrl (read-only getter returns ServiceBaseUrl).</summary>
    public string ServiceUrl
    {
        get => ServiceBaseUrl;
        set { /* Ignore legacy WS URL assignments */ }
    }

    /// <summary>32-byte (64 hex characters) AES master key for decrypting organization API keys.</summary>
    public string AesMasterKeyHex { get; set; } = "000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f";

    /// <summary>Global connection handshake timeout (ms).</summary>
    public int HandshakeTimeoutMs { get; set; } = 10000;

    /// <summary>Backwards-compatibility alias for HandshakeTimeoutMs.</summary>
    public int ConnectionTimeoutMs
    {
        get => HandshakeTimeoutMs;
        set => HandshakeTimeoutMs = value;
    }

    /// <summary>Global transaction timeout (SAMA terminal response wait, ms).</summary>
    public int TransactionTimeoutMs { get; set; } = 60000;

    /// <summary>List of organization POS terminal configurations.</summary>
    public List<PosOrganizationOptions> Organizations { get; set; } = new();

    /// <summary>
    /// Validates configuration on startup, enforcing unique OrganizationIds and non-empty keys.
    /// </summary>
    public void Validate()
    {
        var seenOrgs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var org in Organizations)
        {
            if (string.IsNullOrWhiteSpace(org.OrganizationId))
            {
                throw new InvalidOperationException("PosTerminal configuration invalid: OrganizationId cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(org.EncryptedApiKey))
            {
                throw new InvalidOperationException($"PosTerminal configuration invalid: EncryptedApiKey is missing for organization '{org.OrganizationId}'.");
            }

            if (!seenOrgs.Add(org.OrganizationId))
            {
                throw new InvalidOperationException($"PosTerminal configuration invalid: Duplicate OrganizationId '{org.OrganizationId}' found.");
            }
        }
    }
}

public class PosOrganizationOptions
{
    public string OrganizationId { get; set; } = string.Empty;
    public string EncryptedApiKey { get; set; } = string.Empty;
}
