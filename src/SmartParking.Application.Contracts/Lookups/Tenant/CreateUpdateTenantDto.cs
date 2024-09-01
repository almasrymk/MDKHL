using System;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Lookups
{
    public class CreateUpdateTenantDto : EntityDto<Guid>
    {
        public string name { get; set; }
        public string normalizedName { get; set; }
    }
}
