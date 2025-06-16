using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Cameras.Watchers.Dispensers;

public class CreateUpdateDispenserDto : CreateUpdateWatcherDto
{
    public int TotalRemainNumber { get; set; }
}
