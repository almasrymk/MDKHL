using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Cameras;

public class Camera
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string CameraIP { get; set; }
    public string Type { get; set; }
    public int OrganizationGarageId { get; set; }
    public int OrganizationId { get; set; }
    public Status   Status { get; set; }
}
