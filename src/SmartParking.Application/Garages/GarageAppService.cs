using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Nito.Disposables;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.EnumDescriptions;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;

namespace SmartParking.Garages;

[Authorize]
public class GarageAppService : CrudAppService<Garage, GarageDto, int, PagedAndSortedResultRequestDto, CreateUpdateGarageDto>, IGarageAppService
{
    private readonly IRepository<Garage,int> _repository;
    private readonly IRepository<Camera,int> _repositoryCamera;
    private readonly IRepository<Organization,int> _organizationRepository;
    private readonly OrganizationsAppService _organizationsAppService;
    private IDataFilter _dataFilter;
    private ICurrentUser _currentUser;

    public ICurrentTenant _currentTenant;

    public GarageAppService(IRepository<Garage, int> repository, OrganizationsAppService organizationsAppService, IDataFilter datafilter, ICurrentUser currentUser, ICurrentTenant currentTenant, IRepository<Camera, int> repositoryCamera, IRepository<Organization, int> organizationRepository) : base(repository)
    {
        _repository = repository;
        _organizationsAppService = organizationsAppService;
        _dataFilter = datafilter;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _repositoryCamera = repositoryCamera;
        _organizationRepository = organizationRepository;
    }




    /// <inheritdoc/>

    public override async Task<GarageDto> CreateAsync(CreateUpdateGarageDto input)
    {
        Organization organization=new Organization();
        using (DataFilter.Disable<IMultiTenant>())
        {
             organization = await _organizationRepository.GetAsync(input.OrganizationId);
        }
        if (organization == null)
        {
            throw new EntityNotFoundException(typeof(Organization), input.OrganizationId);
        }
        if (organization.Status == Enums.Status.Inactive)
        {
            throw new Exception("Can't add garage to In active organization.");
            
        }
        string code = organization.Code;
        var numberOfGaragesForOrganization = await _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId);

       if( organization.Subscription.NumbersOfGarages < numberOfGaragesForOrganization.Count + 1)
       {
            throw new Exception("UPGRADE subscription to add garage to organization.");

       }

