using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace SmartParking.Payments.PosTerminal;

public class PosServiceManager : IPosServiceManager, ISingletonDependency
{
    private static readonly HttpClient _httpClient = new();
    private readonly PosTerminalOptions _options;
    private readonly IPosAesEncryptionService _encryptionService;
    private readonly ILogger<PosServiceManager> _logger;
    private readonly Dictionary<string, PosOrganizationOptions> _organizationLookup;

    public event Action<PosEventMessage>? OnEventReceived;

    public PosServiceManager(
        IOptions<PosTerminalOptions> options,
        IPosAesEncryptionService encryptionService,
        ILogger<PosServiceManager> logger)
    {
        _options = options.Value;
        _encryptionService = encryptionService;
        _logger = logger;

        _options.Validate();

        _organizationLookup = _options.Organizations
            .ToDictionary(o => o.OrganizationId, o => o, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsConnected(string organizationId)
    {
        if (string.IsNullOrWhiteSpace(organizationId)) return false;
        return _organizationLookup.ContainsKey(organizationId);
    }

    public IReadOnlyCollection<string> GetConnectedOrganizations()
    {
        return _organizationLookup.Keys.ToList().AsReadOnly();
    }

    public Task<string> GetOrganizationApiKeyAsync(string organizationId)
    {
        if (string.IsNullOrWhiteSpace(organizationId) || !_organizationLookup.TryGetValue(organizationId, out var orgConfig))
        {
            throw new InvalidOperationException($"OrganizationId '{organizationId}' is not configured on server.");
        }

        string decryptedApiKey = _encryptionService.DecryptApiKey(orgConfig.EncryptedApiKey, _options.AesMasterKeyHex);
        return Task.FromResult(decryptedApiKey);
    }

    private string GetNormalizedServiceBaseUrl()
    {
        string url = _options.ServiceBaseUrl;
        if (string.IsNullOrWhiteSpace(url) || url.Contains("/messages", StringComparison.OrdinalIgnoreCase))
        {
            url = "http://127.0.0.1:5005";
        }

        if (url.StartsWith("ws://", StringComparison.OrdinalIgnoreCase))
        {
            url = "http://" + url[5..];
        }
        else if (url.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url[6..];
        }

        int messagesIdx = url.IndexOf("/messages", StringComparison.OrdinalIgnoreCase);
        if (messagesIdx > 0)
        {
            url = url[..messagesIdx];
        }

        int posServiceIdx = url.IndexOf("/pos-service", StringComparison.OrdinalIgnoreCase);
        if (posServiceIdx > 0)
        {
            url = url[..posServiceIdx];
        }

        return url.TrimEnd('/');
    }

    public async Task<PosResponseMessage> SendCommandAsync(string organizationId, PosCommandMessage command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(organizationId) || !_organizationLookup.TryGetValue(organizationId, out var orgConfig))
        {
            _logger.LogWarning("[PosServiceManager] Unknown OrganizationId {OrgId} for command execution.", organizationId);
            return new PosResponseMessage
            {
                RequestId = command.RequestId,
                Success = false,
                Status = "UNAUTHORIZED",
                Error = new PosErrorDetail { Code = "UNKNOWN_ORGANIZATION", Message = $"OrganizationId '{organizationId}' is not configured on server" }
            };
        }

        string decryptedApiKey;
        try
        {
            decryptedApiKey = _encryptionService.DecryptApiKey(orgConfig.EncryptedApiKey, _options.AesMasterKeyHex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PosServiceManager] Failed to decrypt configured API key for Organization {OrgId}", organizationId);
            return new PosResponseMessage
            {
                RequestId = command.RequestId,
                Success = false,
                Status = "CONFIGURATION_ERROR",
                Error = new PosErrorDetail { Code = "DECRYPTION_ERROR", Message = "Failed to decrypt server organization API key" }
            };
        }

        int timeoutMs = _options.TransactionTimeoutMs > 0 ? _options.TransactionTimeoutMs : 60000;
        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        string baseUrl = GetNormalizedServiceBaseUrl();
        string targetUrl = $"{baseUrl}/api/pos/commands";

        var requestBody = new
        {
            requestId = command.RequestId,
            command = command.Command,
            payload = command.Payload
        };

        string jsonContent = JsonSerializer.Serialize(requestBody);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, targetUrl)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        requestMessage.Headers.Add("X-Organization-Id", organizationId);
        requestMessage.Headers.Add("X-API-Key", decryptedApiKey);

        _logger.LogInformation("[PosServiceManager] Sending HTTP POS command {Command} (RequestId={ReqId}) to {Url}", command.Command, command.RequestId, targetUrl);

        try
        {
            var httpResponse = await _httpClient.SendAsync(requestMessage, linkedCts.Token);
            string responseBody = await httpResponse.Content.ReadAsStringAsync(linkedCts.Token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("[PosServiceManager] HTTP request failed with status code {Code}: {Body}", httpResponse.StatusCode, responseBody);
                return new PosResponseMessage
                {
                    RequestId = command.RequestId,
                    Success = false,
                    Status = "HTTP_ERROR",
                    Error = new PosErrorDetail { Code = $"HTTP_{(int)httpResponse.StatusCode}", Message = $"HTTP error {httpResponse.StatusCode}" }
                };
            }

            var posResponse = JsonSerializer.Deserialize<PosResponseMessage>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (posResponse == null)
            {
                _logger.LogWarning("[PosServiceManager] Null JSON response received from POS Service HTTP endpoint");
                return new PosResponseMessage
                {
                    RequestId = command.RequestId,
                    Success = false,
                    Status = "INVALID_RESPONSE",
                    Error = new PosErrorDetail { Code = "INVALID_PAYLOAD", Message = "Received empty response from POS service" }
                };
            }

            _logger.LogInformation("[PosServiceManager] Received correlated HTTP response for RequestId={ReqId}, Success={Success}, Status={Status}",
                posResponse.RequestId, posResponse.Success, posResponse.Status);

            return posResponse;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "[PosServiceManager] HTTP POS command timed out after {Timeout}ms", timeoutMs);
            return new PosResponseMessage
            {
                RequestId = command.RequestId,
                Success = false,
                Status = "TIMEOUT",
                Error = new PosErrorDetail { Code = "POS_TIMEOUT", Message = $"POS command timed out after {timeoutMs}ms" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PosServiceManager] HTTP POS command failed");
            return new PosResponseMessage
            {
                RequestId = command.RequestId,
                Success = false,
                Status = "CONNECTION_ERROR",
                Error = new PosErrorDetail { Code = "HTTP_CLIENT_ERROR", Message = ex.Message }
            };
        }
    }

    public void HandleEventMessage(PosEventMessage eventMessage)
    {
        if (eventMessage != null)
        {
            OnEventReceived?.Invoke(eventMessage);
        }
    }

    public void HandleResponseMessage(PosResponseMessage responseMessage)
    {
    }
}
