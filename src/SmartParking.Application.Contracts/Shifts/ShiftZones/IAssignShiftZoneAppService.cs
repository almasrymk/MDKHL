using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Shifts.ShiftZones;

/// <summary>
/// Application service contract for creating and updating assignments of zones to shifts.
/// Implementations handle validation, persistence and any business rules related to shift zone assignments.
/// </summary>
public interface IAssignShiftZoneAppService
{
    /// <summary>
    /// Adds multiple zone assignment entries to the specified shift zone.
    /// </summary>
    /// <param name="shiftZoneId">The unique identifier of the shift zone to which assignments will be added.</param>
    /// <param name="rows">A collection of <c>CreateUpdateZoneAssignmentDto</c> objects describing the assignments to create.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> wrapping a <see cref="bool"/> indicating whether the add operation completed successfully.
    /// The result may contain validation or processing error details when the operation fails.
    /// </returns>
    Task<Result<bool>> AddShiftZoneAssignmentsAsync(
        Guid shiftZoneId,
        ICollection<CreateUpdateZoneAssignmentDto> rows);

    /// <summary>
    /// Updates multiple zone assignment entries for the specified shift zone.
    /// </summary>
    /// <param name="shiftZoneId">The unique identifier of the shift zone whose assignments will be updated.</param>
    /// <param name="rows">A collection of <c>CreateUpdateZoneAssignmentDto</c> objects containing updated assignment data.
    /// Each DTO should include the necessary identifiers to match existing assignments for update.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> wrapping a <see cref="bool"/> indicating whether the update operation completed successfully.
    /// The result may include validation or processing error details when the operation fails.
    /// </returns>
    Task<Result<bool>> UpdateShiftZoneAssignmentsAsync(
        Guid shiftZoneId,
        ICollection<CreateUpdateZoneAssignmentDto> rows);
}