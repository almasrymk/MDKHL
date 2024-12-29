using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Analysis;

public class GetNumberOfParkingAndEmptyParkingDto
{
    public int NumberOfParking { get; set; }
    public int NumberOfEmptyParking { get; set; }
    public int NumberOfCustomerEmptyParking { get; set; }
    public int NumberOfCustomerParking { get; set; }
    public int NumberOfEmployeeParking { get; set; }
    public int NumberOfEmployeeEmptyParking { get; set; }
    public int NumberOfVisitorEmptyParking { get; set; }
    public int NumberOfVisitorParking { get; set; }
}
