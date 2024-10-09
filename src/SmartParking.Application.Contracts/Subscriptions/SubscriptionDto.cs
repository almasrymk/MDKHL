using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Subscriptions;

public class SubscriptionDto : FullAuditedEntityDto<int>
{
    public string SubscriptionName { get; set; }
    public decimal SubscriptionPrice { get; set; }
    public string SubscriptionDescription { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateOnly ActivationDate { get; set; }
    public DateOnly InActivationDate { get; set; }
    public string SubscriptionType { get; set; }
}
