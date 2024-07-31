using System.Collections.Generic;

namespace SmartParking.Entities
{
    public class Country:BaseEntity
    {
        public Country() 
        {
            Governorates = new List<Governorate>();
        }
        public List<Governorate> Governorates { get; set; }
    }
}
