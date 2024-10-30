using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages
{
    public class CustomerPackageDto
    {
        public int Id { get; set; }
        public Guid CustomerId { get; set; }
        public int PackageId { get; set; }
        public CustomerPackageStatus CustomerPackageStatus { get; set; }
        public virtual Package Package { get; set; }
        public Guid? TenantId { get; set; }

    }
}
