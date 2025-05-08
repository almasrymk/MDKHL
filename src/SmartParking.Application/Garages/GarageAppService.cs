using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Subscriptions;
using SmartParking.Enums;
using SmartParking.Filters;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace SmartParking.Garages;

[Authorize]
public class GarageAppService : CrudAppService<Garage, GarageDto, int, PagedAndSortedResultRequestDto, CreateUpdateGarageDto>, IGarageAppService
{
    private readonly IRepository<Garage,int> _repository;
    private readonly IRepository<Camera,int> _repositoryCamera;
    private readonly IRepository<Organization,int> _organizationRepository;
    private readonly IRepository<Subscription, int> _subscriptionRepository;
    private readonly OrganizationsAppService _organizationsAppService;
    private IDataFilter _dataFilter;
    private ICurrentUser _currentUser;
    public ICurrentTenant _currentTenant;
    public GarageAppService(IRepository<Garage, int> repository, OrganizationsAppService organizationsAppService, IDataFilter datafilter, ICurrentUser currentUser, ICurrentTenant currentTenant, IRepository<Camera, int> repositoryCamera, IRepository<Organization, int> organizationRepository,IRepository<Subscription,int> subscriptionRepository) : base(repository)
    {
        _repository = repository;
        _organizationsAppService = organizationsAppService;
        _dataFilter = datafilter;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _repositoryCamera = repositoryCamera;
        _organizationRepository = organizationRepository;
        _subscriptionRepository = subscriptionRepository;
    }
    /// <inheritdoc/>
    public override async Task<GarageDto> CreateAsync(CreateUpdateGarageDto input)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var organization = await GetAndValidateOrganizationAsync(input.OrganizationId);
            ValidateSlotCapacity(input);
            if(input.Status == Status.Active)
            {
                await CheckSubscriptionGarageLimitAsync(organization, input);
                await CheckSubscriptionLimitsAsync(organization, input, organization.UsedSlots);
                UpdateOrganizationUsage(organization, input);
            }
            input.Code =await GenerateGarageCode(organization, input);
            input.TenantId = organization.TenantId;
            return await base.CreateAsync(input);
        }
    }
    [RemoteService(isEnabled: false)]

    private async Task<Organization> GetAndValidateOrganizationAsync(int organizationId)
    {
        var query = await _organizationRepository.GetQueryableAsync();
        var organization = query
            .Where(x => x.Id == organizationId)
            .Include(x => x.Subscription)
            .FirstOrDefault();

        if (organization == null)
            throw new EntityNotFoundException(typeof(Organization), organizationId);

        if (organization.Status != Enums.Status.Active)
            throw new Exception(LocalizationHelper.GetLocalizedString("InActive")+ LocalizationHelper.GetLocalizedString("Organization"));

        return organization;
    }
    [RemoteService(isEnabled: false)]

    private void ValidateSlotCapacity(CreateUpdateGarageDto input)
    {
        if (input.MaxCapacity != input.MaxSlotsForCustomers + input.MaxSlotsForEmployess + input.MaxSlotsForFree)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("SummationValidation") +' '+ LocalizationHelper.GetLocalizedString("MaxCapacity"));
        }
    }
    [RemoteService(isEnabled: false)]
    private async Task<Subscription> CheckSubscriptionLimitsAsync(Organization organization, CreateUpdateGarageDto input , int usedSots)
    {
        var subscription = await _subscriptionRepository.FirstOrDefaultAsync(s => s.Id == organization.SubscriptionId);
        var numberOfGaragesForOrganization = await _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId && x.Status == Enums.Status.Active);
        if (subscription == null ||  (usedSots + input.MaxCapacity > subscription.ParkingSlot || input.MaxCapacity > subscription.ParkingSlot))
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidation") + input.MaxCapacity + LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidationUpdate") + (subscription.ParkingSlot - usedSots));
        }
        return subscription;
    }
    private async Task<Subscription> CheckSubscriptionGarageLimitAsync(Organization organization, CreateUpdateGarageDto input)
    {
        var subscription = await _subscriptionRepository.FirstOrDefaultAsync(s => s.Id == organization.SubscriptionId);
        var numberOfGaragesForOrganization = await _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId && x.Status == Enums.Status.Active);

        if (subscription == null || subscription.NumbersOfGarages < numberOfGaragesForOrganization.Count + 1)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("AddNewGarageValidation"));
        }
        return subscription;
    }
    [RemoteService(isEnabled: false)]
    private async Task<bool> CheckSubscriptionLimitsCamerasAsync(Organization organization,int camerasCout , Subscription subscription)
    {
         if (organization.ActiveCameras + camerasCout > subscription.NumberOfCameras)
         {
             throw new Exception(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidation"));
         }
        return true; 
    }
    [RemoteService(isEnabled: false)]

    private async Task<string> GenerateGarageCode(Organization organization, CreateUpdateGarageDto input)
    {
        var numberOfGaragesForOrganization = await _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId);
        return organization.Code + GetOrganizationCode(numberOfGaragesForOrganization.Count);
    }
    [RemoteService(isEnabled: false)]

    private void UpdateOrganizationUsage(Organization organization, CreateUpdateGarageDto input)
    {
        if (input.Status == Enums.Status.Active)
        {
            organization.UsedSlots += input.MaxCapacity;
            organization.ActiveGarages++;
        }
    }
    private void UpdateOrganizationUsageDelete(Organization organization, int maxcapacity)
    {
        organization.UsedSlots -= maxcapacity;    
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
            throw new BusinessException(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidation"));
        }
        string Code = "-G-";
        if (input < 9)
        {
            Code += '0';
        }
        Code += (input + 1);
        return Code;
    }
    public async Task<List<Camera>> GetCamerasRelatedToGarageAsync(Garage garage)
    {   
            var cameras = await _repositoryCamera.GetListAsync(x => x.GarageId == garage.Id);
            return cameras;
    }
    /// <inheritdoc/>
    public async override Task<GarageDto> UpdateAsync(int id, CreateUpdateGarageDto input)
    {
        Organization organization;
        Garage garage;
        ValidateSlotCapacity(input);
        using (DataFilter.Disable<IMultiTenant>())
        {
            organization = await GetAndValidateOrganizationAsync(input.OrganizationId);
            garage = await GetAndValidateGarageAsync(id);
            input.TenantId = organization.TenantId;
            await UpdateGarageDetailsAsync(input, organization, garage);
        }
        await CurrentUnitOfWork.SaveChangesAsync();
        return await UpdateGarageAsync(id, input);
    }
    [RemoteService(isEnabled: false)]
    private async Task<Garage> GetAndValidateGarageAsync(int id)
    {
        var garage = await _repository.GetAsync(id);
        if (garage == null)
            throw new EntityNotFoundException(typeof(Garage), id);

        return garage;
    }

    [RemoteService(isEnabled: false)]
    private async Task ChangeCameraStatusRelatedToGarage(Garage Garage,List<Camera> cameras)
    {
        foreach (var camera in cameras) { 
            camera.Status=Garage.Status;
        }   
    }
    [RemoteService(isEnabled: false)]
    private async Task UpdateGarageDetailsAsync(CreateUpdateGarageDto input, Organization organization ,  Garage existingGarage)
    {
        await ((input.Status, existingGarage.Status) switch
        {
            (Status.Inactive, Status.Active) => UpdateGarageWhenNewInActiveOldActive(organization, existingGarage),
            (Status.Active, Status.Inactive) => UpdateGarageWhenNewActiveOldInActive(input, organization, existingGarage),
            (Status.Active, Status.Active) => UpdateGarageWhenNewOldActive(input, organization, existingGarage)
        });
    }
    private async Task UpdateGarageWhenNewOldActive(CreateUpdateGarageDto input, Organization organization, Garage existingGarage)
    {
        int usedSlots = organization.UsedSlots - existingGarage.MaxCapacity;
        var subscription = await CheckSubscriptionLimitsAsync(organization, input, usedSlots);
        organization.UsedSlots -= existingGarage.MaxCapacity;
        organization.UsedSlots += input.MaxCapacity;
    }

    private async Task UpdateGarageWhenNewActiveOldInActive(CreateUpdateGarageDto input, Organization organization, Garage existingGarage)
    {
        await CheckSubscriptionGarageLimitAsync(organization, input);
        existingGarage.Status = Enums.Status.Active;
        var subscription = await CheckSubscriptionLimitsAsync(organization, input, organization.UsedSlots);
        var cameras = await GetCamerasRelatedToGarageAsync(existingGarage);
        var result = CheckSubscriptionLimitsCamerasAsync(organization, cameras.Count, subscription);
        await ChangeCameraStatusRelatedToGarage(existingGarage, cameras);
        organization.ActiveGarages++;
        organization.ActiveCameras += cameras.Count;
        organization.UsedSlots += input.MaxCapacity;
    }

    private async Task UpdateGarageWhenNewInActiveOldActive(Organization organization, Garage existingGarage)
    {
        existingGarage.Status = Enums.Status.Inactive;
        organization.ActiveGarages--;
        organization.UsedSlots -= existingGarage.MaxCapacity;
        existingGarage.CurrnetCapacityEmployees = existingGarage.CurrnetCapacityForFree = existingGarage.CurrnetCapacityForCustomers  = 0;
        var cameras = await GetCamerasRelatedToGarageAsync(existingGarage);
        await ChangeCameraStatusRelatedToGarage(existingGarage, cameras);
        organization.ActiveCameras -= cameras.Count;
    }
    [RemoteService(isEnabled: false)]
    private async Task<GarageDto> UpdateGarageAsync(int id, CreateUpdateGarageDto input)
    {
        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {
            using (DataFilter.Disable<IMultiTenant>())
            {
                return await base.UpdateAsync(id, input);
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
				or.MaxCapacity.ToString().ToLower().Contains(input.SearchingTerm.ToLower())||
				or.Status.ToString().ToLower().Contains(input.SearchingTerm.ToLower())  
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
        // Return the paginated result
        return new PagedResultDto<GarageDto>(
            totalCount,
            garagesDto
        );
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<GarageDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            // Get the IQueryable<garage> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply search filtering
            queryable = ApplySearch(input, queryable);
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
    }

    private static IQueryable<Garage> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Garage> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.Code.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.NameEn.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.NameAr.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Organization.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Organization.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.MaxCapacity.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Status.ToString().ToLower().Contains(input.SearchingTerm.ToLower())
            );
        }
        return queryable;
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
    public async Task<PagedResultDto<GarageDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _repository.GetQueryableAsync();
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
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<GarageDto>> GetGarageByOrganization(PagedAndSortedResultRequestDto input ,int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
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
    }

    /// <inheritdoc/>
    public async override Task<PagedResultDto<GarageDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        if (_currentTenant.Id==null)
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
        if (!CurrentTenant.Id.HasValue)
        {
            using (DataFilter.Disable<IMultiTenant>())
            {
                Garage garage = await _repository.FirstOrDefaultAsync(x => x.Id == id);
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
                UpdateOrganizationUsageDelete(org,garage.MaxCapacity);
                await _repository.HardDeleteAsync(garage);
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
                    return await HandleUpdateForOldTenant(cameras, totalActive, oldTenantId);
                }
            }   
        }
		if (NewOrganization.Subscription.NumberOfCameras < NewOrganization.ActiveCameras + totalActive)
		{
			throw new Exception(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidation"));
		}
		NewOrganization.ActiveCameras += totalActive;
		if (NewOrganization.Subscription.NumbersOfGarages < NewOrganization.ActiveGarages + 1)
		{
            throw new Exception(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidation"));
        }
        NewOrganization.ActiveGarages++;
		NewGarage.OrganizationId = NewOrganization.Id;
        NewGarage.TenantId = TenantId;
		await CurrentUnitOfWork.SaveChangesAsync();
		await _repositoryCamera.UpdateManyAsync(cameras);
        return true;
	}

    private async Task<bool> HandleUpdateForOldTenant(List<Camera> cameras, int totalActive, Guid? oldTenantId)
    {
        var orgId = await _organizationsAppService.GetOrganizationIdByTenant(oldTenantId);
        var oldorg = await _organizationRepository.FindAsync(orgId);
        if (oldorg != null)
        {
            oldorg.ActiveCameras -= totalActive;
            oldorg.ActiveGarages--;
            await CurrentUnitOfWork.SaveChangesAsync();
        }
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
        if (CurrentTenant.Id==null)
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
        try
        {
            using (DataFilter.Disable<IMultiTenant>())
            {
                var garage= await GetAsync(GarageId);
                var query = await _organizationRepository.GetQueryableAsync();
                query = query.Include(x => x.Subscription);
                var org = await query.FirstOrDefaultAsync(x => x.Id == garage.OrganizationId);
                if (org == null || org.Subscription == null) 
                    throw new Exception(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidation"));
               
                if (org.Subscription.NumberOfCameras < org.ActiveCameras + NumOfActiveCamera)
                    throw new Exception(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidation"));

                org.ActiveCameras += NumOfActiveCamera;
                if (org.Subscription.NumbersOfGarages < org.ActiveGarages + 1)
                    throw new Exception(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidation"));
                
                if (NumOfActiveCamera > 0)
                    org.ActiveGarages++;
           
                await CurrentUnitOfWork.SaveChangesAsync();
                return true;
            }
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

    public async Task<PagedResultDto<GarageDto>> GetListOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        queryable = ApplySearch(input, queryable);
        queryable = ApplySort(input, queryable);
        var totalCount = await AsyncExecuter.CountAsync(queryable);
        List<Garage> garages = await ApplyPaged(input, queryable);
        var garagesDto = ObjectMapper.Map<List<Garage>, List<GarageDto>>(garages);
        Console.WriteLine(garagesDto.Count);
        return new PagedResultDto<GarageDto>(
            totalCount,
            garagesDto
        );
    }

    private async Task<List<Garage>> ApplyPaged(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Garage> queryable)
    {
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var garages = await AsyncExecuter.ToListAsync(queryable);
        return garages;
    }

    private static IQueryable<Garage> ApplySort(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Garage> queryable)
    {
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

        return queryable;
    }

    public async Task<PagedResultDto<GarageDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input,Guid? tenantId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _repository.GetQueryableAsync();
            queryable = ApplyFilterTenant(tenantId, queryable);
            queryable = ApplySearch(input, queryable);
            queryable = ApplySort(input, queryable);
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            List<Garage> garages = await ApplyPaged(input, queryable);
            var garagesDto = ObjectMapper.Map<List<Garage>, List<GarageDto>>(garages);
            return new PagedResultDto<GarageDto>(
                totalCount,
                garagesDto
            );
        }
    }
    private static IQueryable<Garage> ApplyFilterTenant(Guid? tenantId, IQueryable<Garage> queryable)
    {
        if (tenantId.HasValue)
            queryable = queryable.Where(x => x.TenantId == tenantId);
        return queryable;
    }
}
