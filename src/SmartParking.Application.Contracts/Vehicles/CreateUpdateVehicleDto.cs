using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Vehicles
{
	public class CreateUpdateVehicleDto  
    {
		public int VehicleModelId { get; set; }
        public string ArPlateLetters { get; set; } = "";
        public string EnPlateLetters { get; set; } = "";
        public string FullEnPlate
        {
            get {
               return EnPlateNumber.Replace(" ", string.Empty)+ EnPlateLetters.Replace(" ", string.Empty);
            }
        }

        public string FullArPlate
        {
            get
            {
                return ArPlateNumber.Replace(" ", string.Empty)+ArPlateLetters.Replace(" ", string.Empty) ;
            }
        }
        public string ArPlateNumber { get; set; } = "";
        public string EnPlateNumber { get; set; } = "";
        public Guid CustomerId { get; set; }
        public int VehicleBrandId { get; set; }
		public int PlateColorId { get; set; }
		public int LicenceTypeId { get; set; }
		public string ModelYear { get; set; }
		public bool BelongToCompany { get; set; }
		public string? CompanyName { get; set; }
    }
}
