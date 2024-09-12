using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System.Collections.Generic;


namespace SmartParking.Entities.Lookups;

public class Emaraa : BaseEntity
{
    public Emaraa()
    {
        Organizations = new List<Organization>();
        Stats = new List<Stats>();  
    }
    public int CountryId { get; set; }
   
    public virtual ICollection<Organization> Organizations { get; set; }
    public virtual Status Status { get; set; }
    public virtual ICollection<Stats> Stats { get; set; }

}
