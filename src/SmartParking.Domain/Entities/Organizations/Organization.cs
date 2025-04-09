using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Subscriptions;
using System;
using System.Collections.Generic;

namespace SmartParking.Entities.Organizations;

public class Organization : BaseEntity
{
    public string Code { get; set; }
    public string CommercialRegistrationNumber { get; set; }
    public DateOnly CRExpiryDate { get; set; }
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
    public string ContactMail { get; set; }
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
    public virtual ICollection<Garage> OrganizationGarages { get; set; } = new HashSet<Garage>();
    public virtual ICollection<Camera> Cameras { get; set; } = new HashSet<Camera>();
    public virtual ICollection<Package> organizationPackages { get; set; } = new HashSet<Package>();
    public virtual ICollection<CustomerPackage> Customers { get; set; } = new HashSet<CustomerPackage>();
    public virtual ICollection<ContactInfo> organizationContactInfos { get; set; } = new HashSet<ContactInfo>();
    public virtual ICollection<CustomerCategory> CustomerCategories { get; set; } = new HashSet<CustomerCategory>();
    public int? SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; }
    public string Address { get; set; }
    public int ActiveGarages { get; set; }
    public int ActiveCameras { get; set; }
    public string OrganizationPicture { get; set; }
    public int BuildingNumber { get; set; }
    public string StreetName { get; set; }
    public int SecondaryNumber { get; set; }
    public int PostalCode { get; set; }
    public string District { get; set; }

    public int UsedSlots { get; set; }
    public int FreeMinutes { get; set; }
    public int Graceperiod { get; set; }

    public string TaxIdentificationNumber { get; set; }
    public bool HasFreeSlots { get; set; }

}
