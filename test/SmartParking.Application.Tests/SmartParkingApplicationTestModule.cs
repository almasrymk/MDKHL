using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute; // This is a tool to create "fakes"
using Volo.Abp.Identity;
using Volo.Abp.Modularity;

namespace SmartParking;

[DependsOn(
    typeof(SmartParkingApplicationModule),
    typeof(SmartParkingDomainTestModule)
)]
public class SmartParkingApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // This helper method registers UserManager, SignInManager, 
        // and all their complex dependencies automatically.
        context.Services.AddIdentity<Volo.Abp.Identity.IdentityUser, Volo.Abp.Identity.IdentityRole>()
            .AddDefaultTokenProviders();

        // If you still get an error about "IHttpContextAccessor", add this:
        context.Services.AddHttpContextAccessor();

        context.Services.AddSingleton(Substitute.For<SmartParking.OpeningGate.IOpenGateFactoryService>());

        Configure<global::OpenIddict.Server.OpenIddictServerOptions>(options =>
        {
            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("test_key_for_openiddict_server_signing_credential_long_enough_123456"));
            options.SigningCredentials.Add(new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256));
            options.JsonWebTokenHandler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
            options.DisableAccessTokenEncryption = true;
        });
    }
}
