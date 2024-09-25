using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.VehicleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
namespace SmartParking.Vehicles.LookUps;

public class VehicleModelsAppService : CrudAppService<VehicleModel, VehicleModelDto, int, PagedAndSortedResultRequestDto, CreateUpdateVehicleModelDto>, IVehicleModelsAppService
{
	public VehicleModelsAppService(IRepository<VehicleModel, int> repository) : base(repository)
	{

	}
}
