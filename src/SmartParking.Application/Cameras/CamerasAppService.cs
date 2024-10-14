using SmartParking.Entities.Cameras;
using SmartParking.Enums;
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

namespace SmartParking.Cameras
{
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
        private IDataFilter _dataFilter;
        private readonly ICurrentUser _currentUser; // Inject ICurrentUser

        public CamerasAppService(IRepository<Camera, int> repository, IDataFilter dataFilter, ICurrentUser currentUser)
            : base(repository)
        {
            _repository = repository;
            _dataFilter = dataFilter;
            _currentUser = currentUser;
        }

        
        public override async Task<PagedResultDto<CameraDto>> GetListAsync(PagedAndSortedResultRequestDto input)
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
        public override async Task<CameraDto> UpdateAsync(int id, CreateUpdateCameraDto input)
        {
            if (_currentUser.IsInRole("admin"))
            {
                using (_dataFilter.Disable<IMultiTenant>())
                {
                    return await base.UpdateAsync(id, input);
                }
            }
            return await base.UpdateAsync(id, input);
        }

        public override async Task<CameraDto> GetAsync(int id)
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
            if (_currentUser.IsInRole("admin"))
            {
                using (_dataFilter.Disable<IMultiTenant>())
                {
                    await base.DeleteAsync(id);

                }
            }
            await base.DeleteAsync(id);
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
            if (await CheckIPUnique(input.CameraIP) == 1)
            {
                throw new UserFriendlyException("The Camera IP is already in use.")
                {
                    Code = "DuplicateCameraIP"
                };
            }
            if(input.Status!=Status.Active && input.Status != Status.Inactive && input.Status != Status.Deleted)
            {
                throw new InvalidOperationException("Invalid status value");
            }

            if (input.CameraType != CameraType.Entry && input.CameraType != CameraType.Exit )
            {
                throw new InvalidOperationException("Invalid status value");
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
                    or.Name.StartsWith(input.SearchingTerm) ||
                    or.Model.StartsWith(input.SearchingTerm) ||
                    or.Code.StartsWith(input.SearchingTerm) ||
                    or.MainType.StartsWith(input.SearchingTerm) ||
                    or.BrandName.StartsWith(input.SearchingTerm) ||
                    or.Code.StartsWith(input.SearchingTerm) ||
                    or.CameraIP.StartsWith(input.SearchingTerm) ||
                    or.MacAddress.StartsWith(input.SearchingTerm) 
                    //or.Type.StartsWith(input.SearchingTerm)
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

}
