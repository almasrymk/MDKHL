using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.Dtos;

public class SearchByPlateAndCustomerIdDto
{
    public Guid CustomerId { get; set; }
    public string EnPlateNumber { get; set; }
}
