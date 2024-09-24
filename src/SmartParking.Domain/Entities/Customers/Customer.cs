using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace SmartParking.Entities.Customers;

public class Customer :IdentityUser
{
    public string NationalId { get; set; }
    public string MobileNumber { get; set; }
    public string FullName { get; set; }
    public virtual ICollection<Vehicle> Vehicles { get; set; }
}

public class Vehicle : BaseEntity
{
    public string LicensePlate { get; set; }
    public string Model { get; set; }
    public string Make { get; set; }
    public int Year { get; set; }
    public Guid CustomerId { get; set; }
    public  virtual Customer Customer { get; set; }
}