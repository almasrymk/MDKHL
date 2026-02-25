using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Extensions;

public static class CustomerPackageExtensions
{
    public static IQueryable<CustomerPackage> CountOfVehicleHasActivePackage(this IQueryable<CustomerPackage> query, int organizationId , DateTime? AccurateTime )
    {
        var accurateTime = AccurateTime.Value;
        var timeOfDay = TimeOnly.FromTimeSpan(accurateTime.TimeOfDay);

       return query = query.Where(x =>
            x.OrganizationId == organizationId &&
            x.CustomerPackageStatus == CustomerPackageStatus.Active &&
            x.ActivationDateOfPackage.HasValue &&
            x.ExpiryDate != null &&
            x.ActivationDateOfPackage.Value.Date <= accurateTime.Date &&
            x.ExpiryDate.Date >= accurateTime.Date);

    }
}
