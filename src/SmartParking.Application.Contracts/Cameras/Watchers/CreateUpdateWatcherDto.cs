using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartParking.Cameras.Watchers;

public class CreateUpdateWatcherDto
{
    public string Name { get; set; }
    public string DeviceId { get; set; }
    public WatcherType CameraType { get; set; }
    public int GarageId { get; set; }
    public int OrganizationId { get; set; }
    public Status Status { get; set; }
    [JsonIgnore]
    public Guid? TenantId { get; set; }
}
