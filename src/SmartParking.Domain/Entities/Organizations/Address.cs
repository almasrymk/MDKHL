using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Organizations
{
    public class Address: FullAuditedEntity<int>, IMultiTenant
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string AddressDetails { get; set; }
        public int CityId { get; set; }
        public int CountryId { get; set; }
        public int StaId { get; set; }
        public int EmaraId { get; set; }
        public Status Status { get; set; }
        public Guid? TenantId { get; set; }

    }
}
