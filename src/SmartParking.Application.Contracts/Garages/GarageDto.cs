using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartParking.Garages;

public class GarageDto
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public int MaxCapacity { get; set; }
	public int CurrnetCapicity { get; set; }
    public  string OrganizationNameAr { get; set; }
    public  string OrganizationNameEn { get; set; }
    public TimeOnly ClosingHours { get; set; }
    public int OrganizationId { get; set; }
    public Status Status { get; set; }
	public Guid? TenantId { get; set; }
    public int MaxSlotsForCustomers { get; set; }
    public int MaxSlotsForEmployess { get; set; }
    public int MaxSlotsForFree { get; set; }

}
