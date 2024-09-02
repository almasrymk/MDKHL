using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Organizations;

public class GarageCategories
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public int GarageId { get; set; }

    public Status Status { get; set; }
}
