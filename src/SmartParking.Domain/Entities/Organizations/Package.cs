using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Organizations;

public class Package : BaseEntity
{
    public int PackageTypeId { get; set; }
    public int CategoriesId { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string published { get; set; }
    public DateTime PublishFrom { get; set; }
    public DateTime PublishTo { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime ActivationDate { get; set; }
    public string ActivedBy { get; set; }
    public DateTime DectivationDate { get; set; }
    public string DeactivatedBy { get; set; }
    public virtual Category Category { get; set; }
    public Status Status { get; set; }
}
