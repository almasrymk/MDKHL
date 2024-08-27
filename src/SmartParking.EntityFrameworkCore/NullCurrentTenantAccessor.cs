using System;
using Volo.Abp;
using Volo.Abp.MultiTenancy;

namespace SmartParking
{
    public class NullCurrentTenantAccessor : ICurrentTenantAccessor
    {
        public BasicTenantInfo CurrentTenant { get; } = null;
        public BasicTenantInfo? Current { get; set; }

        public IDisposable Change(Guid? tenantId)
        {
            // No operation, as this is a null implementation
            return NullDisposable.Instance;
        }

        public IDisposable Change(string tenantName)
        {
            // No operation, as this is a null implementation
            return NullDisposable.Instance;
        }
    }

}
