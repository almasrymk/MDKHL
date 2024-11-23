using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.GarageTransactions;

public class UnLinkedTransaction : FullAuditedEntity<int>
{
    public string DeviceId { get; set; }
    public string? PlateNumber { get; set; }
    public DateTime? SnapTime { get; set; }
    public DateTime? AccurateTime { get; set; }
    public string VehiclePicture { get; set; }
    public bool IsIn { get; set; }

}
