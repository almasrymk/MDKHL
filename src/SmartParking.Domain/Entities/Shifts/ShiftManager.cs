using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Values;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Shifts;
[ComplexType]
public class ShiftManager 
{
    public string ManagerEnName { get; set; }
    public string ManagerUserName { get; set; }
    public string ManagerPhoneNumber { get; set; }

}