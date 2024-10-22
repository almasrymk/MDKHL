using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Analysis;

public class OrganizationCustomersDto : EntityDto
{
    public  List<string> Names { get; set; }
    public List<double> CustomersCount { get; set; }

}
