using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Analysis;

public class GarageTransactionAnalysis
{
    public GarageTransaction GarageTransaction { get; set; }
    public Vehicle  Vehicle { get; set; }
    public Customer Customer { get; set; }
    public Garage Garage { get; set; }
    public string GarageName{ get; set; }
    public string GarageNameAr { get; set; }
    public string UserName { get; set; }
    public CameraType CameraType { get; set; }
    public string PlateNumber { get; set; }
    public DateTime? AccurateTime { get; set; }
    public string OrganizationName { get; set; }
    public string OrganizationNameAr{ get; set; }
    public string ArabicName { get; set; }
    public Guid CustomerId { get; set; }
    public string ShiftArabicName { get; set; }
    public string ShiftEnglishName { get; set; }
    public string CreatorName { get; set; }
    public DateOnly? CreationDate { get; set; }
    public TimeOnly? CreationTime { get; set; }
    public DateOnly? TransactionDate { get; set; }
    public TimeOnly? TransactionTime { get; set; }
}
