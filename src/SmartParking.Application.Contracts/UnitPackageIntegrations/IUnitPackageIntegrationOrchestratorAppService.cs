using SmartParking.UnitPackageIntegrations.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.UnitPackageIntegrations;

/// <summary>
/// Orchestrator interface for unit package integrations that combines CRUD operations and specialized queries.
/// </summary>
public interface IUnitPackageIntegrationOrchestratorAppService : IApplicationService
{
    /// <summary>
    /// Gets all unit package integrations by booking identifier.
    /// </summary>
    /// <param name="bookingId">The booking identifier to filter by.</param>
    /// <returns>A collection of unit package integration DTOs matching the booking ID.</returns>
    Task<ICollection<UnitPackageIntegrationDto>> GetAllByBookingIdAsync(string bookingId);

    /// <summary>
    /// Creates a new unit package integration.
    /// </summary>
    /// <param name="input">The DTO containing the data to create the unit package integration.</param>
    /// <returns>The created unit package integration DTO.</returns>
    Task<UnitPackageIntegrationDto> CreateAsync(CreateUpdateUnitPackageIntegrationDto input);

    /// <summary>
    /// Updates an existing unit package integration.
    /// </summary>
    /// <param name="id">The identifier of the unit package integration to update.</param>
    /// <param name="input">The DTO containing the updated data.</param>
    /// <returns>The updated unit package integration DTO.</returns>
    Task<UnitPackageIntegrationDto> UpdateAsync(Guid id, CreateUpdateUnitPackageIntegrationDto input);

    /// <summary>
    /// Deletes a unit package integration.
    /// </summary>
    /// <param name="id">The identifier of the unit package integration to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Gets a unit package integration by identifier.
    /// </summary>
    /// <param name="id">The identifier of the unit package integration to retrieve.</param>
    /// <returns>The unit package integration DTO.</returns>
    Task<UnitPackageIntegrationDto> GetAsync(Guid id);

    /// <summary>
    /// Gets a list of unit package integrations with pagination and sorting.
    /// </summary>
    /// <param name="input">The paged and sorted result request parameters.</param>
    /// <returns>A paged list of unit package integration DTOs.</returns>
    Task<PagedResultDto<UnitPackageIntegrationDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    /// <summary>
    /// Determines whether a booking with the specified identifier exists.
    /// </summary>
    /// <param name="bookingId">The booking identifier to check for existence.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if a booking with the specified
    /// identifier exists; otherwise, false.</returns>
    Task<bool> BookingIdExist(string bookingId);
}
