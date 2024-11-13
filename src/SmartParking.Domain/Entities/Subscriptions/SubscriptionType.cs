using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartParking.Entities.Subscriptions;

public class SubscriptionType : FullAuditedEntity<int> , IBseEntity
{
	public string TypeAr { get; set; }
	public string TypeEn { get; set; }
	public virtual List<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public Status Status { get ; set ; }
	public SubscriptionStatus SubscriptionStatus { get; set; }
}
