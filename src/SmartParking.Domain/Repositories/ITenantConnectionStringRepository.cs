using SmartParking.Entities;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Repositories
{
    public interface ITenantConnectionStringRepository : IRepository<TenantConnectionString, Guid>
    {
        Task<string> GetConnectionStringAsync(Guid tenantId);
    }
}
