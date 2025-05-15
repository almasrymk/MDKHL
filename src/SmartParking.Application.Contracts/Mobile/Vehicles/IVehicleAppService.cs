using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Results;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Mobile.Vehicles;

public interface IVehicleAppService : IApplicationService
{
    /// <summary>
    /// Creates a new vehicle entry.
    /// </summary>
    /// <param name="input">The data required to create a new vehicle.</param>
    /// <returns>A <see cref="Result{VehicleDto}"/> containing the created vehicle.</returns>
    Task<Result<VehicleDto>> CreateAsync(CreateUpdateVehicleMobileDto input);

    /// <summary>
    /// Edits an existing vehicle entry.
    /// </summary>
    /// <param name="vehicleId">The unique identifier of the vehicle to be edited.</param>
    /// <param name="input">The new data for the vehicle to update.</param>
    /// <returns>A <see cref="Result{VehicleDto}"/> containing the updated vehicle.</returns>
    Task<Result<VehicleDto>> EditAsync(int vehicleId, CreateUpdateVehicleMobileDto input);

    /// <summary>
    /// Deletes a vehicle by its unique identifier.
    /// </summary>
    /// <param name="vehicleId">The unique identifier of the vehicle to be deleted.</param>
    /// <returns>A <see cref="Result{VehicleDto}"/> indicating whether the delete operation was successful or not.</returns>
    Task<Result<VehicleDto>> DeleteAsync(int vehicleId);

    /// <summary>
    /// Retrieves a list of all vehicles.
    /// </summary>
    /// <returns>A <see cref="Result{List{VehicleDto}}"/> containing the list of vehicles.</returns>
    Task<Result<List<VehicleDto>>> GetListAsync();

    /// <summary>
    /// Retrieves a vehicle by its plate number.
    /// </summary>
    /// <param name="plateNumber">The plate number of the vehicle to retrieve.</param>
    /// <returns>The <see cref="VehicleDto"/> for the vehicle with the given plate number.</returns>
    Task<VehicleDto> GetVehicleByPlateNumberAsync(string plateNumber);

    /// <summary>
    /// Retrieves a vehicle by its unique identifier.
    /// </summary>
    /// <param name="vehicleId">The unique identifier of the vehicle.</param>
    /// <returns>A <see cref="Result{VehicleDto}"/> containing the vehicle if found, or an error result.</returns>
    Task<Result<VehicleDto>> GetAsync(int vehicleId);
}
