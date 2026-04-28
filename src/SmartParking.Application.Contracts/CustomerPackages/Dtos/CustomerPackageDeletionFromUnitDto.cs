using System;

namespace SmartParking.CustomerPackages.Dtos;

public class CustomerPackageDeletionFromUnitDto
{
    public Guid CustomerId { get; set; }
    public string PlateNumber { get; set; }
    public DateTime ActivationDate { get; set; }
    public DateTime DeactivationDate { get; set; }
}

