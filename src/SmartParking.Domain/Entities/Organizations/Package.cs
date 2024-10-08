using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Organizations;

public class Package : BaseEntity
{
    public virtual PackageType PackageType { get; set; }
    public int PackageTypeId { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public bool published { get; set; }
    public TimeOnly PublishFrom { get; set; }
    public TimeOnly PublishTo { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime ActivationDate { get; set; }
    public string ActivedBy { get; set; }
    public DateTime DectivationDate { get; set; }
    public string DeactivatedBy { get; set; }
    public int OrgnizationId { get; set; }
    public Status Status { get; set; }
    public virtual Organization Organization { get; set; }

}
