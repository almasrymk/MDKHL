
using SmartParking.Results;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
namespace SmartParking.Lookups.VehicleBrand
{
	public interface IVehicleBrandsAppService : ICrudAppService<VehicleBrandDto, int, PagedAndSortedResultRequestDto, CreateUpdateVehicleBrandDto>
	{


        /// <summary>
        /// Abstract method to create a new vehicle brand.
        /// </summary>
        /// <param name="input">The DTO containing the details of the vehicle brand to be created.</param>
        /// <returns>A task representing the asynchronous operation. The result contains the <see cref="VehicleBrandDto"/> for the created vehicle brand.</returns>
        public Task<VehicleBrandDto> CreateAsync(CreateUpdateVehicleBrandDto input);

        /// <summary>
        /// Abstract method to update a specific vehicle brand.
        /// </summary>
        /// <param name="input">The DTO containing the updated details of the vehicle brand.</param>
        /// <param name="id">The ID of the vehicle brand to update.</param>
        /// <returns>A task representing the asynchronous operation. The result contains the <see cref="VehicleBrandDto"/> for the updated vehicle brand.</returns>
        public Task<VehicleBrandDto> UpdateAsync(int id, CreateUpdateVehicleBrandDto input);

        /// <summary>
        /// Abstract method to search vehicle brands based on specified filters and sorting options.
        /// </summary>
        /// <param name="input">The DTO containing paging, sorting, and search parameters.</param>
        /// <returns>A task representing the asynchronous operation. The result contains a <see cref="PagedResultDto{VehicleBrandDto}"/> containing the filtered vehicle brands.</returns>
        public Task<PagedResultDto<VehicleBrandDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

        /// <summary>
        /// Abstract method to filter vehicle brands based on specified max number, sorting, and filtering parameters.
        /// </summary>
        /// <param name="input">The DTO specifying the maximum number of vehicle brands to retrieve, along with sorting and filtering criteria.</param>
        /// <returns>A task representing the asynchronous operation. The result contains a <see cref="PagedResultDto{VehicleBrandDto}"/> with the filtered vehicle brands.</returns>
        public Task<PagedResultDto<VehicleBrandDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

        /// <summary>
        /// Asynchronously deletes a vehicle brand by its ID.
        /// </summary>
        /// <param name="id">The ID of the vehicle brand to delete.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        public Task DeleteAsync(int id);

        /// <summary>
        /// Asynchronously retrieves a list of vehicle brands.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a list of <see cref="VehicleBrandDto"/> objects representing the vehicle brands.
        /// </returns>
        Task<Result<List<VehicleBrandDto>>> GetbrandsAsync();

        /// <summary>
        /// Asynchronously retrieves all vehicle brands.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a list of all <see cref="VehicleBrandDto"/> objects representing the vehicle brands.
        /// </returns>
        Task<List<VehicleBrandDto>> GetAllVehicleBrandsAsync();

    }
}
