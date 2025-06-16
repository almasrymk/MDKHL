using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
using SmartParking.Entities.Cameras.Watchers;

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
    /// <inheritdoc/>
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
    /// <inheritdoc/>
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
    /// <inheritdoc/>
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
    /// <inheritdoc/>
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
    /// <inheritdoc/>
    public override async Task<CameraDto> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await base.GetAsync(id);
        }
    }
    /// <inheritdoc/>
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
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
    /// <inheritdoc/>
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
    /// Applies sorting to the camera query based on the input sorting field and direction.
    /// </summary>
    /// <param name="input">DTO containing sorting information.</param>
    /// <param name="queryable">Queryable list of cameras.</param>
    /// <returns>Sorted IQueryable of Camera.</returns>
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
            {
                queryable = queryable.OrderBy(input.Sorting);
            }
        }
        return queryable;
    }
    /// <summary>
    /// Checks if a camera is being activated and updates organization's active camera count accordingly.
    /// </summary>
    /// <param name="input">New camera input data.</param>
    /// <param name="organization">Related organization.</param>
    /// <param name="subscription">Organization's subscription.</param>
    /// <param name="existingCamera">Existing camera entity before update.</param>
    /// <returns>Task representing the async operation.</returns>
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
    /// Validates uniqueness of IP and MAC address when updating a camera.
    /// </summary>
    /// <param name="id">ID of the camera being updated.</param>
    /// <param name="input">Camera input data.</param>
    /// <param name="existingCamera">Existing camera entity.</param>
    /// <returns>Task representing the async operation.</returns>
    /// <exception cref="Exception">Thrown when duplicate IP or MAC address is detected.</exception>
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
    /// Applies paging (Skip/Take) to the camera list.
    /// </summary>
    /// <param name="input">Paging information.</param>
    /// <param name="queryable">Queryable list of cameras.</param>
    /// <returns>Paged list of cameras.</returns>
    private async Task<List<Camera>> ApplyPaged(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Camera> queryable)
    {
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var cameras = await AsyncExecuter.ToListAsync(queryable);
        return cameras;
    }
    /// <summary>
    /// Applies search filters to the camera query based on search term.
    /// </summary>
    /// <param name="input">Search term input.</param>
    /// <param name="queryable">Queryable list of cameras.</param>
    /// <returns>Filtered IQueryable of Camera.</returns>
    private static IQueryable<Camera> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Camera> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.Name.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.DeviceId.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.MacAddress.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Garage.NameAr.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Garage.NameEn.ToLower().Contains(input.SearchingTerm.ToLower())
            );
        }
        return queryable;
    }
    /// <summary>
    /// Assigns a generated camera code to the camera input based on organization and garage names.
    /// </summary>
    /// <param name="input">Camera DTO to assign code to.</param>
    /// <param name="garage">Garage information.</param>
    /// <param name="organization">Organization information.</param>
    /// <param name="numberOfCamerasForOrganization">Camera count suffix to append to code.</param>
    /// <returns>Camera DTO with assigned DeviceId.</returns>
    private async Task<CreateUpdateCameraDto> AssignCameraCode(CreateUpdateCameraDto input, GarageDto garage, Organization organization, string numberOfCamerasForOrganization)
    {
        input.DeviceId = await GenerateCameraCode(organization.EnglishName, garage.NameEn, input.Name);
        input.DeviceId += numberOfCamerasForOrganization;
        input.OrganizationId = organization.Id;
        return input;
    }
    /// <summary>
    /// Validates the camera input for create operation (ensures unique IP and MAC address).
    /// </summary>
    /// <param name="input">Camera creation input.</param>
    /// <returns>Task representing the async operation.</returns>
    /// <exception cref="Exception">Thrown when IP or MAC address already exists.</exception>
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
    /// Applies tenant filter to the camera query.
    /// </summary>
    /// <param name="tenantId">Optional tenant ID.</param>
    /// <param name="queryable">Queryable list of cameras.</param>
    /// <returns>Tenant-filtered IQueryable of Camera.</returns>
    private static IQueryable<Camera> ApplyTenant(Guid? tenantId, IQueryable<Camera> queryable)
    {
        if (tenantId.HasValue)
            queryable = queryable.Where(_ => _.TenantId == tenantId);
        return queryable;
    }
    /// <summary>
    /// Checks if the organization is allowed to add more active cameras based on its subscription.
    /// </summary>
    /// <param name="organization">Organization information.</param>
    /// <param name="camerasCount">Number of cameras to be added.</param>
    /// <param name="subscription">Organization's subscription details.</param>
    /// <returns>True if within limit; otherwise throws an exception.</returns>
    /// <exception cref="Exception">Thrown when subscription limit is exceeded.</exception>
    [RemoteService(isEnabled: false)]
    private async Task<bool> CheckSubscriptionLimitsCamerasAsync(Organization organization, int camerasCount, Subscription subscription)
    {
        if (organization.ActiveCameras + camerasCount > subscription.NumberOfCameras)
            throw new Exception(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidation"));
        return true;
    }
    /// <summary>
    /// Checks if a given camera IP is unique in the database.
    /// </summary>
    /// <param name="cameraIP">The IP address of the camera.</param>
    /// <returns>1 if IP exists; -1 if unique.</returns>
    private async Task<int> CheckIPUnique(string cameraIP)
    {
        var camera = await _repository.FirstOrDefaultAsync(x => x.CameraIP.Equals(cameraIP));
        return camera != null ? 1 : -1;
    }
    /// <summary>
    /// Checks if a given MAC address is unique in the database.
    /// </summary>
    /// <param name="MacAddress">The MAC address of the camera.</param>
    /// <returns>1 if MAC address exists; -1 if unique.</returns>
    private async Task<int> CheckMacAddressUnique(string MacAddress)
    {
        var camera = await _repository.FirstOrDefaultAsync(x => x.MacAddress.Equals(MacAddress));
        return camera != null ? 1 : -1;
    }
    #endregion
}

