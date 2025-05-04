using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Analysis;
public class CustomerAnalysis
{
    public string BeneficiaryNameAr { get; set; }
    public string BeneficiaryNameEn { get; set; }
    public CustomerCategoryStatus PackageCategory { get; set; }
    public string BeneficiaryStatus { get; set; }
    public string CreatedBy { get; set; }
    public DateOnly? CreationDate { get; set; }
    public TimeOnly? CreationTime { get; set; }
    public string ModifiedBy { get; set; }
    public DateOnly? ModifiedDate { get; set; }
    public TimeOnly? ModifiedTime { get; set; }
}
