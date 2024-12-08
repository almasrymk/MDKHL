using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Lookups.CameraMainType;

public class CreateUpdateCameraMainTypeDto
{
    public int CameraBrandId { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
}
