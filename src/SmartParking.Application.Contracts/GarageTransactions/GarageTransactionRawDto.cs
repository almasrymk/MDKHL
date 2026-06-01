using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.GarageTransactions;

// Lightweight DB-projection type
public class GarageTransactionRawDto
{
    public int Id { get; set; }
    public string PlateNumber { get; set; }
    public DateTime? AccurateTime { get; set; }
    public WatcherType CameraType { get; set; }
    public DateTime? SnapTime { get; set; }
    public string TicketId { get; set; }
    public CustomerCategoryStatus CustomerCategory { get; set; }
    public bool IsMapped { get; set; }
    public bool IsIn { get; set; }
    public DateTime? CheckoutMobile { get; set; }
    public string GarageEnglishName { get; set; }
    public string GarageArabicName { get; set; }
    public string OrganizationEnglishName { get; set; }
    public string OrganizationArabicName { get; set; }
    public int? Graceperiod { get; set; }
    public string ArabicName { get; set; }

}