using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace SmartParking.Blazor.MultiTenants;

public class MultiDomainTenantResolver : TenantResolveContributorBase
{
    public override string Name => "Custom";

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
}