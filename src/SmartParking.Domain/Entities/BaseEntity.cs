using SmartParking.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities
{
    public class BaseEntity : FullAuditedEntity<int>, IMultiTenant, IBseEntity
    {
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        public Guid? TenantId { get; set; }
        public string Notes { get; set; }
        public Status Status { get; set; } = Status.Active;
    }
}
