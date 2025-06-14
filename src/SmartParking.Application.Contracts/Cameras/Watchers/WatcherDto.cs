using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Cameras.Watchers;

public class WatcherDto : AuditedEntityDto<int>
{
    public string Name { get; set; }
    public string DeviceId { get; set; }
    public WatcherType CameraType { get; set; }
    public int GarageId { get; set; }
    public int OrganizationId { get; set; }
    public Status Status { get; set; }
    public Guid? TenantId { get; set; }
}
