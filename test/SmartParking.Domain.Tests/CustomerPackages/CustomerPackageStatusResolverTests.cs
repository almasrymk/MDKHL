using System;
using Shouldly;
using SmartParking.Enums;
using Xunit;

namespace SmartParking.CustomerPackages;

public class CustomerPackageStatusResolverTests
{
    private static readonly DateTime Now = new(2026, 10, 18, 12, 0, 0);

    [Fact]
    public void Resolve_WithinValidityWindow_ShouldBeActive()
    {
        var result = CustomerPackageStatusResolver.Resolve(
            CustomerPackageStatus.Active, new DateTime(2025, 12, 2, 8, 36, 0), new DateTime(2026, 12, 2, 9, 0, 0), Now);

        result.ShouldBe(CustomerPackageStatus.Active);
    }

    [Fact]
    public void Resolve_PastExpiryDate_ShouldBeExpired()
    {
        var result = CustomerPackageStatusResolver.Resolve(
            CustomerPackageStatus.Active, new DateTime(2025, 12, 3, 16, 46, 0), new DateTime(2026, 1, 2, 17, 0, 0), Now);

        result.ShouldBe(CustomerPackageStatus.Expired);
    }

    [Fact]
    public void Resolve_OnExpiryDayAfterExpiryTime_ShouldStillBeActive()
    {
        var result = CustomerPackageStatusResolver.Resolve(
            CustomerPackageStatus.Active, new DateTime(2026, 10, 1), new DateTime(2026, 10, 18, 9, 0, 0), Now);

        result.ShouldBe(CustomerPackageStatus.Active);
    }

    [Fact]
    public void Resolve_DayAfterExpiry_ShouldBeExpired()
    {
        var result = CustomerPackageStatusResolver.Resolve(
            CustomerPackageStatus.Active, new DateTime(2026, 10, 1), new DateTime(2026, 10, 17, 23, 59, 0), Now);

        result.ShouldBe(CustomerPackageStatus.Expired);
    }

    [Fact]
    public void Resolve_BeforeActivationDate_ShouldBeInActive()
    {
        var result = CustomerPackageStatusResolver.Resolve(
            CustomerPackageStatus.Active, new DateTime(2026, 10, 20), new DateTime(2026, 11, 20), Now);

        result.ShouldBe(CustomerPackageStatus.InActive);
    }

    [Fact]
    public void Resolve_StoredInActiveWithinWindow_ShouldBeInActive()
    {
        var result = CustomerPackageStatusResolver.Resolve(
            CustomerPackageStatus.InActive, new DateTime(2026, 10, 1), new DateTime(2026, 11, 1), Now);

        result.ShouldBe(CustomerPackageStatus.InActive);
    }

    [Fact]
    public void Resolve_StoredInActivePastExpiry_ShouldBeExpired()
    {
        var result = CustomerPackageStatusResolver.Resolve(
            CustomerPackageStatus.InActive, new DateTime(2025, 1, 1), new DateTime(2025, 2, 1), Now);

        result.ShouldBe(CustomerPackageStatus.Expired);
    }

    [Fact]
    public void Resolve_StoredExpiredWithinWindow_ShouldStayExpired()
    {
        var result = CustomerPackageStatusResolver.Resolve(
            CustomerPackageStatus.Expired, new DateTime(2026, 10, 1), new DateTime(2026, 12, 1), Now);

        result.ShouldBe(CustomerPackageStatus.Expired);
    }

    [Fact]
    public void Resolve_StoredDeleted_ShouldStayDeleted()
    {
        var result = CustomerPackageStatusResolver.Resolve(
            CustomerPackageStatus.Deleted, new DateTime(2025, 1, 1), new DateTime(2025, 2, 1), Now);

        result.ShouldBe(CustomerPackageStatus.Deleted);
    }

    [Fact]
    public void Resolve_ActivationDateMinValue_ShouldBeActive()
    {
        var result = CustomerPackageStatusResolver.Resolve(
            CustomerPackageStatus.Active, DateTime.MinValue, new DateTime(2027, 8, 31, 23, 59, 59), Now);

        result.ShouldBe(CustomerPackageStatus.Active);
    }

    [Fact]
    public void Resolve_NoActivationDate_ShouldBeActive()
    {
        var result = CustomerPackageStatusResolver.Resolve(
            CustomerPackageStatus.Active, null, new DateTime(2026, 12, 1), Now);

        result.ShouldBe(CustomerPackageStatus.Active);
    }
}
