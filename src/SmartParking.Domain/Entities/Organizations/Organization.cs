using Microsoft.AspNet.Identity.EntityFramework;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Subscriptions;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Organizations;

public class Organization : BaseEntity
{
    public string Code { get; set; }
    public string UnifiedLicenseNum { get; set; }
    public DateOnly CRExpiryDate { get; set; }
    public DateOnly LicenseExpiryDate { get; set; }
    public int CountryId { get; set; }
    public virtual Country Country { get; set; }
    public int RegionId { get; set; }
    public virtual Region Region { get; set; }
    public int StateId { get; set; }
    public virtual State State { get; set; }
    public int CityId { get; set; }
    public virtual City City { get; set; }
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string  ContactMail { get; set; }
    public decimal FirstHourPrice { get; set; }
    public decimal SecondHourPrice { get; set; }
    public decimal ThirdHourPrice { get; set; }
    public decimal AdditionalHourPrice { get; set; }
    public decimal FullDayPrice { get; set; }
    public bool IsNumberOfHoursExceeded { get; set; }
    public int NumberOfHours { get; set; }
    public bool IsClose { get; set; }
    public TimeOnly ClosingHoursStart { get; set; }
    public TimeOnly ClosingHoursEnd { get; set; }
    public Status Status { get; set; } = Status.Active;
    public virtual ICollection<Garage> OrganizationGarages { get; set; }
    public virtual ICollection<Camera> Cameras { get; set; }
    public virtual ICollection<Package> organizationPackages { get; set; }
    public virtual ICollection<Customer> Customers { get; set; }
    public virtual ICollection<ContactInfo> organizationContactInfos { get; set; }
    public virtual ICollection<CustomerCategory> CustomerCategories { get; set; }
    public int? SubscriptionId { get; set; }
    public virtual Subscription  Subscription { get; set; }


}
