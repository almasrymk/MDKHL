using SmartParking.Entities.Cameras;
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

public class GarageTransaction : FullAuditedEntity<int>, IMultiTenant
{

       public virtual Camera Camera { get; set; }
       public int CameraId { get; set; }
       public string? PlateNumber { get; set; } 
       public DateTime? SnapTime { get; set; }
	public DateTime? AccurateTime { get; set; }
       public virtual Organization Organization { get; set; }
       public int OrganizationID { get; set; }
       public virtual Garage Garage { get; set; }
       public int GarageId { get; set; }
       public string VehiclePicture { get; set; }
       public CameraType CameraType { get; set; }
       public Guid? TenantId { get; set; }
       public bool IsIn { get; set; }
       public DateTime? CheckoutMobile { get; set; }
       public bool IsMapped { get; set; }
       public DateTime? RemainingTime { get; set; }
       public bool? IsPending { get; set; }
       public CustomerCategoryStatus CustomerCategory { get; set; }
       public bool CanCheckout { get; set; }




}
