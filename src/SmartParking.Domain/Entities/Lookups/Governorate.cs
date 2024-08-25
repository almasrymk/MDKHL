using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace SmartParking.Entities
{
    public class Governorate:BaseEntity
    {
        public Governorate()
        {
            Cities=new List<City>();
        }
        public int CountryId { get; set; }
        public string EnglishCapital { get; set; }
        public string ArabicCapital { get; set; }
        public Country Country { get; set; }
        public List<City> Cities { get; set; }

    }
}
