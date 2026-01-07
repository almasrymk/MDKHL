using System.Collections.Generic;

namespace SmartParking.Payments.PaymentIntegrations;

public class OppwaRootOptions
{
    public string Mode { get; set; } = "Test";
    public OppwaEnvironmentOptions Test { get; set; } = new();
    public OppwaEnvironmentOptions Live { get; set; } = new();
}

public class OppwaEnvironmentOptions
{
    public string BaseUrl { get; set; } = "";
    public string Currency { get; set; } = "";
    public string PaymentType { get; set; } = "";
    public AuthOptions Auth { get; set; } = new();
    public string EntityId { get; set; }
    public bool Skip3DSecure { get; set; }
}

public class AuthOptions
{
    public string TokenType { get; set; } = "";
    public string Token { get; set; } = "";
}
