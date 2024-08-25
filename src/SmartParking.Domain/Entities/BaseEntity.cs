using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities
{
    public class BaseEntity : FullAuditedEntity<int>, IMultiTenant
    {
        public int Id { get; set; }
        [Required]
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        public string Notes { get; set; }
        public Guid? TenantId { get; }
    }
}
