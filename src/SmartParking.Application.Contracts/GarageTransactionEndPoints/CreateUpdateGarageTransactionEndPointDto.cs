using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.GarageTransactionEndPoints
{
    public class CreateUpdateGarageTransactionEndPointDto
    {
        public string DeviceID { get; set; } 
        public string? PlateNumber { get; set; }
        public string? PlateNnumber { get; set; }
        public DateTime? SnapTime { get; set; }
        public DateTime? AccurateTime { get; set; }
        public string VehiclePictureName { get; set; } 
        public string VehiclePicture { get; set; }  
    }
}
