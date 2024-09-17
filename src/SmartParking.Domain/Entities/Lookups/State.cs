using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Lookups;

public class State : BaseEntity
{
    public int EmaraaId { get; set; }
    public virtual Status Status { get; set; }

}
