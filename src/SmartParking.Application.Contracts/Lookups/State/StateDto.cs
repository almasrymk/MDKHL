using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Lookups.State;

public class StateDto : BaseDto
{
	public int RegionId { get; set; }
    public Status Status { get; set; }
}
