using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Lookups;

public class CameraMainType :BaseEntity
{
    public int CameraMainTypesId { get; set; }
    public virtual CameraMainType CameraMainTypes { get; set; }
}
