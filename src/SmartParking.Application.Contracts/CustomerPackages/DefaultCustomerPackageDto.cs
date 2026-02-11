using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages;

public class DefaultCustomerPackageDto
{
    public Guid CustomerId { get; set; }
    public Guid TenantId { get; set; }
    public int OrganizationId { get; set; }

}
