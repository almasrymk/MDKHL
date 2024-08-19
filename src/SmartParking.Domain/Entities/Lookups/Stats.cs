using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Lookups;

public class Stats : BaseEntity
{
    public int EmaraaId { get; set; }
    public Status Status { get; set; }

}
