using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;

namespace SmartParking.Lookups.VehicleBrand
{
	public class VehicleBrandDto: FullAuditedEntityDto<int>
	{
		public string ArabicName { get; set; }
		public string EnglishName { get; set; }
	}
}
