using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Analysis;

public class CustomerPackageTransactionDto
{
    public int CustomerPackageId { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerPackageStatus CustomerPackageStatus { get; set; }
    public int TransactionId { get; set; }
    public DateTime? AccurateTime { get; set; }
    public int GarageId { get; set; }
    public bool IsIn { get; set; }
    public string? PlateNumber { get; set; }
    public string? LaneNO { get; set; }
    public string? TicketId { get; set; }
}