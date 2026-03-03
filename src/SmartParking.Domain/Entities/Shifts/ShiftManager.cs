using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParking.Entities.Shifts;
[ComplexType]
public class ShiftManager 
{
    public Guid ShiftManagerId { get; set; }
    public string ManagerEnName { get; set; }
    public string ManagerUserName { get; set; }
    public string ManagerPhoneNumber { get; set; }

}