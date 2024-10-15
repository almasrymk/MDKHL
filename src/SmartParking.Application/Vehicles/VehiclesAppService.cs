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
	private readonly Volo.Abp.Domain.Repositories.IRepository<Vehicle, int> _repository;
	private IDataFilter _dataFilter;
	private readonly ICurrentUser _currentUser; // Inject ICurrentUser
	public VehiclesAppService(Volo.Abp.Domain.Repositories.IRepository<Vehicle, int> repository,
		IDataFilter dataFilter, ICurrentUser currentUser)
	   : base(repository)
	{
		_repository = repository;
		_dataFilter = dataFilter;
		_currentUser = currentUser;
	}

    public override async Task<VehicleDto> CreateAsync(CreateUpdateVehicleDto input)
    {
		var existingPlatNumber = _repository.FirstOrDefaultAsync(x=>x.PlateNumber == input.PlateNumber);

		if(existingPlatNumber !=null)
		{
			throw new InvalidOperationException("PlatNumber already exists");
		}
        if (_currentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.CreateAsync(input);
            }
        }
        else
            return await base.CreateAsync(input);
    }
    public override async Task<PagedResultDto<VehicleDto>> GetListAsync(PagedAndSortedResultRequestDto input)
	{
		if (_currentUser.IsInRole("admin"))
		{
			using (_dataFilter.Disable<IMultiTenant>())
			{
				return await base.GetListAsync(input);
			}
		}
		else
			return await base.GetListAsync(input);
	}
	public override async Task<VehicleDto> UpdateAsync(int id, CreateUpdateVehicleDto input)
	{
		if (_currentUser.IsInRole("admin"))
		{
			using (_dataFilter.Disable<IMultiTenant>())
			{
				return await base.UpdateAsync(id, input);
				//return await base.UpdateAsync(id, input);
			}
		}
		return await base.UpdateAsync(id, input);
	}

	public override async Task<VehicleDto> GetAsync(int id)
	{
		if (_currentUser.IsInRole("admin"))
		{
			using (_dataFilter.Disable<IMultiTenant>())
			{
				return await base.GetAsync(id);
			}
		}
		else
			return await base.GetAsync(id);
	}

	public override async Task DeleteAsync(int id)
	{
		var vehicle = await GetAsync(id);
		if (vehicle == null)
			throw new EntityNotFoundException(typeof(Vehicle), id);
		if (_currentUser.IsInRole("admin"))
		{
			using (_dataFilter.Disable<IMultiTenant>())
			{
				await base.DeleteAsync(id);

			}
		}
		await base.DeleteAsync(id);
	}
}
