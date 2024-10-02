using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;

namespace SmartParking.Lookups.VehicleModel
{
	public class VehicleModelDto: FullAuditedEntityDto<int>
	{
		public string ArabicName { get; set; }
		public string EnglishName { get; set; }
		public int VehiclesBrandId { get; set; }
        public string VehicleBrandArabicName { get; set; }
        public string VehicleBrandEnglishName { get; set; }
    }
}
