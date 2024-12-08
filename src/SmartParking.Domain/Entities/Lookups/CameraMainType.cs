using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Lookups;

public class CameraMainType :BaseEntity
{
    public int CameraBrandId { get; set; }
    public virtual CameraBrand CameraBrand { get; set; }
}
