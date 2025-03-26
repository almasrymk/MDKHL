using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Text.Formatting;

namespace SmartParking.Blazor.MultiTenants;

public class MultiDomainTenantResolver : HttpTenantResolveContributorBase
{
    public const string ContributorName = "Custom";

    public override string Name => ContributorName;

    private static readonly string[] ProtocolPrefixes = { "http://", "https://" };

    private readonly string _domainFormat;

    public MultiDomainTenantResolver(string domainFormat)
    {
        _domainFormat = domainFormat.RemovePreFix(ProtocolPrefixes);
    }

    public override async Task ResolveAsync(ITenantResolveContext context)
    {
        var httpContext = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>()?.HttpContext;
        if (httpContext == null) return;

        var host = httpContext.Request.Host.Host;
        var config = context.ServiceProvider.GetRequiredService<IConfiguration>();
        var tenantDomains = config.GetSection("TenantDomains").Get<string[]>();

        if (tenantDomains == null || tenantDomains.Length == 0) return;

        foreach (var domainPattern in tenantDomains)
        {
            var tenantRepo = context.ServiceProvider.GetRequiredService<ITenantRepository>();
            var pattern = domainPattern.Replace("{0}.", "");
            if (host.EndsWith(pattern, StringComparison.OrdinalIgnoreCase))
            {
                var subdomain = host.Replace($".{pattern}", "");

                if (subdomain.Equals("admin", StringComparison.OrdinalIgnoreCase) || subdomain.Equals("app", StringComparison.OrdinalIgnoreCase))
                {

                    context.TenantIdOrName = null;
                    context.Handled = false;
                    return;

                }


                var tenant = await tenantRepo.FindByNameAsync(subdomain);

                if (tenant != null)
                {
                    context.TenantIdOrName = tenant.Id.ToString();
                    context.Handled = true;
                    return;
                }
            }
        }
        context.TenantIdOrName = null;
        context.Handled = false;
        return;
    }

    protected override Task<string> GetTenantIdOrNameFromHttpContextOrNullAsync(ITenantResolveContext context, HttpContext httpContext)
    {
        if (!httpContext.Request.Host.HasValue)
        {
            return Task.FromResult<string>(null);
        }


        var hostName = httpContext.Request.Host.Value.RemovePreFix(ProtocolPrefixes);
        var subDomain = hostName.Split('.')[0].ToLower();
        if (subDomain.Equals("admin", StringComparison.OrdinalIgnoreCase) || hostName.Equals("app", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<string>(null);
        }

        var tenantRepo = context.ServiceProvider.GetRequiredService<ITenantRepository>();

        var tenant = tenantRepo.FindByNameAsync(subDomain.ToUpper()).Result;

        if (tenant == null)
            return Task.FromResult<string>(null);


        var extractResult = FormattedStringValueExtracter.Extract(hostName, _domainFormat, ignoreCase: true);

        context.Handled = true;

        return Task.FromResult(extractResult.IsMatch ? extractResult.Matches[0].Value : null);
    }
}