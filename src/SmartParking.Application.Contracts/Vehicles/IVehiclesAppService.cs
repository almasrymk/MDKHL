using SmartParking.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
namespace SmartParking.Vehicles
{
	public interface IVehiclesAppService:
		 ICrudAppService<
		VehicleDto,
		int,
		PagedAndSortedResultRequestDto,
		CreateUpdateVehicleDto
		>
	{
        /// <summary>
        /// Abstract method to create a new vehicle.
        /// </summary>
        /// <param name="input">The DTO containing the details of the vehicle to create.</param>
        /// <returns>A task representing the asynchronous operation, with the result being a <see cref="VehicleDto"/> containing the created vehicle.</returns>
        Task<VehicleDto> CreateAsync(CreateUpdateVehicleDto input);

        /// <summary>
        /// Abstract method to retrieve a paginated list of vehicles based on sorting and filtering parameters.
        /// </summary>
        /// <param name="input">The input containing sorting, search, and pagination parameters.</param>
        /// <returns>A task representing the asynchronous operation, with the result being a <see cref="PagedResultDto{VehicleDto}"/> containing the list of vehicles.</returns>
        Task<PagedResultDto<VehicleDto>> GetListAsync(PagedAndSortedResultRequestDto input);

        /// <summary>
        /// Abstract method to update an existing vehicle based on its ID.
        /// </summary>
        /// <param name="id">The ID of the vehicle to update.</param>
        /// <param name="input">The DTO containing the updated details of the vehicle.</param>
        /// <returns>A task representing the asynchronous operation, with the result being a <see cref="VehicleDto"/> containing the updated vehicle.</returns>
        Task<VehicleDto> UpdateAsync(int id, CreateUpdateVehicleDto input);

        /// <summary>
        /// Abstract method to retrieve the details of a specific vehicle based on its ID.
        /// </summary>
        /// <param name="id">The ID of the vehicle to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, with the result being a <see cref="VehicleDto"/> containing the vehicle details.</returns>
        Task<VehicleDto> GetAsync(int id);

        /// <summary>
        /// Abstract method to delete a specific vehicle based on its ID.
        /// </summary>
        /// <param name="id">The ID of the vehicle to delete.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        Task DeleteAsync(int id);

        /// <summary>
        /// Method to split a vehicle's plate number into the numeric and letter components.
        /// </summary>
        /// <param name="plate">The vehicle plate to split.</param>
        /// <param name="numbers">Output parameter containing the numeric part of the plate.</param>
        /// <param name="letters">Output parameter containing the letter part of the plate.</param>
        void SplitPlate(string plate, out string numbers, out string letters);

        /// <summary>
        /// Asynchronously splits a vehicle's plate number into the numeric and letter components.
        /// </summary>
        /// <param name="plate">The vehicle plate to split.</param>
        /// <returns>A task representing the asynchronous operation, with the result being a tuple containing the numeric and letter parts of the plate.</returns>
        Task<(string numbers, string letters)> SplitPlateAsync(string plate);


    }
}
