using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Enums;

public struct DateInt()
{
    public int Hours { get; set; }
    public int TotalHours { get; set; }
    public int Minutes { get; set; }
    public int DayOfYear { get; set; }
    public DateTime Date { get; set; }

    public TimeSpan TimeOfDay { get; set; }
};
