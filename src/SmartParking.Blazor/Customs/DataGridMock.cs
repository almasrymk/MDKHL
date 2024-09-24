using System.Collections.Generic;

namespace SmartParking.Blazor.Customs
{
    public static class PeriodicTableData
    {
        public static List<Element> GetElements()
        {
            return new List<Element>
        {
            new Element { Organization  = "MOVENPICK", Country = "Saudi Arabia", Cities = "Al Madina", Position = 1, Status = "Active" },
            new Element { Organization  = "MOVENPICK", Country = "Saudi Arabia", Cities = "Al Madina", Position = 18, Status = "Inactive" },
            new Element { Organization  = "MOVENPICK", Country = "Saudi Arabia", Cities = "Al Madina", Position = 1, Status = "Active" },
            new Element { Organization  = "MOVENPICK", Country = "Saudi Arabia", Cities = "Al Madina", Position = 2, Status = "Active" },
            new Element { Organization  = "MOVENPICK", Country = "Saudi Arabia", Cities = "Al Madina", Position = 13, Status = "Inactive" },
            new Element { Organization  = "MOVENPICK", Country = "Saudi Arabia", Cities = "Al Madina", Position = 14, Status = "Active" },
            // Add more elements as needed...
        };
        }
    }   

}
