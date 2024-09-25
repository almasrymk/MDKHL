using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Lookups.PlateColor;
using SmartParking.Lookups.VehicleBrand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
namespace SmartParking.Vehicles.LookUps
{
	public class VehicleBrandsAppService : CrudAppService<VehicleBrand, VehicleBrandDto, int, PagedAndSortedResultRequestDto, CreateUpdateVehicleBrandDto>, IVehicleBrandsAppService
	{
		public VehicleBrandsAppService(IRepository<VehicleBrand, int> repository) : base(repository)
		{
		}
	}
}
