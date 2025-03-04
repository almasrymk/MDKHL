using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Discounts;

public class CreateUpdateDiscountDto : CreateUpdateBaseDto
{

    public decimal DiscountPercent { get; set; }
    public Status Status { get; set; }


}
