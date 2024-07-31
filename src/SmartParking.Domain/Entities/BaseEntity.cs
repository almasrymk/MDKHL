using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartParking.Entities
{
    public class BaseEntity : FullAuditedEntity<int>
    {
        [Required]
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        public string Notes { get; set; }

    }
}
