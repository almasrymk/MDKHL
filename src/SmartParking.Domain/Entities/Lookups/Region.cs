using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System.Collections.Generic;


namespace SmartParking.Entities.Lookups;

public class Region : BaseEntity
{
    public Region()
    {
        Organizations = new List<Organization>();
        State = new List<State>();  
    }
    public int CountryId { get; set; }
    public virtual Country Country { get; set; }
    public virtual ICollection<Organization> Organizations { get; set; }
    public virtual Status Status { get; set; }
    public virtual ICollection<State> State { get; set; }

}
