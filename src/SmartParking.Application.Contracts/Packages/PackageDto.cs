using SmartParking.Entities;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;

namespace SmartParking.Packages
{
    public class PackageDto
    {
        public int Id { get; set; }
        public string ArabicName { get; set; }
		public string EnglishName { get; set; }
        public PackageType PackageType { get; set; }

        public int PackageTypeId { get; set; }
        public string PackageTypeArabicName { get; set; }
        public string PackageTypeEnglishName { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public bool published { get; set; }
        public TimeOnly PublishFrom { get; set; }
        public TimeOnly PublishTo { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ActivationDate { get; set; }
        public string ActivedBy { get; set; }
        public DateTime DectivationDate { get; set; }
        public string DeactivatedBy { get; set; }
        //public  Organization Organization { get; set; }

        public int OrganizationId { get; set; }
        public string OrganizationArabicName { get; set; }
        public string OrganizationEnglishName { get; set; }
        public Status Status { get; set; }
        public CustomerCategoryStatus CustomerCategory { get; set; }
    }
}
