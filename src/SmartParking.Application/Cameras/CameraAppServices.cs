using SmartParking.Entities.Cameras;
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

namespace SmartParking.Cameras
{
    public class CameraAppServices:
        CrudAppService<
            Camera,
            CameraDto,
            int,
            PagedAndSortedResultRequestDto,
            CreateUpdateCameraDto
            >,
        ICameraAppService
    {
        private readonly IRepository<Camera, int> _repository;
        private IDataFilter _dataFilter;
        private readonly ICurrentUser _currentUser; // Inject ICurrentUser

        public CameraAppServices(IRepository<Camera, int> repository,IDataFilter dataFilter, ICurrentUser currentUser)
            : base(repository)
        {
            _repository = repository;
            _dataFilter = dataFilter;
            _currentUser = currentUser;
        }
        public override async Task<PagedResultDto<CameraDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            if(_currentUser.IsInRole("admin"))
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
            if(_currentUser.IsInRole("admin"))
            {
                using (_dataFilter.Disable<IMultiTenant>())
                {
                    return await base.UpdateAsync(id,input);
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

        public override async Task DeleteAsync(int id)
        {
            var camera=await GetAsync(id);
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
    }

}
