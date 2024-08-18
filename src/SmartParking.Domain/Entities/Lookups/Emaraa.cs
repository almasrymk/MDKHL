using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System.Collections.Generic;


namespace SmartParking.Entities.Lookups;

public class Emaraa : BaseEntity
{
    public string EnName { get; set; }
    public string ArName { get; set; }
    public int CountryId { get; set; }
    public ICollection<Organization> Organizations { get; set; }
    public Status Status { get; set; }
    public ICollection<Stats> Stats { get; set; }

}
