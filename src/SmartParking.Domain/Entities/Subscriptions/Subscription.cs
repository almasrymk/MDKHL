using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Subscriptions;

public class Subscription :BaseEntity
{
    public Status Status { get; set; }
}
