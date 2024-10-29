using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Lookups;

public class State : BaseEntity
{
    public int RegionId { get; set; }
    public virtual Region Region { get; set; }
    public  Status Status { get; set; }

}
