
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Mobile.Vehicles;
using SmartParking.Results;
using System;

using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
namespace SmartParking.Vehicles;

[Authorize]
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
    private readonly IRepository<LicenceType, int> _licenceTypeRepository;

    public VehiclesAppService(Volo.Abp.Domain.Repositories.IRepository<Vehicle, int> repository,
		IDataFilter dataFilter, ICurrentUser currentUser,IRepository<LicenceType,int> licenceTypeRepository)
	   : base(repository)
	{
		_repository = repository;
		_dataFilter = dataFilter;
		_currentUser = currentUser;
        _licenceTypeRepository = licenceTypeRepository;
    }

    public override async Task<VehicleDto> CreateAsync(CreateUpdateVehicleDto input)
    {
        
        // check plate number must include both plate letters and plate numbers, either in Arabic or English.
        if (!input.ArPlateLetters.IsNullOrEmpty() && !input.ArPlateNumber.IsNullOrEmpty())
        {
            input.PlateNumber = input.ArPlateLetters + input.ArPlateNumber;

        }
        else if (!input.EnPlateLetters.IsNullOrEmpty() && !input.EnPlateNumber.IsNullOrEmpty())
        {
            input.PlateNumber = input.EnPlateLetters + input.EnPlateNumber;
        }
        else
        {
             throw new InvalidOperationException("Plate number must include both plate letters and plate numbers, either in Arabic or English.");
            
        }
        // LicenceType is plate type is found 
        var licenceType = await _licenceTypeRepository.FirstOrDefaultAsync(l => l.Id == input.LicenceTypeId);
        if (licenceType == null)
        {
            return Result<VehicleDto>.NotFound("licence type not found");

        }
        var existingPlatNumber =await _repository.FirstOrDefaultAsync(x=>x.PlateNumber == input.PlateNumber);
        input.PlateColorId = licenceType.PlateColorId;
		if(existingPlatNumber !=null)
		{
			throw new InvalidOperationException("PlatNumber already exists");
		}
        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
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
        if (CurrentTenant.Id == null)
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
        
		return await base.UpdateAsync(id, input);
	}

	public override async Task<VehicleDto> GetAsync(int id)
	{
        
			return await base.GetAsync(id);
	}

	public override async Task DeleteAsync(int id)
	{
		var vehicle = await _repository.FindAsync(id);
		if (vehicle == null )
			throw new EntityNotFoundException(typeof(Vehicle), id);
        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
			{

                //await base.DeleteAsync(id);
                vehicle.Status = Status.Deleted;
                await CurrentUnitOfWork.SaveChangesAsync();
            }
		}
        //await base.DeleteAsync(id);
        vehicle.Status = Status.Deleted;
        await CurrentUnitOfWork.SaveChangesAsync();
    }
}
