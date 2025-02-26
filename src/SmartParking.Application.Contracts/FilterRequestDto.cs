using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking;

public class FilterRequestDto
{
    public string FilterProperty { get; set; }
    public string FilterValue { get; set; }
    public string FilterOperator { get; set; }
    public bool IsNot { get; set; }
    public TypeFilter TypeFilter { get; set; }
    public DateTime StartDate { get; set; }  = DateTime.MinValue;
    public DateTime EndDate { get; set; } = DateTime.MaxValue;
    public TimeSpan StartTime { get; set; }  = TimeSpan.MinValue;
    public TimeSpan EndTime { get; set; } = TimeSpan.MaxValue;
    public DateTime Date { get; set; } = DateTime.MinValue;
    public TimeSpan Time { get; set; } = TimeSpan.MinValue;
    public IList<string> FilterValues { get; set; } = new List<string>();
    public Type TypeOfColumnFilter { get; set; }

}
