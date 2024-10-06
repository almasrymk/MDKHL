using SmartParking.Enums;
using System;

namespace SmartParking.Packages
{
    public class CreateUpdatePackageDto
    {
		public string ArabicName { get; set; }
		public string EnglishName { get; set; }
		public int PackageTypeId { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public bool published { get; set; }
        public TimeOnly PublishFrom { get; set; }
        public TimeOnly PublishTo { get; set; }
        public int OrgnizationId { get; set; }
        public Status Status { get; set; }
    }
}
