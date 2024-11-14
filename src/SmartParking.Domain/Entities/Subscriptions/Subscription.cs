using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Subscriptions;

public class Subscription : BaseEntity
{
    public decimal SubscriptionPrice { get; set; }
    public string SubscriptionDescription { get; set; }
    public SubscriptionStatus StatusSubscription { get; set; } = SubscriptionStatus.Draft;
    public DateOnly ActivationDate { get; set; }
    public DateOnly InActivationDate { get; set; }
    public int NumbersOfGarages { get; set; }
    public int NumberOfCameras { get; set; }
    public int NumberOfUsers { get; set; }
    public int NumberOfCustomers { get; set; }
    public int SubscriptionSlotNumber { get; set; }
    public int SubscriptionTypeId { get; set; }
    public int ParkingSlot { get; set; }
    public virtual SubscriptionType SubscriptionType { get; set; }
    public virtual ICollection<Organization> Organizations { get; set; }
}
