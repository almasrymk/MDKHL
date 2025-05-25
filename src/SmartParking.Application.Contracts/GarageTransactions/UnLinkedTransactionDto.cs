using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.GarageTransactions;

public class UnLinkedTransactionDto
{
    public int Id { get; set; }
    public string DeviceId { get; set; }
    public string? PlateNumber { get; set; }
    public DateTime? SnapTime { get; set; }
    public DateTime? AccurateTime { get; set; }
    public string VehiclePicture { get; set; }
    public bool IsIn { get; set; }
    public string OrganizationArabicName { get; set; }
    public string OrganizationEnglishName { get; set; }

}
