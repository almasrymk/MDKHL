using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.SubscriptionTypes;

public class CreateUpdateSubscriptionTypeDto 
{

	public string TypeAr { get; set; }
	public string TypeEn { get; set; }
	public Status Status { get; set; }

}
