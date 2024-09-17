//using SmartParking.Entities;
//using SmartParking.Repositories;
//using System;
//using System.Threading.Tasks;
//using Volo.Abp.DependencyInjection;

//namespace SmartParking.Logic
//{
//    public class TenantCreator : ITransientDependency
//    {
//        private readonly ITenantConnectionStringRepository _tenantConnectionStringRepository;

//        public TenantCreator(ITenantConnectionStringRepository tenantConnectionStringRepository)
//        {
//            _tenantConnectionStringRepository = tenantConnectionStringRepository;
//        }

//        public async Task CreateTenantAsync(TenantConnectionString tenant )
//        {
//            var tenantConnectionString = new TenantConnectionString(
//                Guid.NewGuid(),
//                tenant.TenantId,
//                tenant.IpAddress,
//                tenant.Password,
//                tenant.UserName,
//                tenant.Password,
//                tenant.Port
//            );

//            await _tenantConnectionStringRepository.InsertAsync(tenantConnectionString);
//        }
//    }

//}
