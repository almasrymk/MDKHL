using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartParking.Cameras
{
    public class CreateUpdateCameraDto
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string MainType { get; set; }
        public string BrandName { get; set; }
        public string DeviceId { get; set; }
        public string CameraIP { get; set; }
        public string MacAddress { get; set; }
        public CameraType CameraType { get; set; }
        public int GarageId { get; set; }
        public int OrganizationId { get; set; }
        public Status Status { get; set; }

		[JsonIgnore]
		public Guid? TenantId { get; set; }
    }
}
