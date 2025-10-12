using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF;

public class InvoiceLocalizationCreateDto : BaseLocalizationCreateDto
{
    public string ImagePath { get; set; } = string.Empty;

    // Invoice-specific keys
    public string InvoiceKey { get; set; } = "Invoice";
    public string TicketIdKey { get; set; } = "TicketId";
    public string PlateNumberKey { get; set; } = "PlateNumber";
    public string EntranceTimeKey { get; set; } = "EntranceTime";
    public string ExitTimeKey { get; set; } = "ExitTime";
    public string SubscriptionKey { get; set; } = "Subscription";
    public string HourKey { get; set; } = "Hour";
    public string DurationKey { get; set; } = "TotalHours";
    public string HoursKey { get; set; } = "Hours";
}
