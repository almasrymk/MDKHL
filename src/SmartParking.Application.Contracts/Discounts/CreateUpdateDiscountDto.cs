using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Discounts;

public class CreateUpdateDiscountDto
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public decimal DiscountPercent { get; set; }
    public string Notes { get; set; }
}
