using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Vehicles;

public class VehicleCustomerReportDto
{
    public int Id { get; set; }
    public string CustomerEnglishName { get; set; }
    public string CustomerArabicName { get; set; }
    public string ModelYear { get; set; }
    public string BrandArabicName { get; set; }
    public string ModelArabicName { get; set; }
    public string BrandEnglishName { get; set; }
    public string ModelEnglishName { get; set; }
    public string ArPlateNumber { get; set; }
    public string EnPlateNumber { get; set; }
    public Guid? TenantId { get; set; }
    public string CreatedBy { get; set; }
    public TimeOnly? CreationTime { get; set; }
    public DateOnly? CreationDate { get; set; }
}
