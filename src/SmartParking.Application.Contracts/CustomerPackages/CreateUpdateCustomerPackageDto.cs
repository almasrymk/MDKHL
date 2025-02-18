using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages
{
    public class CreateUpdateCustomerPackageDto
    {
        public Guid CustomerId { get; set; }
        public int PackageId { get; set; }
        public CustomerPackageStatus CustomerPackageStatus { get; set; }
        public Guid? TenantId { get; set; }
        public string EnPlateNumber { get; set; }
        public string ArPlateNumber { get; set; }
    }
}
