using SmartParking.Entities.Lookups;
using SmartParking.Enums;

namespace SmartParking.Entities;

public class City: BaseEntity
{
    public int StateId { get; set; }
    public virtual State State { get; set; }
    public Status Status { get; set; }
   
}
