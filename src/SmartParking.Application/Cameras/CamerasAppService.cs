using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Subscriptions;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using System.Linq.Dynamic.Core;
using SmartParking.Filters;

namespace SmartParking.Cameras;

[Authorize]
public class CamerasAppService :
    CrudAppService<
        Camera,
        CameraDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateCameraDto
        >,
    ICamerasAppService
{
    private readonly IRepository<Camera, int> _repository;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IGarageAppService _garageAppService;
    private readonly IRepository<Subscription, int> _subscriptionRepository;
    private IDataFilter _dataFilter;
    public CamerasAppService(IRepository<Camera, int> repository, IRepository<Subscription, int> subscriptionRepository, IDataFilter dataFilter,  IRepository<Organization, int> organizationRepository = null, IGarageAppService garageAppService = null)
        : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
        _organizationRepository = organizationRepository;
        _garageAppService = garageAppService;
        _subscriptionRepository = subscriptionRepository;
    }

    #region public methods
    /// <summary>
    /// method gets list of cameras
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<CameraDto>> GetListOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        queryable = ApplySearch(input, queryable);
        queryable = ApplySort(input, queryable);
        var totalCount = await AsyncExecuter.CountAsync(queryable);
        List<Camera> cameraes = await ApplyPaged(input, queryable);
        var cameraesDto = ObjectMapper.Map<List<Camera>, List<CameraDto>>(cameraes);
        return new PagedResultDto<CameraDto>(
            totalCount,
            cameraesDto
        );
    }
   
    /// <summary>
    /// method gets list of cameras for mdkhl
    /// </summary>
    /// <param name="input"></param>
    /// <param name="tenantId"></param>
    /// <returns>PagedResultDto<CameraDto></returns>
    public async Task<PagedResultDto<CameraDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input,Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _repository.GetQueryableAsync();
            queryable = ApplyTenant(tenantId, queryable);
            queryable = ApplySearch(input, queryable);
            queryable = ApplySort(input, queryable);
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            List<Camera> cameraes = await ApplyPaged(input, queryable);
            var cameraesDto = ObjectMapper.Map<List<Camera>, List<CameraDto>>(cameraes);
            return new PagedResultDto<CameraDto>(
                totalCount,
                cameraesDto
            );
        }
    }

    /// <summary>
    /// update camera 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns>CameraDto</returns>
    /// <exception cref="Exception"></exception>
    public override async Task<CameraDto> UpdateAsync(int id, CreateUpdateCameraDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var garage = await _garageAppService.GetAsync(input.GarageId);
            var organization = await _organizationRepository.FirstOrDefaultAsync(x => x.Id == garage.OrganizationId);
            var subscription = await _subscriptionRepository.FirstOrDefaultAsync(s => s.Id == organization.SubscriptionId);
            if (subscription == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidation"));
            }
            var existingCamera = await _repository.GetAsync(id);
            await ValidateUpdateInput(id, input, existingCamera);
            await CheckOldAndNewCameraStatus(input, organization, subscription, existingCamera);
            return await base.UpdateAsync(id, input);
        }
    }
    
    /// <inheritdoc/>
    public async Task<string> GenerateCameraCode(string OrgEnglishName, string GarageEnglishName, string CameraEnglishName)
    {
        string Code = OrgEnglishName.Substring(0, 3) + '-' + GarageEnglishName.Substring(0, 2) + '-' + CameraEnglishName.Substring(0, 2) + '-';
        return Code;
    }
    /// <summary>
    /// method to get number of cameras related to garage
    /// </summary>
    /// <param name="garageId"></param>
    /// <returns>string</returns>
    public async Task<string> GetNumbersOfCamerasRelatedToGarage(int garageId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query =await _repository.GetQueryableAsync();
            query=query.Where(x=>x.GarageId==garageId);
            var Count=await query.CountAsync();
            Count++;
            return Count.ToString();
        }
    }
    /// <summary>
    /// method to get camera by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>CameraDto</returns>
    public override async Task<CameraDto> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await base.GetAsync(id);
        }
    }
    /// <summary>
    /// methods that deletes camera.
    /// it takes camera id as parameter , checks if it exists
    /// if don't it throws an exception
    /// if exists it deletes it , changes its status to deleted
    /// </summary>
    /// <param name="id">camera id</param>
    /// <returns>string</returns>
    /// <exception cref="EntityNotFoundException">exception thrown when garage isn't exist</exception>
    public override async Task DeleteAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var camera = await _repository.GetAsync(id);
            if (camera == null)
                throw new EntityNotFoundException(typeof(Camera), id);
            if (camera.Status == Enums.Status.Active)
            {
                var result = await _garageAppService.AddActiveCameraRelatedToOrganizationGarage(camera.GarageId, -1);
                if (result == true)
                    camera.Status = Status.Deleted;
                else
                    throw new Exception("Can't delete camera");
            }
            await _repository.HardDeleteAsync(camera);
        }
    }
    /// <summary>
    /// Method that used to create new camera to specific organization
    /// </summary>
    /// <param name="input">input is createupdateCameraDto type
    /// holds data from presentation layer
    /// </param>
    /// <returns>Dto of created camera</returns>
    /// <exception cref="UserFriendlyException"> exception is returned when Camera IP is duplicate </exception>     
    public override async Task<CameraDto> CreateAsync(CreateUpdateCameraDto input)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            GarageDto garage = await _garageAppService.GetAsync(input.GarageId);
            Organization organization = await _organizationRepository.GetAsync(garage.OrganizationId);
            Subscription subscription = await _subscriptionRepository.FirstOrDefaultAsync(s => s.Id == organization.SubscriptionId);
            await ValidateCreateInput(input);
            var numberOfCamerasForOrganization = await GetNumbersOfCamerasRelatedToGarage(input.GarageId);
            if (input.Status == Enums.Status.Active)
            {
                var result = await CheckSubscriptionLimitsCamerasAsync(organization, 1, subscription);
                if (result)
                    organization.ActiveCameras++;
            }
            input =  await AssignCameraCode(input, garage, organization, numberOfCamerasForOrganization);
            await CurrentUnitOfWork.SaveChangesAsync();
            return await base.CreateAsync(input);
        }
    }
    
    /// <summary>
    /// method used to get list of cameraes
    /// </summary>
    /// <param name="input">specifies max number of camera and if it sorted or not</param>
    /// <returns>cameraDto List</returns>
    public async Task<PagedResultDto<CameraDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        queryable = ApplySearch(input, queryable);
        var totalCount = await AsyncExecuter.CountAsync(queryable);
        List<Camera> cameraes = await ApplyPaged(input, queryable);
        var cameraesDto = ObjectMapper.Map<List<Camera>, List<CameraDto>>(cameraes);
        return new PagedResultDto<CameraDto>(
            totalCount,
            cameraesDto
        );
    }

  
    /// <inheritdoc/>
    public async Task<PagedResultDto<CameraDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<camera> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<Camera>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);
        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of organizations entities
        var cameraes = await AsyncExecuter.ToListAsync(queryable);
        // Map the organizations entities to organizationsDto objects
        var cameraesDto = ObjectMapper.Map<List<Camera>, List<CameraDto>>(cameraes);
        // Return the paginated result
        return new PagedResultDto<CameraDto>(
            totalCount,
            cameraesDto
        );
    }
    ///<inheritdoc/>
    public async Task<PagedResultDto<CameraDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            // Get the IQueryable<camera> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply filtering
            var filterExpression = BuildExpression<Camera>.BuildFilterExpression(input.filters);
            if (filterExpression is not null)
            {
                queryable = queryable.Where(filterExpression);
            }
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            // Execute the query and get the list of organizations entities
            var cameraes = await AsyncExecuter.ToListAsync(queryable);
            // Map the organizations entities to organizationsDto objects
            var cameraesDto = ObjectMapper.Map<List<Camera>, List<CameraDto>>(cameraes);
            // Return the paginated result
            return new PagedResultDto<CameraDto>(
                totalCount,
                cameraesDto
            );
        }
    }
    /// <summary>
    /// method used to get camera by Camera Id
    /// </summary>
    /// <param name="cameraId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<CameraDto> GetCameraIPByCameraId(int cameraId)
    {
        var camera = await _repository.FindAsync(cameraId);
        if (camera == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("Camera") + " "+LocalizationHelper.GetLocalizedString("NotFoundValidation"));
        }
        return ObjectMapper.Map<Camera, CameraDto>(camera);
    }
    #endregion
    #region private methods
    /// <summary>
    /// this method applies sorting
    /// </summary>
    /// <param name="input"></param>
    /// <param name="queryable"></param>
    /// <returns></returns>
    private static IQueryable<Camera> ApplySort(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Camera> queryable)
    {
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
            }
            else
                queryable = queryable.OrderBy(input.Sorting);
        }
        return queryable;
    }
    /// <summary>
    /// method to check old and new camera status
    /// </summary>
    /// <param name="input"></param>
    /// <param name="organization"></param>
    /// <param name="subscription"></param>
    /// <param name="existingCamera"></param>
    /// <returns></returns>
    private async Task CheckOldAndNewCameraStatus(CreateUpdateCameraDto input, Organization? organization, Subscription? subscription, Camera existingCamera)
    {
        if (input.Status == Enums.Status.Active && existingCamera.Status != Enums.Status.Active)
        {
            var result = await CheckSubscriptionLimitsCamerasAsync(organization, 1, subscription);
            if (result)
                organization.ActiveCameras++;
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }

    /// <summary>
    /// method to validate update camera input 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <param name="existingCamera"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task ValidateUpdateInput(int id, CreateUpdateCameraDto input, Camera existingCamera)
    {
        if (await CheckIPUnique(input.CameraIP) == 1 && existingCamera.Id != id)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("CameraIP") + ' ' + LocalizationHelper.GetLocalizedString("DuplicateValidation"));
        }
        if (await CheckMacAddressUnique(input.MacAddress) == 1 && existingCamera.Id != id)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("MacAddress") + ' ' + LocalizationHelper.GetLocalizedString("DuplicateValidation"));
        }
    }
    /// <summary>
    /// method to apply paged to camera list 
    /// </summary>
    /// <param name="input"></param>
    /// <param name="queryable"></param>
    /// <returns></returns>
    private async Task<List<Camera>> ApplyPaged(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Camera> queryable)
    {
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var cameraes = await AsyncExecuter.ToListAsync(queryable);
        return cameraes;
    }
    /// <summary>
    /// method used to apply search
    /// </summary>
    /// <param name="input"></param>
    /// <param name="queryable"></param>
    /// <returns>IQueryable<Camera></returns>
    private static IQueryable<Camera> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Camera> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.Name.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.DeviceId.ToLower().Contains(input.SearchingTerm.ToLower()) |
                or.MacAddress.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Garage.NameAr.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Garage.NameEn.ToLower().Contains(input.SearchingTerm.ToLower())
            );
        }
        return queryable;
    }
    /// <summary>
    /// method used to assign camera code
    /// </summary>
    /// <param name="input"></param>
    /// <param name="garage"></param>
    /// <param name="organization"></param>
    /// <param name="numberOfCamerasForOrganization"></param>
    /// <returns>CreateUpdateCameraDto</returns>
    private async Task<CreateUpdateCameraDto> AssignCameraCode(CreateUpdateCameraDto input, GarageDto garage, Organization organization, string numberOfCamerasForOrganization)
    {
        input.DeviceId = await GenerateCameraCode(organization.EnglishName, garage.NameEn, input.Name);
        input.DeviceId += numberOfCamerasForOrganization;
        input.OrganizationId = organization.Id;
        return input;
    }

    /// <summary>
    /// method used to validate create input
    /// </summary>
    /// <param name="input"></param>
    private async Task ValidateCreateInput(CreateUpdateCameraDto input)
    {
        if (await CheckIPUnique(input.CameraIP) == 1)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("CameraIP") + ' ' + LocalizationHelper.GetLocalizedString("DuplicateValidation"));
        }
        if (await CheckMacAddressUnique(input.MacAddress) == 1)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("MacAddress") + ' ' + LocalizationHelper.GetLocalizedString("DuplicateValidation"));
        }
    }
    /// <summary>
    /// method to add tenant to query use for search 
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="queryable"></param>
    /// <returns>IQueryable<Camera></returns>
    private static IQueryable<Camera> ApplyTenant(Guid? tenantId, IQueryable<Camera> queryable)
    {
        if (tenantId.HasValue)
            queryable = queryable.Where(_ => _.TenantId == tenantId);
        return queryable;
    }
    /// <summary>
    /// method used to check if organization has enough cameras
    /// </summary>
    /// <param name="organization"></param>
    /// <param name="camerasCout"></param>
    /// <param name="subscription"></param>
    /// <returns>bool</returns>
    [RemoteService(isEnabled: false)]

    private async Task<bool> CheckSubscriptionLimitsCamerasAsync(Organization organization, int camerasCout, Subscription subscription)
    {
        if (organization.ActiveCameras + camerasCout > subscription.NumberOfCameras)
            throw new Exception(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidation"));
        return true;
    }
    /// <summary>
    /// method used to Check if IP is unqiue or not
    /// </summary>
    /// <param name="cameraIP">Added Camera IP</param>
    /// <returns>1 if IP Exists</returns>
    private async Task<int> CheckIPUnique(string cameraIP)
    {
        var camera = await _repository.FirstOrDefaultAsync(x => x.CameraIP.Equals(cameraIP));
        if (camera != null)
            return 1; // CameraIP exists
        return -1;
    }
    /// <summary>
    /// method used to Check if IP is unqiue or not
    /// </summary>
    /// <param name="cameraIP">Added Camera IP</param>
    /// <returns>1 if IP Exists</returns>
    private async Task<int> CheckMacAddressUnique(string MacAddress)
    {
        var camera = await _repository.FirstOrDefaultAsync(x => x.MacAddress.Equals(MacAddress));
        if (camera != null)
            return 1; // MacAddress exists
        return -1;
    }
    #endregion
}

