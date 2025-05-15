
using SmartParking.Results;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.VehicleModel
{
	public interface IVehicleModelsAppService : ICrudAppService<VehicleModelDto, int, PagedAndSortedResultRequestDto, CreateUpdateVehicleModelDto>
	{

        /// <summary>
        /// Abstract method to create a new vehicle model.
        /// </summary>
        /// <param name="input">The DTO containing the details of the vehicle model to create.</param>
        /// <returns>A task representing the asynchronous operation, with the result being a <see cref="VehicleModelDto"/> containing the created vehicle model.</returns>
        Task<VehicleModelDto> CreateAsync(CreateUpdateVehicleModelDto input);

        /// <summary>
        /// Abstract method to update an existing vehicle model.
        /// </summary>
        /// <param name="input">The DTO containing the updated details of the vehicle model.</param>
        /// <param name="id">The ID of the specific vehicle model to update.</param>
        /// <returns>A task representing the asynchronous operation, with the result being a <see cref="VehicleModelDto"/> containing the updated vehicle model.</returns>
        Task<VehicleModelDto> UpdateAsync(int id, CreateUpdateVehicleModelDto input);

        /// <summary>
        /// Abstract method to delete a specific vehicle model based on its ID.
        /// </summary>
        /// <param name="id">The ID of the specific vehicle model to delete.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        public Task DeleteAsync(int id);

        /// <summary>
        /// Abstract method to search for vehicle models based on specified filters, sorting, and pagination.
        /// </summary>
        /// <param name="input">The input containing sorting, search, and pagination parameters.</param>
        /// <returns>A task representing the asynchronous operation, with the result being a <see cref="PagedResultDto{VehicleModelDto}"/> containing the list of vehicle models matching the search criteria.</returns>
        public Task<PagedResultDto<VehicleModelDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

        /// <summary>
        /// Abstract method to filter vehicle models based on specific criteria such as sorting and filtering options.
        /// </summary>
        /// <param name="input">The input specifying the maximum number of vehicle models, whether sorting is applied, and any additional filters.</param>
        /// <returns>A task representing the asynchronous operation, with the result being a <see cref="PagedResultDto{VehicleModelDto}"/> containing the filtered vehicle models.</returns>
        public Task<PagedResultDto<VehicleModelDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

        /// <summary>
        /// Asynchronously retrieves all vehicle models associated with a specific brand.
        /// </summary>
        /// <param name="brandId">The ID of the vehicle brand to retrieve the models for.</param>
        /// <returns>A task representing the asynchronous operation, with the result being a <see cref="Result{List{VehicleModelDto}}"/> containing the list of vehicle models for the specified brand.</returns>
        public Task<Result<List<VehicleModelDto>>> GetModelsByBrandAsync(int brandId);

        /// <summary>
        /// Asynchronously retrieves all vehicle models.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with the result being a list of all <see cref="VehicleModelDto"/> objects.</returns>
        Task<List<VehicleModelDto>> GetAllVehicleModelsAsync();

    }
}
