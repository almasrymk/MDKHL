using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartParking.Payments.PosTerminal;

public interface IPosServiceManager
{
    /// <summary>
    /// Checks if POS service configuration is available for an organization.
    /// </summary>
    bool IsConnected(string organizationId);

    /// <summary>
    /// Returns a list of all configured OrganizationIds.
    /// </summary>
    IReadOnlyCollection<string> GetConnectedOrganizations();

    /// <summary>
    /// Returns the decrypted API key for an organization for cashier session dispatch.
    /// </summary>
    Task<string> GetOrganizationApiKeyAsync(string organizationId);

    /// <summary>
    /// Sends a high-level command to the standalone POS service via HTTP POST and awaits correlated response.
    /// </summary>
    Task<PosResponseMessage> SendCommandAsync(string organizationId, PosCommandMessage command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles incoming event messages from connected POS services.
    /// </summary>
    event Action<PosEventMessage>? OnEventReceived;

    /// <summary>
    /// Dispatches inbound event message to event subscribers.
    /// </summary>
    void HandleEventMessage(PosEventMessage eventMessage);

    /// <summary>
    /// Dispatches inbound response message.
    /// </summary>
    void HandleResponseMessage(PosResponseMessage responseMessage);
}
