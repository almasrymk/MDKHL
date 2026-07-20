using System;

namespace SmartParking.Utilities;

/// <summary>
/// Helper utilities for timezone-aware date comparisons.
/// NOTE: Hardcoded to KSA timezone (Arabian Standard Time / Asia/Riyadh, UTC+3) because this entire POS terminal
/// integration feature is explicitly scoped to Geidea's Saudi Arabia Web ECR product (SAMA / ZATCA).
/// If SmartParking ever expands POS terminal integration to other countries, this should be made organization-configurable.
/// </summary>
public static class DateTimeTimeZoneExtensions
{
    private static readonly TimeZoneInfo KsaTimeZone = GetKsaTimeZoneInfo();

    private static TimeZoneInfo GetKsaTimeZoneInfo()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"); // Windows ID
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Riyadh"); // IANA ID (Linux/macOS)
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Riyadh");
        }
    }

    /// <summary>
    /// Converts a UTC DateTime to Saudi Arabia Local Time (UTC+3).
    /// </summary>
    public static DateTime ToKsaTime(this DateTime utcDateTime)
    {
        var utc = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utc, KsaTimeZone);
    }

    /// <summary>
    /// Returns current DateTime in Saudi Arabia Local Time (UTC+3).
    /// </summary>
    public static DateTime KsaNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, KsaTimeZone);

    /// <summary>
    /// Evaluates whether a UTC creation timestamp falls on the same KSA local date as KSA Today.
    /// </summary>
    public static bool IsSameKsaDay(this DateTime utcCreationTime)
    {
        return utcCreationTime.ToKsaTime().Date == KsaNow.Date;
    }
}
