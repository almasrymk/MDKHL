using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Analysis;

public class PaymentGroup
{
    public DateTime Date { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public double TotalAmount { get; set; }
    public PaymentCategory PaymentCategory { get; set; }
}
