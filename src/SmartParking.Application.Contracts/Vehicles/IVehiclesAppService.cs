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
        Task<VehicleDto> CreateAsync(CreateUpdateVehicleDto input);

        Task<PagedResultDto<VehicleDto>> GetListAsync(PagedAndSortedResultRequestDto input);
		Task<VehicleDto> UpdateAsync(int id, CreateUpdateVehicleDto input);

		Task<VehicleDto> GetAsync(int id);
		Task DeleteAsync(int id);
	}
}
