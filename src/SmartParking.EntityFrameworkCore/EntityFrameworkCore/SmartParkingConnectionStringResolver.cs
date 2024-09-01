using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SmartParking.EntityFrameworkCore
{
    public class SmartParkingConnectionStringResolver : IConnectionStringResolver, ITransientDependency
    {
        private readonly Lazy<TenantConnectionStringRepository> _tenantConnectionStringRepository;

        private readonly ICurrentTenant _currentTenant;
        private readonly IConfiguration _configuration;
        public SmartParkingConnectionStringResolver(
        Lazy<TenantConnectionStringRepository> tenantConnectionStringRepository,
            ICurrentTenant currentTenant,
            IConfiguration configuration)
        {
            _tenantConnectionStringRepository = tenantConnectionStringRepository;
            _currentTenant = currentTenant;
            _configuration = configuration;
        }

        public string Resolve(string? connectionStringName = null)
        {
            if (_currentTenant.Id.HasValue)
            {
                // Resolve tenant-specific connection string
                var tenantConnectionString =  _tenantConnectionStringRepository.Value.GetConnectionStringAsync(_currentTenant.Id.Value).Result;
                if (!string.IsNullOrEmpty(tenantConnectionString))
                {
                    return tenantConnectionString;
                }

                throw new Exception($"No connection string found for tenant {_currentTenant.Id.Value}.");
            }

            // Fallback to default connection string if no tenant ID
            var defaultConnectionString = _configuration.GetConnectionString("Default");

            if (string.IsNullOrEmpty(defaultConnectionString))
            {
                throw new ArgumentException("Default connection string is not configured.");
            }
            return defaultConnectionString;

        }

        public async Task<string> ResolveAsync(string? connectionStringName = null)
        {
            if (_currentTenant.Id.HasValue)
            {
                // Resolve tenant-specific connection string
                var tenantConnectionString = await _tenantConnectionStringRepository.Value.GetConnectionStringAsync(_currentTenant.Id.Value);
                if (!string.IsNullOrEmpty(tenantConnectionString))
                {
                    return tenantConnectionString;
                }

                throw new Exception($"No connection string found for tenant {_currentTenant.Id.Value}.");
            }

            // Fallback to default connection string if no tenant ID
            var defaultConnectionString = _configuration.GetConnectionString("Default");

            if (string.IsNullOrEmpty(defaultConnectionString))
            {
                throw new ArgumentException("Default connection string is not configured.");
            }

            return defaultConnectionString;
        }
    }
    public class DefaultConnectionStringResolver : IConnectionStringResolver
    {
        private readonly IConfiguration _configuration;

        public DefaultConnectionStringResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(string connectionStringName)
        {
            return _configuration.GetConnectionString(connectionStringName);
        }

        public Task<string> ResolveAsync(string? connectionStringName = null)
        {
            throw new NotImplementedException();
        }
    }
}