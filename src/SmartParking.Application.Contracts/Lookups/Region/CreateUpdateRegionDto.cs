using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Lookups.Region;

public class CreateUpdateRegionDto 
{
	public string ArabicName { get; set; }
	public string EnglishName { get; set; }
	public int CountryId { get; set; }
    public Status Status { get; set; }
}

