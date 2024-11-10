using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Subscriptions;
using SmartParking.Enums;
using SmartParking.Garages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;


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
    private readonly ICurrentUser _currentUser; // Inject ICurrentUser


	public CamerasAppService(IRepository<Camera, int> repository, IRepository<Subscription, int> subscriptionRepository, IDataFilter dataFilter, ICurrentUser currentUser, IRepository<Organization, int> organizationRepository = null, IGarageAppService garageAppService = null)
		: base(repository)
	{
		_repository = repository;
		_dataFilter = dataFilter;
		_currentUser = currentUser;
		_organizationRepository = organizationRepository;
		_garageAppService = garageAppService;
        _subscriptionRepository = subscriptionRepository;
    }


	public override async Task<PagedResultDto<CameraDto>> GetListAsync(PagedAndSortedResultRequestDto input)
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
    public override async Task<CameraDto> UpdateAsync(int id, CreateUpdateCameraDto input)
    {
        var garage = await _garageAppService.GetAsync(input.GarageId);
        var organization = await _organizationRepository.GetAsync(garage.OrganizationId);
        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                if (input.Status == Enums.Status.Active)
                {
                    await _garageAppService.AddActiveCameraRelatedToOrganizationGarage(input.GarageId,1);

				}
                return await base.UpdateAsync(id, input);
            }
        }
		if (input.Status == Enums.Status.Active)
        {
			await _garageAppService.AddActiveCameraRelatedToOrganizationGarage(input.GarageId, 1);

        }
       input.DeviceId = await GenerateCameraCode(organization.EnglishName,garage.NameEn,input.Name);
        input.OrganizationId = organization.Id;
        return await base.UpdateAsync(id, input);
    }

    public async Task<string> GenerateCameraCode(string OrgEnglishName ,string GarageEnglishName,string CameraEnglishName)
    {
       string Code = OrgEnglishName.Substring(0, 3) +'-'+ GarageEnglishName.Substring(0, 2) + '-' + CameraEnglishName.Substring(0, 2)+'-';
        string ncode = Code;

        Random rnd = new Random();
        while (true)
        {
            ncode=Code;
            Camera camera = new Camera();
            ncode += rnd.Next(00, 99).ToString("00");
            using (DataFilter.Disable<IMultiTenant>())
            {
                camera = await _repository.FirstOrDefaultAsync(x => x.DeviceId == ncode);

            }
            if (camera == null)
            {
                Code = ncode;
                break;
            }

        }
        return Code;

    }

        public override async Task<CameraDto> GetAsync(int id)
        {

        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
            {
                using (_dataFilter.Disable<IMultiTenant>())
                {
                    return await base.GetAsync(id);
                }
            }
            else
                return await base.GetAsync(id);
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
            var camera = await GetAsync(id);
            if (camera == null)
                throw new EntityNotFoundException(typeof(Camera), id);
            if (camera.Status == Enums.Status.Active) { 
        
		        await _garageAppService.AddActiveCameraRelatedToOrganizationGarage(camera.GarageId, -1);
            }
       
            camera.Status = Status.Deleted;
      
            await CurrentUnitOfWork.SaveChangesAsync();
             //await base.DeleteAsync(id);
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
            GarageDto garage = new GarageDto();
            Organization organization = new Organization();

        using (DataFilter.Disable<IMultiTenant>())
        {
            garage = await _garageAppService.GetAsync(input.GarageId);
            organization = await _organizationRepository.GetAsync(garage.OrganizationId);



            if (await CheckIPUnique(input.CameraIP) == 1)
            {
                throw new UserFriendlyException("The Camera IP is already in use.")
                {
                    Code = "DuplicateCameraIP"
                };
            }
            var numberOfCamerasForOrganization = await _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId);
            var subscription = await _subscriptionRepository.FirstOrDefaultAsync(s => s.Id == organization.SubscriptionId);
            if (subscription.NumbersOfGarages < numberOfCamerasForOrganization.Count + 1)
            {
                throw new Exception("UPGRADE subscription to add camera to organization.");

            }
            if (input.Status == Enums.Status.Active)
            {

                await _garageAppService.AddActiveCameraRelatedToOrganizationGarage(input.GarageId, 1);
            }
            input.DeviceId = await GenerateCameraCode(organization.EnglishName, garage.NameEn, input.Name);
            input.OrganizationId = organization.Id;
            return await base.CreateAsync(input);
        }
    }
        /// <summary>
        /// method used to Check if IP is unqiue or not
        /// </summary>
        /// <param name="cameraIP">Added Camera IP</param>
        /// <returns>1 if IP Exists</returns>
        private async Task<int> CheckIPUnique(string cameraIP)
        {
            // Use FirstOrDefaultAsync to find the camera by IP
            var camera = await _repository.FirstOrDefaultAsync(x => x.CameraIP.Equals(cameraIP));

        // If a camera is found, return 1 (or any positive number to indicate it's not unique)
        if (camera != null)
        {
            return 1; // CameraIP exists
        }

            // Return 0 if the CameraIP is unique (not found)
            return -1;
        }
        /// <summary>
        /// method used to get list of cameraes
        /// </summary>
        /// <param name="input">specifies max number of camera and if it sorted or not</param>
        /// <returns>cameraDto List</returns>
        public async Task<PagedResultDto<CameraDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
        {
            // Get the IQueryable<camera> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply search filtering

            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {

                queryable = queryable.Where(or =>
                    or.Name.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.Model.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.DeviceId.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.MainType.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.BrandName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.CameraIP.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.MacAddress.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.Garage.NameAr.ToLower().Contains(input.SearchingTerm.ToLower())||
                    or.Garage.NameEn.ToLower().Contains(input.SearchingTerm.ToLower())
                    
                );
            }
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of camera entities
        var cameraes = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to cameraDto objects
        var cameraesDto = ObjectMapper.Map<List<Camera>, List<CameraDto>>(cameraes);
        // Return the paginated result
        return new PagedResultDto<CameraDto>(
            totalCount,
            cameraesDto
        );
    }

    /// <summary>
    /// method used to get list of cameraes
    /// </summary>
    /// <param name="input">specifies max number of camera and if it filtered or not</param>
    /// <returns>cameraeDto List</returns>
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
}
