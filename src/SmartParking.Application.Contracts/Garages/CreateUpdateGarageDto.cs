using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartParking.OrganizationsGarages;

public class CreateUpdateGarageDto
{
    public string Code { get; set; }
    public string NameEn { get; set; }
    public string NameAr { get; set; }
    public int MaxCapacity { get; set; }
	public int CurrnetCapicity { get; set; }
	public TimeOnly ClosingHours { get; set; }
    public int OrganizationId { get; set; }
    public Status Status { get; set; }
    [JsonIgnore]
    public  Guid? TenantId { get; set; }
    public int MaxSlotsForCustomers { get; set; }
    public int MaxSlotsForEmployess { get; set; }
    public int MaxSlotsForFree { get; set; }
    public string OpenGateURL { get; set; }


}
