using System;
using SmartParking.Enums;

namespace SmartParking.CustomerPackages;

public static class CustomerPackageStatusResolver
{
    /// <summary>
    /// Resolves the status to display for a customer package from its stored status and validity dates.
    /// Uses day granularity, matching the gate rule in CustomerPackageExtensions.
    /// </summary>
    /// <param name="storedStatus">The status saved on the customer package.</param>
    /// <param name="activationDate">The package activation date, if any.</param>
    /// <param name="expiryDate">The package expiry date.</param>
    /// <param name="now">The current date and time.</param>
    /// <returns>The effective status of the package at <paramref name="now"/>.</returns>
    public static CustomerPackageStatus Resolve(
        CustomerPackageStatus storedStatus,
        DateTime? activationDate,
        DateTime expiryDate,
        DateTime now)
    {
        if (storedStatus is CustomerPackageStatus.Deleted or CustomerPackageStatus.Expired)
            return storedStatus;

        if (now.Date > expiryDate.Date)
            return CustomerPackageStatus.Expired;

        if (storedStatus == CustomerPackageStatus.InActive)
            return CustomerPackageStatus.InActive;

        if (activationDate.HasValue && now.Date < activationDate.Value.Date)
            return CustomerPackageStatus.InActive;

        return CustomerPackageStatus.Active;
    }
}
