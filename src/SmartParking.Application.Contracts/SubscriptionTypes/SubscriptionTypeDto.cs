using SmartParking.Enums;
using SmartParking.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.SubscriptionTypes;

public class SubscriptionTypeDto : EntityDto<int>
{

	public string TypeAr { get; set; }
	public string TypeEn { get; set; }
	public List<SubscriptionDto> Subscriptions { get; set; }  = new List<SubscriptionDto>();

    public Status Status { get; set; }
    public SubscriptionStatus SubscriptionStatus { get; set; }

}
