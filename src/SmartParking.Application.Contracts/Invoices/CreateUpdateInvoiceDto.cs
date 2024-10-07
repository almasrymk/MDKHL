using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Invoices;

public class CreateUpdateInvoiceDto 
{
    public string InvoiceNumber { get; set; }
    public int TotalHours { get; set; }
    public decimal InvoicePrice { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public int OrganizationId { get; set; }
    public int GarageId { get; set; }
    public int VehicleId { get; set; }
    // public int PackageId { get; set; }

}
