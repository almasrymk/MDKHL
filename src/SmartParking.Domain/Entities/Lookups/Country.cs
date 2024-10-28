using SmartParking.Entities.Lookups;
using SmartParking.Enums;
using System.Collections.Generic;

namespace SmartParking.Entities;

public class Country:BaseEntity
{
    public Country()
    {
        Governorates = new List<Governorate>();
    }
    public string EnglishCapital { get; set; }
    public string ArabicCapital { get; set; }
    public virtual List<Region> Regions { get; set; }
    public virtual List<Governorate> Governorates { get; set; }
    public  Status Status { get; set; }
}
