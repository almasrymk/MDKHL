using SmartParking.Entities.Lookups;
using SmartParking.Enums;
using System.Collections.Generic;

namespace SmartParking.Entities;

public class Country:BaseEntity
{

    public string EnglishCapital { get; set; }
    public string ArabicCapital { get; set; }
    public virtual ICollection<Region> Regions { get; set; } = new List<Region>();
    public  Status Status { get; set; }
}
