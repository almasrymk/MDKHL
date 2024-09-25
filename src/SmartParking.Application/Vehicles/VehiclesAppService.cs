using Abp.Domain.Repositories;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
namespace SmartParking.Vehicles;

public class VehiclesAppService:
	CrudAppService<
		Vehicle,
		VehicleDto,
		int,
		PagedAndSortedResultRequestDto,
		CreateUpdateVehicleDto
		>,
	IVehiclesAppService
{
	public VehiclesAppService(Volo.Abp.Domain.Repositories.IRepository<Vehicle, int> repository)
	   : base(repository)
	{
		
	}
}
