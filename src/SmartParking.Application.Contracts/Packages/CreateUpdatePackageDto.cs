using SmartParking.Entities;
using SmartParking.Enums;
using System;
using System.Collections.Generic;

namespace SmartParking.Packages;

public class CreateUpdatePackageDto
{
		public string ArabicName { get; set; }
		public string EnglishName { get; set; }
        public PackageType PackageType { get; set; }
        public int PackageTypeId { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public TimeOnly PublishFrom { get; set; }
        public TimeOnly PublishTo { get; set; }
        public int OrganizationId { get; set; }
        public SubscriptionStatus PackageStatus { get; set; }
        public CustomerCategoryStatus CustomerCategory { get; set; }
        public int DurationOfPackage { get; set; }
        public Guid? TenantId { get; set; }
        public DateTime ActivationDate { get; set; }


}
