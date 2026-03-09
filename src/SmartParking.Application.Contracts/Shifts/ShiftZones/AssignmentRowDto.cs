using System;

namespace SmartParking.Shifts.ShiftZones;

/// <summary>
/// Shared UI-facing row model used by both AddShiftZoneDialog and
/// UpdateShiftZoneDialog to represent a single keeper/device pairing.
/// Promoted out of private nested classes so that extension methods can
/// map to and from it without a UI-layer dependency.
/// </summary>
public class AssignmentRowDto
{
    /// <summary>
    /// Set only on rows loaded from existing persisted ShiftDetails.
    /// Null for brand-new rows added by the user.
    /// </summary>
    public Guid? ExistingShiftDetailsId { get; set; }

    public Guid? StaffId { get; set; }

    /// <summary>Entity ID of the ShiftStaffDetails record (update-only).</summary>
    public Guid? StaffEntityId { get; set; }

    public int? DeviceId { get; set; }

    /// <summary>Entity ID of the ShiftDevice record (update-only).</summary>
    public Guid? DeviceEntityId { get; set; }
}