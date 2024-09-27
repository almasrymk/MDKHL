using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Lookups.Region;

public class RegionDto : BaseDto
{
    public int CountryId { get; set; }
    public Status Status { get; set; }
}
