using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Subscriptions;

public class CreateUpdateSubscriptionDto : CreateUpdateBaseDto
{
    public decimal SubscriptionPrice { get; set; }
    public string SubscriptionDescription { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateOnly ActivationDate { get; set; }
    public DateOnly InActivationDate { get; set; }
    public int NumbersOfGarages { get; set; }
    public int NumberOfCameras { get; set; }
    public int NumberOfUsers { get; set; }
    public int NumberOfMobileCustomers { get; set; }
    public int NumberOfOrganizations { get; set; }
    public int SubscriptionSlotNumber { get; set; }
    public int SubscriptionTypeId { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid CreatorId { get; set; }
}
