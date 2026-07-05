using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartParking.Enums;

namespace SmartParking.Payments.PaymentIntegrations;

public static class PaymentCallbackHelper
{
    public static string ResolveCallbackUrl(IServiceProvider serviceProvider, string baseUri, GatewayKey gatewayKey)
    {
        baseUri = baseUri.TrimEnd('/');

        switch (gatewayKey)
        {
            case GatewayKey.Geidea:
            {
                var opts = serviceProvider.GetRequiredService<IOptionsMonitor<GeideaOptions>>().CurrentValue;
                var env = opts.Mode == "Live" ? opts.Live : opts.Test;
                var relPath = env.CallbackUrl.TrimStart('/');
                return $"{baseUri}/{relPath}";
            }

            case GatewayKey.Oppwa:
            {
                var opts = serviceProvider.GetRequiredService<IOptionsMonitor<OppwaRootOptions>>().CurrentValue;
                var env = opts.Mode == "Live" ? opts.Live : opts.Test;
                var relPath = env.CallbackUrl.TrimStart('/');
                return $"{baseUri}/{relPath}";
            }

            default:
                return $"{baseUri}/api/PaymentResult/{gatewayKey.ToString().ToLower()}/callback";
        }
    }
}
