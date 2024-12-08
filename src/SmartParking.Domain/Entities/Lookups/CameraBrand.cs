using SmartParking.Entities.Lookups.VehiclesLookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Lookups;

public class CameraBrand : BaseEntity
{
    public virtual ICollection<CameraModel> Models { get; set; }
}
