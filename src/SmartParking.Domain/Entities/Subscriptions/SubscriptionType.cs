using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartParking.Entities.Subscriptions;

public class SubscriptionType : FullAuditedEntity<int>
{
    public string Type { get; set; }

}
