using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.GarageTransactions
{
	public class GarageTransactionDto: FullAuditedEntityDto<int>
	{
		public int CameraId { get; set; }
		public string PlateNumber { get; set; }
		public DateTime SnapTime { get; set; }
		public DateTime AccurateTime { get; set; }
		public int OrganizationID { get; set; }
		public int GarageId { get; set; }
		public string VehiclePicture { get; set; }
        public WatcherType CameraType { get; set; }
        public Guid? TenantId { get; set; }

        public  bool IsIn { get; set; }
        public DateTime? CheckoutMobile { get; set; }
        public bool IsMapped { get; set; }
        public DateTime? RemainingTime { get; set; }
        public bool? IsPending { get; set; }
        public CustomerCategoryStatus CustomerCategory { get; set; }
        public bool CanCheckout { get; set; }
        public string OrganizationArabicName { get; set; }
        public string OrganizationEnglishName { get; set; }
        public string GarageArabicName { get; set; }
        public string GarageEnglishName { get; set; }


    }
}
