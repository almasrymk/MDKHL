using System;
using Volo.Abp.Application.Dtos;

namespace SmartParking
{
    public class BaseDto :FullAuditedEntityDto<int>
    { 
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        public string Notes { get; set; }
        public Guid? TenantId { get; set; }
    }
}
