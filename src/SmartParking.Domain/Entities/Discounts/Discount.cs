using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Discounts;

public class Discount : BaseEntity
{
    public decimal DiscountPercent { get; set; }
}
