using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace SmartParking.Entities.Customers;

public class Customer : FullAuditedEntity<Guid> , IMultiTenant
{
    public Guid IdentityUserId { get; set; }
    public virtual IdentityUser IdentityUser { get; set; }
    public string OTP { get; set; }
    public DateTime OtpExpirationDate { get; set; }
    public IdentificationTypes IdentificationTypes { get; set; }
    public string NationalId { get; set; }
    public string ArabicName { get; set; }
    public virtual ICollection<Organization> Organizations { get; set; }
    public virtual ICollection<CustomerPackage> CustomerPackages { get; set; }
    public virtual ICollection<Invoice> Invoices { get; set; }
    public Guid? TenantId { get; set; }
}
