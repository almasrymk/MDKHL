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
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? Date { get; set; }

}
