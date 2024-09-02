using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Customers;

public class Customer:BaseEntity
{
    public Status Status { get; set; }
}
