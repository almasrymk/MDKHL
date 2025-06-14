using SmartParking.Cameras.Watchers;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartParking.Cameras;

public class CreateUpdateCameraDto : CreateUpdateWatcherDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public int ModelId { get; set; }
    public int MainTypeId { get; set; }
    public int BrandNameId { get; set; }
    public string CameraIP { get; set; }
    public string MacAddress { get; set; }
}
