using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Mobile.Vehicles;

public class CreateUpdateVehicleMobileDto
{
    public string ArPlateLetters { get; set; }
    public string EnPlateLetters { get; set; }
    public string ArPlateNumber { get; set; }
    public string EnPlateNumber { get; set; }
    public string plateType { get; set; }
    public int ModelId { get; set; }
    public int BrandId { get; set; }
    public string Year { get; set; }

}
