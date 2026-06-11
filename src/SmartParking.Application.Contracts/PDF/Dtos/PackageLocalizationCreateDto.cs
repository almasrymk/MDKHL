using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF.Dtos;

public class PackageLocalizationCreateDto : BaseLocalizationCreateDto
{
    public string PackageName { get; set; } = string.Empty;

    public string SubscriptionTimeKey { get; set; } = "SubscriptionTime";
    public string ExpirationTimeKey { get; set; } = "ExpirationTime";

    public string DurationKey { get; set; } = "Duration";
    public string DaysKey { get; set; } = "Days";
}
