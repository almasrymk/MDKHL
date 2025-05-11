using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Analysis;

public class FinalResult
{
    public DateTime Day { get; set; }
    public string DayOfWeek { get; set; }
    public double AllTotalAmount { get; set; }
    public double PackageTotalAmount { get; set; }
    public double InvoiceTotalAmount { get; set; }
}