        input.Code = code + GetOrganizationCode(numberOfGaragesForOrganization.Count);
        if (input.Status == Enums.Status.Active)
        {
            organization.ActiveGarages++;
        }
		input.TenantId=organization.TenantId;
		await CurrentUnitOfWork.SaveChangesAsync();
        return await base.CreateAsync(input);
    }

    [RemoteService(isEnabled: false)]

    /// <summary>
    /// Method is used to create a code for garage
    /// </summary>
    /// <param name="input">input is garage id which is related to specific organization </param>
    /// <returns></returns>
    /// <exception cref="BusinessException">it throws exception when organization tries to add more than 99 garage</exception>

    public string GetOrganizationCode(int input)
    {
        if (input == 99)
        {
            throw new BusinessException("Can't add more garages for that organization");
        }
        string Code = "-G-";
        if (input < 9)
        {
            Code += '0';
        }
        Code += (input + 1);
        return Code;
    }
    

    /// <inheritdoc/>
    public async override Task<GarageDto> UpdateAsync(int id, CreateUpdateGarageDto input)
    {
        Organization organization=new Organization();
        Garage garage=new Garage();
        int NumberOfGaragesInInputOrganization = 0;

        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {

            using (DataFilter.Disable<IMultiTenant>())
            {
                organization = await _organizationRepository.GetAsync(input.OrganizationId);
                garage = await _repository.GetAsync(id);
                var result = await _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId);
                NumberOfGaragesInInputOrganization=result.Count;

            }
        }
        if (organization == null)
        {
            throw new EntityNotFoundException(typeof(Organization), input.OrganizationId);
        }
        if (garage.OrganizationId != input.OrganizationId)
        {
            input.TenantId=organization.TenantId;
            input.Code=organization.Code;
            input.Code += GetOrganizationCode(NumberOfGaragesInInputOrganization);
            
        }
        if(input.Status == Enums.Status.Inactive && garage.Status==Enums.Status.Active)
        {
            organization.ActiveGarages--;
        }
        if(input.Status == Enums.Status.Active && garage.Status==Enums.Status.Inactive)
        {
            organization.ActiveGarages++;
        }

		await CurrentUnitOfWork.SaveChangesAsync();
		if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {

            using (DataFilter.Disable<IMultiTenant>())
            {
                return await base.UpdateAsync(id,input);
            }

        }
        return await base.UpdateAsync(id, input);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<GarageDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<garage> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.Code.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.NameEn.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.NameAr.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.MaxCapacity.ToString().Contains(input.SearchingTerm) 
               
            );
        }
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
            }
            else
            {
                queryable = queryable.OrderBy(input.Sorting);
            }

        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of hotel entities
        var garages = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var garagesDto = ObjectMapper.Map<List<Garage>, List<GarageDto>>(garages);
        Console.WriteLine(garagesDto.Count);
        // Return the paginated result
        return new PagedResultDto<GarageDto>(
            totalCount,
            garagesDto
        );
    }


    /// <inheritdoc/>
    public async Task<PagedResultDto<GarageDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<garage> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<Garage>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
            }
            else
            {
                queryable = queryable.OrderBy(input.Sorting);
            }

        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of garages entities
        var garages = await AsyncExecuter.ToListAsync(queryable);

        // Map the garages entities to garagesDto objects
        var garagesDto = ObjectMapper.Map<List<Garage>, List<GarageDto>>(garages);
        Console.WriteLine(garagesDto.Count);
        // Return the paginated result
        return new PagedResultDto<GarageDto>(
            totalCount,
            garagesDto
        );
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<GarageDto>> GetGarageByOrganization(PagedAndSortedResultRequestDto input ,int organizationId)
    {

            var query = await _repository.GetQueryableAsync();
             query=  query.Where(x => x.OrganizationId == organizationId);
            var totalCount = await AsyncExecuter.CountAsync(query);
            query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
            query.OrderBy(x => x.Code);
            var garages = await AsyncExecuter.ToListAsync(query);
            var garagesDto = ObjectMapper.Map<List<Garage>, List<GarageDto>>(garages);
            return new PagedResultDto<GarageDto>(
               totalCount,
               garagesDto
           );
        
    }

    /// <inheritdoc/>
    public async override Task<PagedResultDto<GarageDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.GetListAsync(input);
            }
        }
        else
            return await base.GetListAsync(input);
    }

    [RemoteService(IsEnabled = false)]

    public async Task<int> GetCountWithountMultiTenant()
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {

            var query = await _repository.GetQueryableAsync();
            var count = await AsyncExecuter.CountAsync(query);
            return count;
        }



    }
    /// <inheritdoc/>

    public override async Task DeleteAsync(int id)
    {
        
        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {

            using (DataFilter.Disable<IMultiTenant>())
            {
				Garage garage;
				using (DataFilter.Disable<IMultiTenant>())
				{
					garage = await _repository.FirstOrDefaultAsync(x => x.Id == id);

				}

				if (garage == null)
				{
					throw new Exception("There is no garage related to that id");
				}
				var org = await _organizationRepository.GetAsync(garage.OrganizationId);
				if (org != null)
				{
					org.ActiveGarages--;
				}

				if (garage.Status == Status.Deleted)
				{
					throw new Exception("Can't Delete an deleted Garage");

				}
				garage.Status = Status.Deleted;
				await base.DeleteAsync(id);
            }

        }
        else
        {

         Garage garage;
	     garage = await _repository.FirstOrDefaultAsync(x => x.Id == id);

		

		if (garage == null)
		{
			throw new Exception("There is no garage related to that id");
		}
		var org = await _organizationRepository.GetAsync(garage.OrganizationId);
		if (org != null)
		{
			org.ActiveGarages--;
		}

		if (garage.Status == Status.Deleted)
		{
			throw new Exception("Can't Delete an deleted Garage");

		}
		garage.Status = Status.Deleted;
		await base.DeleteAsync(id);

        }


    }
	/// <inheritdoc/>

	public async Task<int> GetAllGaragesCount()
    {

        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {

            var countt= await GetCountWithountMultiTenant();
            return countt;
            
        }
        

            var query = await _repository.GetQueryableAsync();
            var count = await AsyncExecuter.CountAsync(query);
            return count;
        
    }
	/// <inheritdoc/>

	public async Task<bool> ChangeTenantForCamerasRelatedToGarage(int GarageID, Guid TenantId)
	{
        //get list of cameras of current tenant
        List<Camera>cameras = new List<Camera>();
        if (CurrentTenant.Id == null)
        {
            //if current tenant is MDKHL
            using (_dataFilter.Disable<IMultiTenant>())
            {
                cameras = await _repositoryCamera.GetListAsync(x => x.GarageId == GarageID);
            }

        }
        else
        {
            //if organization admin
            cameras = await _repositoryCamera.GetListAsync(x => x.GarageId == GarageID);

        }

        int totalActive = 0;
        var oldTenantId = cameras.Count>0? cameras[0].TenantId:null;

        var organizationId = await _organizationsAppService.GetOrganizationIdByTenant(TenantId);
        Organization NewOrganization = new Organization();
        Garage NewGarage = new Garage();

        using (DataFilter.Disable<IMultiTenant>())
        {
            NewOrganization = await _organizationRepository.GetAsync(organizationId);
            NewGarage = await _repository.GetAsync(GarageID);

        }

        



        //cahnge cameras from old tenant to new
		foreach (var camera in cameras )
		{
            camera.DeviceId = await GenerateCameraCode(NewOrganization.EnglishName,NewGarage.NameEn,camera.Name);
            // Update the TenantId
			camera.TenantId = TenantId;
            camera.GarageId= GarageID;

            if(camera.Status == Enums.Status.Active)
            {
              totalActive++ ;

            }
		}


        if (CurrentTenant.Id == null)
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var flag =await AddActiveCameraRelatedToOrganizationGarage(GarageID, totalActive);
                if (!flag) {
                    throw new Exception("Failed to change assign");
                }
                else if(oldTenantId != null) 
                {
					var orgId = await _organizationsAppService.GetOrganizationIdByTenant(oldTenantId);
					var oldorg = await _organizationRepository.FindAsync(orgId);
                    if (oldorg != null) { 
                        oldorg.ActiveCameras-=totalActive;
                        oldorg.ActiveGarages--;
						await CurrentUnitOfWork.SaveChangesAsync();
					}
                    await _repositoryCamera.UpdateManyAsync(cameras);

                    return true;
                }
            }  
            
        }


		if (NewOrganization.Subscription.NumberOfCameras < NewOrganization.ActiveCameras + totalActive)
		{
			throw new Exception("UPGRADE subscription to add cameras to organization.");
		}
		NewOrganization.ActiveCameras += totalActive;
		if (NewOrganization.Subscription.NumbersOfGarages < NewOrganization.ActiveGarages + 1)
		{
			throw new Exception("UPGRADE subscription to add garage to organization.");

		}
		NewOrganization.ActiveGarages++;
		NewGarage.OrganizationId = NewOrganization.Id;
        NewGarage.TenantId = TenantId;
		await CurrentUnitOfWork.SaveChangesAsync();


		await _repositoryCamera.UpdateManyAsync(cameras);
        return true;

	}
    public async Task<string> GenerateCameraCode(string OrgEnglishName, string GarageEnglishName, string CameraEnglishName)
    {
        string Code = OrgEnglishName.Substring(0, 3) + '-' + GarageEnglishName.Substring(0, 2) + '-' + CameraEnglishName.Substring(0, 2)+'-';
        string ncode = Code;

        Random rnd = new Random();
        while (true)
        {
            ncode = Code;
            Camera camera = new Camera();
            ncode += rnd.Next(00, 99).ToString("00");
            using (DataFilter.Disable<IMultiTenant>())
            {
                camera = await _repositoryCamera.FirstOrDefaultAsync(x => x.DeviceId == ncode);

            }
            if (camera == null)
            {
                Code = ncode;
                break;
            }

        }
        return Code;

    }

    public override async Task<GarageDto> GetAsync(int id)
    {
        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {

            using (DataFilter.Disable<IMultiTenant>())
            {
                return await base.GetAsync(id);
            }

        }
        return await base.GetAsync(id);

    }
	/// <inheritdoc/>

	public async Task<bool> AddActiveCameraRelatedToOrganizationGarage(int GarageId, int NumOfActiveCamera)
	{
        var garage= await GetAsync(GarageId);
        try
        {
            var org = await _organizationRepository.GetAsync(garage.OrganizationId);

            if (org.Subscription.NumberOfCameras < org.ActiveCameras + NumOfActiveCamera)
            {
                throw new Exception("UPGRADE subscription to add cameras to organization.");
            }
            org.ActiveCameras += NumOfActiveCamera;
            if (org.Subscription.NumbersOfGarages < org.ActiveGarages + 1)
            {
                throw new Exception("UPGRADE subscription to add garage to organization.");

            }
            org.ActiveGarages++;
            await CurrentUnitOfWork.SaveChangesAsync();
            return true;

		}
		catch (Exception ex) {

            Console.WriteLine(ex.Message);
            return false;
        
        }
	}

    public async Task<Tuple<int,int>> GetMaxCapacityofActiveGaragesAndItsCount()
    {
        using (DataFilter.Disable<IMultiTenant>()) {
            var query =await _repository.GetQueryableAsync();
            query=query.Where(x=>x.Status==Enums.Status.Active);
            var count=await AsyncExecuter.CountAsync(query);
            int maxcapacity = query.Sum(x => x.MaxCapacity);
            return new Tuple<int,int>(maxcapacity, count);

        }

    }
}
