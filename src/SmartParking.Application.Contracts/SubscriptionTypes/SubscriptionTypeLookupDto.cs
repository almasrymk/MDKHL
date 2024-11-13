using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.SubscriptionTypes;

public class SubscriptionTypeLookupDto : EntityDto<int>
{
    public string SubscriptionTypeEn { get; set; }
    public string SubscriptionTypeAr { get; set; }

    public Status Status { get; set; }
    public SubscriptionStatus SubscriptionStatus { get; set; }

}
